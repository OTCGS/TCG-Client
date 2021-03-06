﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;

namespace Security.Store
{
    internal partial class PrivateKey : IPrivateKey
    {
        private bool validParameter = false;

        private byte[] modulusCach;
        private byte[] exponentCach;
        private byte[] pCach;
        private byte[] qCach;
        private byte[] dpCach;
        private byte[] dqCach;
        private byte[] inverseQCach;
        private byte[] dCach;

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

        public Windows.Security.Cryptography.Core.CryptographicKey KeyPair { get; set; }

        public Windows.Security.Cryptography.Core.AsymmetricKeyAlgorithmProvider provider;

        public PrivateKey()
        {
            provider = Windows.Security.Cryptography.Core.AsymmetricKeyAlgorithmProvider.OpenAlgorithm(Windows.Security.Cryptography.Core.AsymmetricAlgorithmNames.RsaSignPkcs1Sha256);
            KeyPair = provider.CreateKeyPair(1024);
            validParameter = true;
        }


#if DEBUG
        public async Task<byte[]> Sign(byte[] toSign, [CallerMemberName] string callerName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLine = -1)
#else
        public async Task<byte[]> Sign(byte[] toSign)
#endif
        {
            if (!ValidParameter)
                throw new InvalidOperationException("Private Key muss aus Validen Daten erstellt sein.");
            var sig = (await CryptographicEngine.SignAsync(KeyPair, toSign.AsBuffer())).ToArray();
#if DEBUG
            Log(callerName, callerFilePath, callerLine, this.modulusCach, this.exponentCach, toSign, sig);
#endif
            return sig;
        }

#if DEBUG
        public bool Veryfiy(byte[] toVeryfy, byte[] signiture, [CallerMemberName] string callerName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLine = -1)
#else
        public bool Veryfiy(byte[] toVeryfy, byte[] signiture)
#endif
        {
            if (!ValidParameter)
                throw new InvalidOperationException("Private Key muss aus Validen Daten erstellt sein.");
            var erg = CryptographicEngine.VerifySignature(KeyPair, toVeryfy.AsBuffer(), signiture.AsBuffer());
#if DEBUG
            Log(callerName, callerFilePath, callerLine, this.modulusCach, this.exponentCach, toVeryfy, signiture, erg);
#endif

            return erg;
        }

        private void SetCach()
        {
            var byteblob = KeyPair.Export(CryptographicPrivateKeyBlobType.Capi1PrivateKey).ToArray();
            var blobstruct = BlobConverter.ToPrivateKeyBlobData(byteblob);
            BlobConverter.GetParameters(blobstruct, out modulusCach, out exponentCach, out pCach, out qCach, out dpCach, out dqCach, out inverseQCach, out dCach);
        }




        public void SetKey(byte[] modulus, byte[] exponent)
        {
            throw new NotSupportedException();
        }

        public void SetKey(byte[] modulus, byte[] exponent, byte[] p, byte[] q, byte[] dp, byte[] dq, byte[] inverseQ, byte[] d)
        {
            var binary = BlobConverter.ToPrivateKeyBlobByte(BlobConverter.ToPrivateKeyBlobData(modulus, exponent, p, q, dp, dq, inverseQ, d));

            try
            {
                KeyPair = provider.ImportKeyPair(binary.AsBuffer(), CryptographicPrivateKeyBlobType.Capi1PrivateKey);
                validParameter = true;
            }
            catch (Exception)
            {
                validParameter = false;
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