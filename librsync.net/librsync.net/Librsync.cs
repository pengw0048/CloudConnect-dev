using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace librsync.net
{
    public static class Librsync
    {
        public static Stream ComputeSignature(Stream inputFile)
        {
            return ComputeSignature(
                inputFile,
                new SignatureJobSettings
                {
                    //MagicNumber = MagicNumber.Blake2Signature,
                    MagicNumber = MagicNumber.Md5MagicNumber,
                    //BlockLength = SignatureHelpers.DefaultBlockLength,
                    BlockLength = 64 * 1024,
                    //StrongSumLength = SignatureHelpers.DefaultStrongSumLength
                    StrongSumLength = 16
                });
        }
        
        public static Stream ComputeSignature(Stream inputFile, SignatureJobSettings settings)
        {
            return new SignatureStream(inputFile, settings);
        }

        public static Stream ComputeDelta(Stream signature, Stream newFile)
        {
            return new DeltaStream(signature, newFile);
        }

        public static Stream ApplyDelta(Stream originalFile, Stream delta)
        {
            return new PatchedStream(originalFile, delta);
        }
    }
}
