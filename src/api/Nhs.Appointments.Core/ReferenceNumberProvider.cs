using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using LuhnNet;

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
    TimeProvider timeProvider)
    : IReferenceNumberProvider
{
    internal const int SequenceMax = 10_000_000;
    internal const int CoprimeStride = 7; // coprime with SequenceMax

    /// <summary>
    ///     Generate the booking reference number in a way that is future-proof, for the very long term (100 years)
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetReferenceNumber(byte[] hmacSecretKey)
    {
        var sequenceNumber = await bookingReferenceDocumentStore.GetNextSequenceNumber();
        var now = timeProvider.GetUtcNow();
        return Generate(sequenceNumber, now, hmacSecretKey);
    }

    /// <summary>
    ///     Partition key so that a different unique key is generated every 4 days
    ///     Lasts 100 years until a collision... Hello year 2125!
    /// </summary>
    /// <param name="dtUtc"></param>
    /// <returns></returns>
    private static string PartitionKey(DateTimeOffset dtUtc)
    {
        //divide total days into 4, result is always < 100, so can use 2 digits
        var dayPartition = (dtUtc.DayOfYear / 4) + 1;

        //i.e for 5th Jan 2025 will be 2502 (25 year, Jan 5th is within the 2nd partition of 4 days)
        return (dtUtc.Year % 100).ToString("D2") + dayPartition.ToString("D2");
    }

    private static string Generate(int sequenceNumber, DateTimeOffset utcNow, byte[] hmacSecretKey)
    {
        var overflowedSequenceNumber = sequenceNumber % SequenceMax;
        var partitionKey = PartitionKey(utcNow);
        var sequenceBijection = GenerateSequenceBijection(overflowedSequenceNumber, partitionKey, hmacSecretKey)
            .ToString("D7");
        var partitionAndSequenceBijection = partitionKey + sequenceBijection;

        //adds a simple check whether the booking reference is valid (all 12 digits have to satisfy a criteria)
        //this can be guessed with 10% chance as it is a single digit,
        //but can help us detect brute force checks before DB hit
        var checkDigit = Luhn.CalculateCheckDigit(partitionAndSequenceBijection).ToString("D1");

        return FormatBookingReference(partitionAndSequenceBijection +
                                      checkDigit); // 13 digits that match the desired checksum outcome
    }

    /// <summary>
    ///     A bijection is a 1-1 mapping between two sets of numbers.
    ///     This generation adds an element of seeming randomness to the sequence numbers generated, while still being
    ///     deterministic and avoiding collisions.
    ///     This method means all 10 million sequence numbers will each generate a different number in that range. (0-9999999)
    ///     Using a dynamically generated offset means this offset cannot be guessed, as it is generated using a secret key.
    ///     This is needed to protect against sequential attacks visible in the reference (people guessing the next booking is
    ///     exactly 1 higher)
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="partitionKey"></param>
    /// <param name="hmacSecretKey"></param>
    /// <returns></returns>
    private static int GenerateSequenceBijection(int sequence, string partitionKey, byte[] hmacSecretKey)
    {
        var offset = ComputeProtectedOffset(partitionKey, hmacSecretKey);
        
        //TODO does the coprime stride (and NOT the offset, need the protected generation)...?
        return (int)((((long)CoprimeStride * sequence) + offset) % SequenceMax);
    }

    /// <summary>
    ///     Computes the offset used for the bijection.
    ///     Generates the offset using the partitionKey and a secret key.
    ///     This doesn't have to generate unique results, it just needs to be deterministic and non-guessable and guarded.
    /// </summary>
    /// <param name="partitionKey"></param>
    /// <param name="hmacSecretKey"></param>
    /// <returns></returns>
    private static int ComputeProtectedOffset(string partitionKey, byte[] hmacSecretKey)
    {
        using var hmac = new HMACSHA256(hmacSecretKey);
        var hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(partitionKey));

        // Take first 8 bytes as unsigned 64-bit
        var val = BitConverter.ToUInt64(hash, 0);

        //result cannot be larger than sequence max
        return (int)(val % SequenceMax); // result: 0 … SequenceMax-1
    }

    /// <summary>
    ///     A quick way to check if the provided booking reference is valid,
    ///     before firing off to the DB to try and find a record with that identifier.
    ///     No valid records that fail this check should exist in the DB
    /// </summary>
    /// <param name="bookingReference"></param>
    /// <returns></returns>
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
        return raw?.Length != 12
            ? throw new ArgumentException("Must be 12 digits")
            : $"{raw[..4]}-{raw.Substring(4, 4)}-{raw.Substring(8, 4)}";
    }

    [GeneratedRegex(@"^\d{4}-\d{4}-\d{4}$")]
    private static partial Regex BookingReferenceRegex();
}
