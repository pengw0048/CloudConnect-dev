using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace librsync.net
{
    public enum MagicNumber
    {
        /// <summary>
        /// Magic number for strong hashes computed with Blake2
        /// </summary>
        Blake2Signature = 0x72730137,

        /// <summary>
        /// DEPRECATED
        /// Magic number for strong hashes computed with MD4
        /// </summary>
        Md4MagicNumber = 0x72730136,

        /// <summary>
        /// Magic number for strong hashes computed with MD5
        /// </summary>
        Md5MagicNumber = 0x19890604,

        /// <summary>
        /// Magic number for strong hashes computed with MD5
        /// </summary>
        MurMur3 = 0x12345678,

        /// <summary>
        /// Magic number for deltas
        /// </summary>
        Delta = 0x72730236,
    }
    
    public static class SignatureHelpers
    {
        public const int DefaultBlockLength = 2 * 1024;
        public const int DefaultStrongSumLength = 32;
        private static System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();

        public static void WriteHeader(BinaryWriter s, SignatureJobSettings settings)
        {
            StreamHelpers.WriteBigEndian(s, (uint)settings.MagicNumber);
            StreamHelpers.WriteBigEndian(s, (uint)settings.BlockLength);
            StreamHelpers.WriteBigEndian(s, (uint)settings.StrongSumLength);
        }

        public static void WriteBlock(BinaryWriter s, byte[] block, SignatureJobSettings settings)
        {
            int weakSum = CalculateWeakSum(block);
            byte[] strongSum;

            if (settings.MagicNumber == MagicNumber.Blake2Signature)
            {
                strongSum = CalculateBlake2StrongSum(block);
            }
            else if (settings.MagicNumber == MagicNumber.Md5MagicNumber)
            {
                strongSum = CalculateMD5StrongSum(block);
            }
            else if (settings.MagicNumber == MagicNumber.MurMur3)
            {
                strongSum = CalculateMurmur3(block);
            }
            else
            {
                throw new NotImplementedException("Non-blake2 hashes aren't supported");
            }

            StreamHelpers.WriteBigEndian(s, (ulong)weakSum, bytes: 4);
            s.Write(strongSum, 0, settings.StrongSumLength);
        }

        private static int CalculateWeakSum(byte[] buf)
        {
            Rollsum sum = new Rollsum();
            sum.Update(buf);
            return sum.Digest;
        }

        private static byte[] CalculateMD5StrongSum(byte[] block)
        {
            byte[] t= md5.ComputeHash(block);
            return t;
        }

        private static byte[] CalculateMurmur3(byte[] block)
        {
            return Murmur3.ComputeHash(block);
        }

        private static byte[] CalculateBlake2StrongSum(byte[] block)
        {
            return Blake2Sharp.Blake2B.ComputeHash(block, new Blake2Sharp.Blake2BConfig { OutputSizeInBytes = DefaultStrongSumLength });
        }

        public static SignatureFile ParseSignatureFile(Stream s)
        {
            var result = new SignatureFile();
            var r = new BinaryReader(s);
            uint magicNumber = StreamHelpers.ReadBigEndianUint32(r);
            if (magicNumber == (uint)MagicNumber.Blake2Signature)
            {
                result.StrongSumMethod = CalculateBlake2StrongSum;
            }
            else if (magicNumber == (uint)MagicNumber.Md5MagicNumber)
            {
                result.StrongSumMethod = CalculateMD5StrongSum;
            }
            else if (magicNumber == (uint)MagicNumber.MurMur3)
            {
                result.StrongSumMethod = CalculateMurmur3;
            }
            else
            {
                throw new InvalidDataException(string.Format("Unknown magic number {0}", magicNumber));
            }

            result.BlockLength = (int)StreamHelpers.ReadBigEndianUint32(r);
            result.StrongSumLength = (int)StreamHelpers.ReadBigEndianUint32(r);

            var signatures = new List<BlockSignature>();
            ulong i = 0;
            while (true)
            {
                byte[] weakSumBytes = r.ReadBytes(4);
                if (weakSumBytes.Length == 0)
                {
                    // we're at the end of the file
                    break;
                }
                
                int weakSum = (int)StreamHelpers.ConvertFromBigEndian(weakSumBytes);
                byte[] strongSum = r.ReadBytes(result.StrongSumLength);
                signatures.Add(new BlockSignature
                {
                    StartPos = (ulong)result.BlockLength * i,
                    WeakSum = weakSum,
                    StrongSum = strongSum
                });

                i++;
            }

            result.BlockLookup = signatures.ToLookup(sig => sig.WeakSum);
            return result;
        }
    }

    public struct SignatureJobSettings
    {
        public MagicNumber MagicNumber;
        public int BlockLength;
        public int StrongSumLength;
    }

    public struct SignatureFile
    {
        public int BlockLength;
        public int StrongSumLength;
        public Func<byte[], byte[]> StrongSumMethod;
        public ILookup<int, BlockSignature> BlockLookup;
    }
}
