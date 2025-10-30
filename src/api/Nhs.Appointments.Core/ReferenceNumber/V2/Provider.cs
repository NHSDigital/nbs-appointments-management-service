using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using LuhnNet;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nhs.Appointments.Core.ReferenceNumber.V2;

public interface IProvider
{
    Task<string> GetReferenceNumber();

    bool IsValidBookingReference(string bookingReference);

    int DeriveSequenceMultiplier(string partitionKey);
}

public interface IBookingReferenceDocumentStore
{
    Task<int> GetNextSequenceNumber();
}

public class Provider(
    IOptions<ReferenceNumberOptions> options,
    IBookingReferenceDocumentStore bookingReferenceDocumentStore,
    IMemoryCache memoryCache,
    ILogger<Provider> logger,
    TimeProvider timeProvider)
    : IProvider
{
    //needed for backwards compatibility since we are now checking correctly formatted strings
    private Regex BookingReferenceV1Regex => new (@"^\d{2}-\d{2}-\d{6}$");
    
    private Regex BookingReferenceV2Regex => new (@"^\d{4}-\d{5}-\d{4}$");
    
    private IOptions<ReferenceNumberOptions> Options { get; } = options;

    //in generating the partition key, splitting the current day of the year into X buckets of this length
    internal const int PartitionBucketLengthInDays = 4;

    //the sequence max that rolls over. system can support this many references within X many days
    //i.e. SequenceMax = 100 million, and PartitionBucketLengthInDays = 4
    //means the system can generate 100 million unique references within 4 day span of a year
    internal const int SequenceMax = 100_000_000;

    /// <summary>
    ///     Generate a booking reference number in a way that is future-proof, non-sequential, seemingly randomised,
    ///     and avoids collisions even with high traffic in a short period of time,
    ///     Valid for the very long term (100 years)
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetReferenceNumber()
    {
        var sequenceNumber = await bookingReferenceDocumentStore.GetNextSequenceNumber();
        var now = timeProvider.GetUtcNow();
        return Generate(sequenceNumber, now);
    }

    /// <summary>
    ///     Partition key so that a different unique key is generated every 'PartitionBucketLengthInDays' days
    ///     Lasts 100 years until a collision... Hello year 2125!
    /// </summary>
    private static string PartitionKey(DateTimeOffset dtUtc)
    {
        //divide total days into bucket length
        var dayPartition = (dtUtc.DayOfYear / PartitionBucketLengthInDays) + 1;

        //i.e. on the 5th Jan 2025 will be 0225 (Jan 5th is within the 2nd partition of 4 days, 25 year part)
        return dayPartition.ToString("D2") + (dtUtc.Year % 100).ToString("D2");
    }

    private string Generate(int sequenceNumber, DateTimeOffset utcNow)
    {
        var overflowedSequenceNumber = sequenceNumber % SequenceMax;
        var partitionKey = PartitionKey(utcNow);
        var sequenceBijection = GenerateSequenceValue(overflowedSequenceNumber, partitionKey)
            .ToString("D8");
        var partitionAndSequenceBijection = partitionKey + sequenceBijection;

        //adds a simple check whether the booking reference is valid (all 13 digits have to satisfy the Luhn criteria)
        //this can be guessed with a 10% chance as it is a single digit.
        //but can help us detect brute force checks before hitting the DB.
        var checkDigit = Luhn.CalculateCheckDigit(partitionAndSequenceBijection).ToString("D1");

        return FormatBookingReference(partitionAndSequenceBijection + checkDigit); // 13 digits that match the desired checksum outcome
    }

    /// <summary>
    ///     This generation adds an element of seeming randomness to the sequence numbers generated, while still being
    ///     deterministic and avoiding collisions within 100 million sequence numbers (mod 100 million).
    ///     This method means all 100 million sequence numbers will each generate a different and distinct number in that range. (0-99999999)
    ///     Uses a dynamically generated sequence increment multiplier which means this cannot be inferred, as it is generated using a secret key and will change with each partition change.
    ///     This is needed to protect against sequential attacks visible in the reference numbers (i.e people guessing the next booking is
    ///     exactly 1 reference number higher)
    /// </summary>
    private int GenerateSequenceValue(int sequence, string partitionKey)
    {
        var sequenceMultiplier = GetSequenceMultiplier(partitionKey);

        // A bijection is a 1-1 mapping between two sets of numbers.
        // f(i) = ( S * i ) mod N : is a bijection on (0, ..., N-1) when the multiplier (S) is coprime with N (i.e gcd(S,N) = 1)
        
        return (int)((long)sequenceMultiplier * sequence % SequenceMax);
    }

    private int GetSequenceMultiplier(string partitionKey)
    {
        var cacheKey = $"{Options.Value.HmacKeyVersion}:{partitionKey}";
        if (memoryCache.TryGetValue(cacheKey, out int sequenceMultiplier))
        {
            return sequenceMultiplier;
        }

        sequenceMultiplier = DeriveSequenceMultiplier(partitionKey);
        memoryCache.Set(cacheKey, sequenceMultiplier, TimeSpan.FromDays(PartitionBucketLengthInDays + 2)); // easily spans the partition length
        return sequenceMultiplier;
    }

    /// <summary>
    ///     Derives the sequence multiplier used for the bijection.
    ///     Generates a sequence multiplier that is COPRIME with SequenceMax, using the partitionKey and a secret key, and some manual manipulation to guarantee the value is coprime.
    ///     This doesn't have to generate unique results, two different partition keys and secrets can generate the same result.
    ///     This just needs to be deterministic and non-guessable and guarded by a secret.
    ///     It is used for obfuscation of the sequencing, not for security.
    /// </summary>
    public int DeriveSequenceMultiplier(string partitionKey)
    {
        //generate a hmacMultiplier that uses both the partition key and the hmac secret key
        //this can be any 64-bit integer
        using var h = new HMACSHA256(Options.Value.HmacKey);
        var mac = h.ComputeHash(Encoding.ASCII.GetBytes(partitionKey));
        var hmacMultiplier = BitConverter.ToUInt64(mac, 0);

        //next make the hmacMultiplier be a value between 0 - 10 million (we're going to generate the final digit manually to make a number that is coprime with 100 million)
        var baseMultiplier = (int)(hmacMultiplier % (SequenceMax / 10));

        //the following logic is dependent on SequenceMax being a number of format 10^x

        // Pick a last digit from {1,3,7,9} deterministically (since any number ending in any of these are coprime to 100 million (sequence max))
        int[] lastDigitOptions = [1, 3, 7, 9];

        //takes the last two bits of hmacMultiplier (so a value 0–3) to pick the last digit deterministically
        var lastDigit = lastDigitOptions[(int)(hmacMultiplier & 3)];

        // 0,...,999,999,999 with the last digit guaranteed to end in either 1,3,7,9. (i.e 672,509,093 where 67250909 was the hmacMultiplier)
        var multiplier = (baseMultiplier * 10) + lastDigit; 

        //confirmation of GCD(multiplier,SequenceMax) == 1, without this, the entire bijection pattern fails!!
        if ((int)BigInteger.GreatestCommonDivisor(multiplier, SequenceMax) != 1)
        {
            var message =
                $"CRITICAL ERROR - Derived sequence multiplier does not pass logical requirement of GCD(multiplier,SequenceMax) == 1. " +
                $"Failure for multiplier: '{multiplier}' and sequenceMax: '{SequenceMax}'";            
            
            //fail fast - logically this should NEVER happen and something has gone VERY WRONG!
            logger.LogCritical(message);
            throw new InvalidOperationException(message);
        }

        //GCD(multiplier,SequenceMax) == 1 guaranteed
        return multiplier; 
    }

    /// <summary>
    ///     A quick way to check if the provided booking reference is valid,
    ///     before firing off to the DB to try and find a record with that identifier.
    ///     No valid records that fail this check should exist in the DB
    /// </summary>
    public bool IsValidBookingReference(string bookingReference)
    {
        //backwards compatibility with V1 bookings in flight
        if (BookingReferenceV1Regex.IsMatch(bookingReference))
        {
            //the provided string is in the right format for V1.
            //no further checks required
            return true;
        }
        
        if (!BookingReferenceV2Regex.IsMatch(bookingReference))
        {
            //the provided string is not in the right format
            return false;
        }

        var digitReference = bookingReference.Replace("-", string.Empty);

        if (!Luhn.IsValid(digitReference))
        {
            // checksum fail indicates someone may have tried to guess a booking reference format using the valid format
            // this could however also be a user typing error
            
            //if there are a lot of these warnings, could this suggest someone trying to brute force guess a valid reference...?
            logger.LogWarning("Booking Reference '{BookingReference}' does not pass the valid Luhn digit requirement.", bookingReference);
            return false;
        }

        return true;
    }

    internal static string FormatBookingReference(string raw)
    {
        return raw?.Length != 13
            ? throw new ArgumentException("Must be 13 digits")
            : $"{raw[..4]}-{raw.Substring(4, 5)}-{raw.Substring(9, 4)}";
    }
}
