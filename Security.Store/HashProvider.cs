using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;

namespace Security.Store
{
    class HashProvider
    {
        private static readonly HashAlgorithmProvider MD5;
        private static readonly HashAlgorithmProvider Sha1;
        private static readonly HashAlgorithmProvider Sha256;

        static HashProvider()
        {
            Sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            MD5 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            Sha1 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);

        }
        public static byte[] HashSha256(byte[] data)
        {
            var cryptographicHash = Sha256.CreateHash();
            cryptographicHash.Append(data.AsBuffer());
            return cryptographicHash.GetValueAndReset().ToArray();
        }

        public static byte[] HashMD5(byte[] data)
        {
            var cryptographicHash = MD5.CreateHash();
            cryptographicHash.Append(data.AsBuffer());
            return cryptographicHash.GetValueAndReset().ToArray();
        }

        public static byte[] HashSha1(byte[] data)
        {
            var cryptographicHash = Sha1.CreateHash();
            cryptographicHash.Append(data.AsBuffer());
            return cryptographicHash.GetValueAndReset().ToArray();
        }
    }
}
