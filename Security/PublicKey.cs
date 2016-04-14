using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

#if NETFX_CORE
namespace Security.Store
#else
namespace Security.Desktop
#endif
{
    internal partial class PublicKey
    {



        [System.Diagnostics.Conditional("DEBUG")]
        private void Log(string callerName, string callerFilePath, int callerLine, byte[] modulus, byte[] exponent, byte[] data, byte[] signature, bool? boolreturn = null, [CallerMemberName]string callingMethod = "")
        {
            Logger.TransactionInfo($@"{callingMethod}:
    Public Key Modulus:
{ BlockFormat(modulus, 2)}
    Public Key Exponent:
{ BlockFormat(exponent, 2)}
    Data:
{ BlockFormat(data, 2)}
    Signatur:
{ BlockFormat(signature, 2)}
    IsValid={boolreturn}", callerName, callerFilePath, callerLine);
        }



        private string BlockFormat(byte[] data, int indention = 0)
        {
            var str = BitConverter.ToString(data).Replace("-", "");
            StringBuilder b = new StringBuilder();
            for (int j = 0; j < indention; j++)
                b.Append('\t');
            for (int i = 0; i < str.Length; i += 4)
            {
                if ((i / 4) % 6 == 0 && i > 0)
                {
                    b.AppendLine();
                    for (int j = 0; j < indention; j++)
                        b.Append('\t');
                }
                b.Append(str.Substring(i, Math.Min(4, str.Length - i)));
                b.Append(' ');
            }
            return b.ToString();
        }


        public override int GetHashCode()
        {
            unchecked
            {
                const int prime = 31;
                int result = 1;
                foreach (var item in Modulus.Concat(Exponent))
                    result = result * prime + item;
                return result;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is PublicKey))
                return false;
            var other = (PublicKey)obj;
            if (!Modulus.SequenceEqual(other.Modulus))
                return false;
            if (!Exponent.SequenceEqual(other.Exponent))
                return false;
            return true;
        }
    }
}