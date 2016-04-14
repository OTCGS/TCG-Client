using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Security.Desktop
{
    internal partial class PublicKey : IPublicKey
    {
        protected System.Security.Cryptography.RSACryptoServiceProvider KeyPair { get; set; }

        protected readonly System.Security.Cryptography.SHA256 hash = System.Security.Cryptography.SHA256.Create();
        private byte[] moduloCach;
        private byte[] exponentCach;

        private bool validParameter = true;

        public PublicKey()
        {
            KeyPair = new System.Security.Cryptography.RSACryptoServiceProvider();
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
                this.KeyPair.ImportCspBlob(binary);
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
            if (!validParameter)
                return;
            var byteblob = KeyPair.ExportCspBlob(false);
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
                throw new InvalidOperationException("Daten des Keys müssen valide sein.");
            return KeyPair.VerifyData(toVeryfy, hash, signiture);
        }

        public bool ValidParameter
        {
            get { return validParameter; }
        }
    }
}