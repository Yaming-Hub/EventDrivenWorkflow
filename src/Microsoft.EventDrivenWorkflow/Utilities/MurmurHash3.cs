using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Utilities
{
    /// <summary>
    /// This code is public domain. Reference source: http://blog.teamleadnet.com/2012/08/murmurhash3-ultra-fast-hash-algorithm.html
    /// Explanation :  https://en.wikipedia.org/wiki/MurmurHash
    /// Copied from /sources/dev/Pdp/src/PdpCommon/Helpers/MurmurHash3.cs with minor edits
    /// </summary>
    internal class MurmurHash3
    {
        /// <summary>
        /// The read size.
        /// </summary>
        private const ulong READSIZE = 16;

        /// <summary>
        /// C1 value.
        /// </summary>
        private const ulong C1 = 0x87c37b91114253d5L;

        /// <summary>
        /// C2 value.
        /// </summary>
        private const ulong C2 = 0x4cf5ad432745937fL;

        /// <summary>
        /// Seed value.
        /// </summary>
        private const uint Seed = 144;

        /// <summary>
        /// H1 value.
        /// </summary>
        private ulong h1;

        /// <summary>
        /// H2 value.
        /// </summary>
        private ulong h2;

        /// <summary>
        /// Generates the MurmurHash3 (128 bits) for the string 
        /// </summary>
        /// <param name="stringToEncode">String to encode</param>
        /// <returns>Hash for the string as a byte[] of length 16</returns>
        public static byte[] Hash(string stringToEncode)
        {
            if (string.IsNullOrWhiteSpace(stringToEncode))
            {
                throw new ArgumentNullException(nameof(stringToEncode));
            }

            return new MurmurHash3().ComputeHash(stringToEncode);
        }

        /// <summary>
        /// Generates the MurmurHash3 for the string 
        /// </summary>
        /// <param name="stringToEncode">String to encode</param>
        /// <returns>Hash for the string as a string of length 32</returns>
        public static string HashToString(string stringToEncode)
        {
            byte[] hashedArray = Hash(stringToEncode);

            // remove redundent hyphen to reduce memory cost
            return BitConverter.ToString(hashedArray).Replace("-", string.Empty);
        }

        /// <summary>
        /// Generates the Guid for the string.
        /// </summary>
        /// <param name="stringToEncode">String to encode</param>
        /// <returns>A Guid hashed from string.</returns>
        public static Guid HashToGuid(string stringToEncode)
        {
            return new Guid(HashToBytes(stringToEncode, 16));
        }

        /// <summary>
        /// Generates the murmur hash for the input string and maps resultant hash to
        /// maxValue buckets.
        /// MurmurHash ensures approximately random and even distribution of input string.
        /// The 128 bit murmurhash output is converted to 64 bits by XOR ing the excess byte i with
        /// the (i % 8) th byte of murmur output.
        /// This operation should maintain the randomness if we assume the MurmurHash output
        /// does not form any patterns for the input data. The Avalanche effect for MurmurHash
        /// (change in 1 bit of input string results in more than 50% of the hash output bits) should
        /// ensure that XOR ing the bytes also result in random output.
        /// </summary>
        /// <param name="stringToEncode">String to encode.</param>
        /// <param name="maxValue">Maximum integer value to bucket the murmur hash of string.</param>
        /// <returns>The integer value - (HashToBytes(stringToEncode, 8) % maxValue).</returns>
        public static ulong HashToUlong(string stringToEncode, ulong maxValue)
        {
            return BitConverter.ToUInt64(HashToBytes(stringToEncode, 8), 0) % maxValue;
        }

        /// <summary>
        /// Generates the murmur hash for the input string and maps resultant hash to
        /// number of bytes specified by byteLength.
        /// </summary>
        /// <param name="stringToEncode">String to encode.</param>
        /// <param name="byteLength">Number of bytes the murmur hash output should be mapped to.</param>
        /// <returns>Hash for the string as a byte[] of length byteLength.</returns>
        public static byte[] HashToBytes(string stringToEncode, int byteLength)
        {
            var hashedBytes = Hash(stringToEncode);
            var resultantBytes = new byte[byteLength];
            for (int i = 0; i < hashedBytes.Length; i++)
            {
                resultantBytes[i % byteLength] ^= hashedBytes[i];
            }

            return resultantBytes;
        }

        /// <summary>
        /// Final mix.
        /// </summary>
        /// <param name="k">The value.</param>
        /// <returns>The return value.</returns>
        private static ulong MixFinal(ulong k)
        {
            // avalanche bits
            k ^= k >> 33;
            k *= 0xff51afd7ed558ccdL;
            k ^= k >> 33;
            k *= 0xc4ceb9fe1a85ec53L;
            k ^= k >> 33;
            return k;
        }

        /// <summary>
        /// Mix first key.
        /// </summary>
        /// <param name="k1">The value.</param>
        /// <returns>The return value.</returns>
        private static ulong MixKey1(ulong k1)
        {
            k1 *= C1;
            k1 = RotateLeft(k1, 31);
            k1 *= C2;
            return k1;
        }

        /// <summary>
        /// Mix second key.
        /// </summary>
        /// <param name="k2">The value.</param>
        /// <returns>The return value.</returns>
        private static ulong MixKey2(ulong k2)
        {
            k2 *= C2;
            k2 = RotateLeft(k2, 33);
            k2 *= C1;
            return k2;
        }

        /// <summary>
        /// Rotate left.
        /// </summary>
        /// <param name="original">Original value.</param>
        /// <param name="bits">The bits.</param>
        /// <returns>The return value.</returns>
        private static ulong RotateLeft(ulong original, int bits)
        {
            return original << bits | original >> 64 - bits;
        }

        /// <summary>
        /// Get as a ulong.
        /// </summary>
        /// <param name="stringBytes">Value as bytes.</param>
        /// <param name="pos">The position.</param>
        /// <returns>The return value.</returns>
        private static ulong GetUInt64(byte[] stringBytes, int pos)
        {
            return BitConverter.ToUInt64(stringBytes, pos);
        }

        /// <summary>
        /// Compute the hash.
        /// </summary>
        /// <param name="stringToEncode">String to be encoded.</param>
        /// <returns>Hash value as a byte[]</returns>
        private byte[] ComputeHash(string stringToEncode)
        {
            // The cost of doing the below line is very low - in the order of less than a micro second for the cache key length we are talking about. 
            // Hence there isn't a point of optimizing the below line to something without encoding. 
            // Also, UTF8 produces one byte where possible instead of 2 bytes like ToCharArray() or Unicode do. 
            /* Future Optimizations if needed */

            /*  1. use "fixed (char* stringBytes = stringToEncode)" in ComputeHash function instead of calling Encoding.UTF8.GetBytes.
                2. Treat every char as ushort, so non-ascii char could be hashed correctly. Or treat as byte for better performance.
                3. Increase the char* pointer in every loop, no need to maintain the pos variable.
                4. Add [MethodImpl(MethodImplOptions.AggressiveInlining)] to private methods. */

            byte[] stringBytes = Encoding.UTF8.GetBytes(stringToEncode);

            h1 = Seed;

            int pos = 0;
            ulong length = (ulong)stringBytes.Length;
            ulong remaining = length;

            // read 128 bits, 16 bytes, 2 longs in each cycle
            while (remaining >= READSIZE)
            {
                ulong k1 = GetUInt64(stringBytes, pos);
                pos += 8;

                ulong k2 = GetUInt64(stringBytes, pos);
                pos += 8;

                remaining -= READSIZE;

                MixBody(k1, k2);
            }

            // if the input MOD 16 != 0
            if (remaining > 0)
            {
                ProcessBytesRemaining(stringBytes, remaining, pos, stringToEncode);
            }

            h1 ^= length;
            h2 ^= length;

            h1 += h2;
            h2 += h1;

            h1 = MixFinal(h1);
            h2 = MixFinal(h2);

            h1 += h2;
            h2 += h1;

            var hash = new byte[READSIZE];

            Array.Copy(BitConverter.GetBytes(h1), 0, hash, 0, 8);
            Array.Copy(BitConverter.GetBytes(h2), 0, hash, 8, 8);

            return hash;
        }

        /// <summary>
        /// Process the remaining bytes.
        /// </summary>
        /// <param name="bb">String to encode as bytes.</param>
        /// <param name="remaining">Remaining value.</param>
        /// <param name="pos">The position.</param>
        /// <param name="stringToEncode">String to encode.</param>
        private void ProcessBytesRemaining(byte[] bb, ulong remaining, int pos, string stringToEncode)
        {
            ulong k1 = 0;
            ulong k2 = 0;

            switch (remaining)
            {
                case 15:
                    k2 ^= (ulong)bb[pos + 14] << 48; // fall through
                    goto case 14;
                case 14:
                    k2 ^= (ulong)bb[pos + 13] << 40; // fall through
                    goto case 13;
                case 13:
                    k2 ^= (ulong)bb[pos + 12] << 32; // fall through
                    goto case 12;
                case 12:
                    k2 ^= (ulong)bb[pos + 11] << 24; // fall through
                    goto case 11;
                case 11:
                    k2 ^= (ulong)bb[pos + 10] << 16; // fall through
                    goto case 10;
                case 10:
                    k2 ^= (ulong)bb[pos + 9] << 8; // fall through
                    goto case 9;
                case 9:
                    k2 ^= bb[pos + 8]; // fall through
                    goto case 8;
                case 8:
                    k1 ^= GetUInt64(bb, pos);
                    break;
                case 7:
                    k1 ^= (ulong)bb[pos + 6] << 48; // fall through
                    goto case 6;
                case 6:
                    k1 ^= (ulong)bb[pos + 5] << 40; // fall through
                    goto case 5;
                case 5:
                    k1 ^= (ulong)bb[pos + 4] << 32; // fall through
                    goto case 4;
                case 4:
                    k1 ^= (ulong)bb[pos + 3] << 24; // fall through
                    goto case 3;
                case 3:
                    k1 ^= (ulong)bb[pos + 2] << 16; // fall through
                    goto case 2;
                case 2:
                    k1 ^= (ulong)bb[pos + 1] << 8; // fall through
                    goto case 1;
                case 1:
                    k1 ^= bb[pos]; // fall through
                    break;
                default:
                    throw new ApplicationException("Hash calculation failed for string " + stringToEncode);
            }

            // This is implementation of pseudocode from wikipedia. It does not need MixBody here. 
            h1 ^= MixKey1(k1);
            h2 ^= MixKey2(k2);
        }

        /// <summary>
        /// Mix the body.
        /// </summary>
        /// <param name="k1">First value.</param>
        /// <param name="k2">Second value.</param>
        private void MixBody(ulong k1, ulong k2)
        {
            h1 ^= MixKey1(k1);

            h1 = RotateLeft(h1, 27);
            h1 += h2;
            h1 = h1 * 5 + 0x52dce729;

            h2 ^= MixKey2(k2);

            h2 = RotateLeft(h2, 31);
            h2 += h1;
            h2 = h2 * 5 + 0x38495ab5;
        }
    }
}