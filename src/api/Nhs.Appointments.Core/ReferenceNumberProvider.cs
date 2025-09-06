using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using LuhnNet;
using Microsoft.Extensions.Caching.Memory;

namespace Nhs.Appointments.Core;

public interface IReferenceNumberProvider
{
    Task<string> GetReferenceNumber(byte[] hmacSecretKey);
}

public interface IBookingReferenceDocumentStore
{
    Task<int> GetNextSequenceNumber();
}

public partial class ReferenceNumberProvider(
    IBookingReferenceDocumentStore bookingReferenceDocumentStore,
    IMemoryCache memoryCache,
    TimeProvider timeProvider)
    : IReferenceNumberProvider
{
    //hard-coded for MVP, potentially need keyVersion mapping to secret is ever up-versioned.
    private const string HmacKeyVersion = "v1";
    
    //in generating the partition key, splitting the current day of the year into X buckets of this length
    private const int PartitionBucketLengthInDays = 4;
    
    //the sequence max that rolls over. system can support this many references within X many days
    //i.e SequenceMax = 100 million, and PartitionBucketLengthInDays = 4
    //means the system can generate 100 million unique references within 4 day span of a year
    internal const int SequenceMax = 100_000_000;

    /// <summary>
    ///     Generate a booking reference number in a way that is future-proof, non-sequential, seemingly randomised,
    ///     and avoids collisions even with high traffic in a short period of time,
    ///     Valid for the very long term (100 years)
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetReferenceNumber(byte[] hmacSecretKey)
    {
        var sequenceNumber = await bookingReferenceDocumentStore.GetNextSequenceNumber();
        var now = timeProvider.GetUtcNow();
        return Generate(sequenceNumber, now, hmacSecretKey);
    }

    /// <summary>
    ///     Partition key so that a different unique key is generated every 'PartitionBucketLengthInDays' days
    ///     Lasts 100 years until a collision... Hello year 2125!
    /// </summary>
    /// <param name="dtUtc"></param>
    /// <returns></returns>
    private static string PartitionKey(DateTimeOffset dtUtc)
    {
        //divide total days into bucket length
        var dayPartition = (dtUtc.DayOfYear / PartitionBucketLengthInDays) + 1;

        //i.e for 5th Jan 2025 will be 2502 (25 year, Jan 5th is within the 2nd partition of 4 days)
        return (dtUtc.Year % 100).ToString("D2") + dayPartition.ToString("D2");
    }

    private string Generate(int sequenceNumber, DateTimeOffset utcNow, byte[] hmacSecretKey)
    {
        var overflowedSequenceNumber = sequenceNumber % SequenceMax;
        var partitionKey = PartitionKey(utcNow);
        var sequenceBijection = GenerateSequenceBijection(overflowedSequenceNumber, partitionKey, hmacSecretKey)
            .ToString("D8");
        var partitionAndSequenceBijection = partitionKey + sequenceBijection;

        //adds a simple check whether the booking reference is valid (all 12 digits have to satisfy a criteria)
        //this can be guessed with 10% chance as it is a single digit,
        //but can help us detect brute force checks before DB hit
        var checkDigit = Luhn.CalculateCheckDigit(partitionAndSequenceBijection).ToString("D1");

        return FormatBookingReference(partitionAndSequenceBijection + checkDigit); // 13 digits that match the desired checksum outcome
    }

    /// <summary>
    ///     A bijection is a 1-1 mapping between two sets of numbers.
    ///     This generation adds an element of seeming randomness to the sequence numbers generated, while still being
    ///     deterministic and avoiding collisions.
    ///     This method means all 100 million sequence numbers will each generate a different and distinct number in that range. (0-99999999)
    ///     Using a dynamically generated stride means this cannot be guessed, as it is generated using a secret key and will change with each partition change.
    ///     This is needed to protect against sequential attacks visible in the reference (people guessing the next booking is
    ///     exactly 1 higher)
    /// </summary>
    private int GenerateSequenceBijection(int sequence, string partitionKey, byte[] hmacSecretKey)
    {
        var sequenceStride = GetSequenceStride(partitionKey, hmacSecretKey);
        
        // f(i) = ( S * i + O ) mod N : is a bijection on (0, ..., N-1)
        // when the stride (S) is coprime with N (i.e gcd(S,N) = 1), and O is a constant offset (set to zero for us as no benefit)
        return (int)((long)sequenceStride * sequence % SequenceMax);
    }

    private int GetSequenceStride(string partitionKey, byte[] hmacSecretKey)
    {
        var cacheKey = $"{HmacKeyVersion}:{partitionKey}";
        if (memoryCache.TryGetValue(cacheKey, out int sequenceStride))
        {
            return sequenceStride;
        }

        sequenceStride = DeriveSequenceStride(partitionKey, hmacSecretKey);
        memoryCache.Set(cacheKey, sequenceStride, TimeSpan.FromDays(PartitionBucketLengthInDays + 2)); // easily spans the partition length
        return sequenceStride;
    }

    /// <summary>
    ///     Derives the stride used for the bijection.
    ///     Generates a coprime stride (with SequenceMax) using the partitionKey and a secret key.
    ///     This doesn't have to generate unique results, two different partition keys can generate the same result
    ///     it just needs to be deterministic and non-guessable and guarded by a secret.
    /// </summary>
    /// <param name="partitionKey"></param>
    /// <param name="hmacSecretKey"></param>
    /// <returns></returns>
    private static int DeriveSequenceStride(string partitionKey, byte[] hmacSecretKey)
    {
        using var h = new HMACSHA256(hmacSecretKey);
        var mac = h.ComputeHash(Encoding.ASCII.GetBytes(partitionKey));
        var hmacStride = BitConverter.ToUInt64(mac, 0);

        // Base stride is (0,...,N-1)/10 - so multiplying by 10 still stays < N
        var baseStride = (int)(hmacStride % (SequenceMax / 10)); // 0,..., N-1

        //the following logic is dependent on SequenceMax being a number of format 10^x
        
        // Pick a last digit from {1,3,7,9} deterministically (since any number ending in any of these are coprime to 100 million (sequence max))
        int[] tail = [1, 3, 7, 9];
        
        //takes the last two bits of v (so a value 0–3) to pick the last digit deterministically
        var lastDigit = tail[(int)(hmacStride & 3)];

        var stride = (baseStride * 10) + lastDigit; // 0,...,N-1 with last digit ending in either {1,3,7,9}
        
        //confirmation of GCD(S,N) == 1, without this, the entire bijection pattern fails!!
        if ((int)BigInteger.GreatestCommonDivisor(stride, SequenceMax) != 1)
        {
            //fail fast - logically this should NEVER happen
            throw new InvalidOperationException($"CRITICAL ERROR - Derived sequence stride does not pass logical requirement of GCD(stride,SequenceMax) == 1. " +
                                                $"Failure for stride: '{stride}' and sequenceMax: '{SequenceMax}'");
        }
        
        //GCD(S,N) == 1 guaranteed
        return stride; 
    }

    /// <summary>
    ///     A quick way to check if the provided booking reference is valid,
    ///     before firing off to the DB to try and find a record with that identifier.
    ///     No valid records that fail this check should exist in the DB
    /// </summary>
    public static bool IsValidBookingReference(string bookingReference)
    {
        if (!BookingReferenceRegex().IsMatch(bookingReference))
        {
            //the provided string is not in the right format
            return false;
        }

        var digitReference = bookingReference.Replace("-", string.Empty);

        if (!Luhn.IsValid(digitReference))
        {
            // Logger.LogWarning
            // checksum fail indicates someone tried to guess a booking reference format using the valid format
            return false;
        }

        return true;
    }

    private static string FormatBookingReference(string raw)
    {
        return raw?.Length != 13
            ? throw new ArgumentException("Must be 13 digits")
            : $"{raw[..4]}-{raw.Substring(4, 5)}-{raw.Substring(9, 4)}";
    }

    [GeneratedRegex(@"^\d{4}-\d{5}-\d{4}$")]
    private static partial Regex BookingReferenceRegex();
}
