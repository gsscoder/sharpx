// Based on https://docs.microsoft.com/en-us/archive/msdn-magazine/2007/september/net-matters-tales-from-the-cryptorandom.
#pragma warning disable 8602, 8618

using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace SharpX;

/// <summary>A thread safe random number generator based on the RNGCryptoServiceProvider.</summary>
[Obsolete("CryptoRandom is obsolete. To generate a random number, use one of the RandomNumberGenerator static methods instead.")]
public class CryptoRandom : Random
{
    readonly RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();
    byte[] _buffer;
    int _bufferPosition;

    /// <summary>Gets a value indicating whether this instance has random pool enabled.</summary>
    public bool IsRandomPoolEnabled { get; private set; }

    /// <summary>Initializes a new instance of the <c>CryptoRandom</c> class with. Using this
    /// overload will enable the random buffer pool.</summary>
    public CryptoRandom() : this(true) { }

    /// <summary>Initializes a new instance of the <c>CryptoRandom</c> class. This method will
    /// disregard whatever value is passed as seed and it's only implemented in order to be fully
    /// backwards compatible with <c>System.Random</c>. Using this overload will enable the random
    /// buffer pool.</summary>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "ignoredSeed",
        Justification = "Cannot remove this parameter as we implement the full API of System.Random")]
    public CryptoRandom(int seed) : this(true) { }

    /// <summary>Initializes a new instance of the <c>CryptoRandom</c> class with optional random
    /// buffer.</summary>
    public CryptoRandom(bool enableRandomPool)
    {
        IsRandomPoolEnabled = enableRandomPool;
    }

    void InitBuffer()
    {
        if (IsRandomPoolEnabled) {
            if (_buffer == null || _buffer.Length != 512) {
                _buffer = new byte[512];
            }
        }
        else {
            if (_buffer == null || _buffer.Length != 4) {
                _buffer = new byte[4];
            }
        }
        _rng.GetBytes(_buffer);
        _bufferPosition = 0;
    }

    /// <summary>Returns a non-negative random integer.</summary>
    public override int Next() =>
        // Mask away the sign bit so that we always return nonnegative integers
        (int)GetRandomUInt32() & 0x7FFFFFFF;

    /// <summary>Returns a non-negative random integer that is less than the specified
    /// maximum.</returns>
    public override int Next(int maxValue)
    {
        if (maxValue < 0) throw new ArgumentOutOfRangeException(nameof(maxValue));

        return Next(0, maxValue);
    }

    /// <summary>Returns a non-negative random integer that is within a specified range.</summary>
    public override int Next(int minValue, int maxValue)
    {
        if (minValue > maxValue) throw new ArgumentOutOfRangeException(nameof(minValue));
        if (minValue == maxValue) return minValue;

        long diff = maxValue - minValue;
        while (true) {
            uint rand = GetRandomUInt32();
            long max = 1 + (long)uint.MaxValue;
            long remainder = max % diff;
            if (rand < max - remainder) {
                return (int)(minValue + (rand % diff));
            }
        }
    }

    /// <summary>Returns a random floating-point number that is greater than or equal to 0.0, and
    /// less than 1.0.</summary>
    public override double NextDouble() => GetRandomUInt32() / (1.0 + uint.MaxValue);

    /// <summary>Fills the elements of a specified array of bytes with random numbers.</summary>
    public override void NextBytes(byte[] buffer)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));

        lock (this)
        {
            if (IsRandomPoolEnabled && _buffer == null) {
                InitBuffer();
            }
            // Can we fit the requested number of bytes in the buffer?
            if (IsRandomPoolEnabled && _buffer.Length <= buffer.Length)
            {
                int count = buffer.Length;
                EnsureRandomBuffer(count);
                Buffer.BlockCopy(_buffer, _bufferPosition, buffer, 0, count);
                _bufferPosition += count;
            }
            else {
                // Draw bytes directly from the RNGCryptoProvider
                _rng.GetBytes(buffer);
            }
        }
    }

    uint GetRandomUInt32()
    {
        lock (this) {
            EnsureRandomBuffer(4);
            uint rand = BitConverter.ToUInt32(_buffer, _bufferPosition);
            _bufferPosition += 4;
            return rand;
        }
    }

    void EnsureRandomBuffer(int requiredBytes)
    {
        if (_buffer == null) {
            InitBuffer();
        }

        if (requiredBytes > _buffer.Length) throw new ArgumentOutOfRangeException(nameof(requiredBytes),
            "Cannot be greater than random buffer.");

        if ((_buffer.Length - _bufferPosition) < requiredBytes) {
            InitBuffer();
        }
    }
}
