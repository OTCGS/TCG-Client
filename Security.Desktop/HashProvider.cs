using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Security.Desktop
{
    class HashProvider
    {
        private static readonly SHA256 hashSha256;
        private static readonly SHA1 hashSha1;
        private static readonly MD5 hashMd5;

        static HashProvider()
        {
            hashSha256 = System.Security.Cryptography.SHA256.Create();
            hashMd5 = System.Security.Cryptography.MD5.Create();
            hashSha1 = System.Security.Cryptography.SHA1.Create();


        }
        public static byte[] HashSha256(byte[] data)
        {
            return hashSha256.ComputeHash(data);
        }

        public static byte[] HashMD5(byte[] data)
        {
            return hashMd5.ComputeHash(data);

        }

        public static byte[] HashSha1(byte[] data)
        {
            return hashSha1.ComputeHash(data);
        }


    }
}
