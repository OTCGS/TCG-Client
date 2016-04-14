using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if !NETFX_CORE
using ns = Security.Desktop;
#else
using ns = Security.Store;
#endif
namespace Security
{
    public static class SecurityFactory
    {

        public static IPrivateKey CreatePrivateKey()
        {
            return new ns.PrivateKey();
        }

        public static IPublicKey CreatePublicKey()
        {
            return new ns.PublicKey();
        }

        public static byte[] HashSha256(byte[] toHash)
        {
            return ns.HashProvider.HashSha256(toHash);

        }
        public static byte[] HashMd5(byte[] toHash)
        {
            return ns.HashProvider.HashMD5(toHash);

        }
        public static byte[] HashSha1(byte[] toHash)
        {
            return ns.HashProvider.HashSha1(toHash);

        }

    }
}