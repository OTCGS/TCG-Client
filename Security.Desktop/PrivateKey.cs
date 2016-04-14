using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Security.Desktop
{
    internal class PrivateKey : IPrivateKey
    {
        private byte[] modulusCach;
        private byte[] exponentCach;
        private byte[] pCach;
        private byte[] qCach;
        private byte[] dpCach;
        private byte[] dqCach;
        private byte[] inverseQCach;
        private byte[] dCach;

        protected readonly System.Security.Cryptography.SHA256 hash = System.Security.Cryptography.SHA256.Create();

        private bool validParameter = true;

        public byte[] Modulus
        {
            get
            {
                if (modulusCach == null)
                    SetCach();
                return modulusCach;
            }
            set
            {
                if (modulusCach == null)
                    SetCach();
                if (value.SequenceEqual(modulusCach))
                    return;
                SetKey(value, Exponent, P, Q, DP, DQ, InverseQ, D);
            }
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
                SetKey(Modulus, value, P, Q, DP, DQ, InverseQ, D);
            }
        }

        public byte[] P
        {
            get
            {
                if (pCach == null)
                    SetCach();
                return pCach;
            }
            set
            {
                if (pCach == null)
                    SetCach();
                if (value.SequenceEqual(pCach))
                    return;
                SetKey(Modulus, Exponent, value, Q, DP, DQ, InverseQ, D);
            }
        }

        public byte[] Q
        {
            get
            {
                if (qCach == null)
                    SetCach();
                return qCach;
            }
            set
            {
                if (qCach == null)
                    SetCach();
                if (value.SequenceEqual(qCach))
                    return;
                SetKey(Modulus, Exponent, P, value, DP, DQ, InverseQ, D);
            }
        }

        public byte[] DP
        {
            get
            {
                if (dpCach == null)
                    SetCach();
                return dpCach;
            }
            set
            {
                if (dpCach == null)
                    SetCach();
                if (value.SequenceEqual(dpCach))
                    return;
                SetKey(Modulus, Exponent, P, Q, value, DQ, InverseQ, D);
            }
        }

        public byte[] DQ
        {
            get
            {
                if (dqCach == null)
                    SetCach();
                return dqCach;
            }
            set
            {
                if (dqCach == null)
                    SetCach();
                if (value.SequenceEqual(dqCach))
                    return;
                SetKey(Modulus, Exponent, P, Q, DP, value, InverseQ, D);
            }
        }

        public byte[] InverseQ
        {
            get
            {
                if (inverseQCach == null)
                    SetCach();
                return inverseQCach;
            }
            set
            {
                if (inverseQCach == null)
                    SetCach();
                if (value.SequenceEqual(inverseQCach))
                    return;
                SetKey(Modulus, Exponent, P, Q, DP, DQ, value, D);
            }
        }

        public byte[] D
        {
            get
            {
                if (dCach == null)
                    SetCach();
                return dCach;
            }
            set
            {
                if (dCach == null)
                    SetCach();
                if (value.SequenceEqual(dCach))
                    return;
                SetKey(Modulus, Exponent, P, Q, DP, DQ, InverseQ, value);
            }
        }

        protected System.Security.Cryptography.RSACryptoServiceProvider KeyPair { get; set; }

        public PrivateKey()
        {
            this.KeyPair = new System.Security.Cryptography.RSACryptoServiceProvider(1024);
            validParameter = true;
        }

#if DEBUG
        public Task<byte[]> Sign(byte[] toSign, [CallerMemberName] string callerName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLine = -1)
#else
        public  Task<byte[]> Sign(byte[] toSign)
#endif
        {
            if (!ValidParameter)
                throw new InvalidOperationException("Daten des Private Keys müssen valide sein. (Sign)");
            return Task.FromResult(KeyPair.SignData(toSign, hash));
        }

#if DEBUG
        public bool Veryfiy(byte[] toVeryfy, byte[] signiture, [CallerMemberName] string callerName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLine = -1)
#else
        public bool Veryfiy(byte[] toVeryfy, byte[] signiture)
#endif
        {
            if (!ValidParameter)
                throw new InvalidOperationException("Daten des Privatr Keys müssen valide sein. (Veryfi)");
            return KeyPair.VerifyData(toVeryfy, hash, signiture);
        }

        private void SetCach()
        {
            var byteblob = KeyPair.ExportCspBlob(true);
            var blobstruct = BlobConverter.ToPrivateKeyBlobData(byteblob);
            BlobConverter.GetParameters(blobstruct, out modulusCach, out exponentCach, out pCach, out qCach, out dpCach, out dqCach, out inverseQCach, out dCach);
        }

        public  void SetKey(byte[] modulus, byte[] exponent)
        {
            throw new NotSupportedException();
        }
        public void SetKey(byte[] modulus, byte[] exponent, byte[] p, byte[] q, byte[] dp, byte[] dq, byte[] inverseQ, byte[] d)
        {
            var binary = BlobConverter.ToPrivateKeyBlobByte(BlobConverter.ToPrivateKeyBlobData(modulus, exponent, p, q, dp, dq, inverseQ, d));
            try
            {
                this.KeyPair.ImportCspBlob(binary);
                this.validParameter = true;
            }
            catch (Exception)
            {
                this.validParameter = false;
            }
            modulusCach = modulus;
            exponentCach = exponent;
            pCach = p;
            qCach = q;
            dpCach = dp;
            dqCach = dq;
            inverseQCach = inverseQ;
            dCach = d;
        }

        public bool ValidParameter
        {
            get { return validParameter; }
        }
    }
}