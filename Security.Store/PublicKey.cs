using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;

namespace Security.Store
{
    internal partial class PublicKey : IPublicKey
    {
        protected Windows.Security.Cryptography.Core.CryptographicKey KeyPair
        {
            get;
            set;
        }

        private bool validParameter = false;
        private Windows.Security.Cryptography.Core.AsymmetricKeyAlgorithmProvider provider;
        private byte[] moduloCach;
        private byte[] exponentCach;
        public PublicKey()
        {
            provider = Windows.Security.Cryptography.Core.AsymmetricKeyAlgorithmProvider.OpenAlgorithm(Windows.Security.Cryptography.Core.AsymmetricAlgorithmNames.RsaSignPkcs1Sha256);
            KeyPair = provider.CreateKeyPair(512);
        }

        public byte[] Modulus
        {
            get
            {
                if (moduloCach == null)
                    SetCach();
                return moduloCach;
            }

            set
            {
                if (moduloCach == null)
                    SetCach();
                if (value.SequenceEqual(moduloCach))
                    return;
                SetKey(value, Exponent);
            }
        }

        public void SetKey(byte[] modulo, byte[] exponent)
        {
            var binary = BlobConverter.ToPublicKeyBlobByte(BlobConverter.ToPublicKeyBlobData(modulo, exponent));
            try
            {
                KeyPair = provider.ImportPublicKey(binary.AsBuffer(), CryptographicPublicKeyBlobType.Capi1PublicKey);
                validParameter = true;
            }
            catch (Exception)
            {
                validParameter = false;
            }

            moduloCach = modulo;
            exponentCach = exponent;
        }

        private void SetCach()
        {
            var byteblob = KeyPair.ExportPublicKey().ToArray();
            var blobstruct = BlobConverter.ToPublicKeyBlobData(byteblob);
            BlobConverter.GetParameters(blobstruct, out moduloCach, out exponentCach);
        }

        public byte[] Exponent
        {
            get
            {
                if (exponentCach == null)
                    SetCach();
                return exponentCach;
            }

            set
            {
                if (exponentCach == null)
                    SetCach();
                if (value.SequenceEqual(exponentCach))
                    return;
                SetKey(Modulus, value);
            }
        }


#if DEBUG
        public bool Veryfiy(byte[] toVeryfy, byte[] signiture, [CallerMemberName] string callerName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLine = -1)
#else
        public bool Veryfiy(byte[] toVeryfy, byte[] signiture)
#endif
        {
            if (!ValidParameter)
                throw new InvalidOperationException("Daten des PublicKeys müssen valide sein. (Veryfiy)");


            var erg = CryptographicEngine.VerifySignature(KeyPair, toVeryfy.AsBuffer(), signiture.AsBuffer());
#if DEBUG
            Log(callerName, callerFilePath, callerLine,this.moduloCach, this.exponentCach, toVeryfy, signiture, erg);
#endif
            return erg;
        }

        public bool ValidParameter
        {
            get
            {
                return validParameter;
            }
        }

    }
}