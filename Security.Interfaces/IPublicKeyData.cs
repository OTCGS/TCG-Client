using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Misc.Serialization;

namespace Security.Interfaces
{
    [XmlClass("RSAKeyValue")]
    public interface IPublicKeyData
    {
        //  <RSAKeyValue>
        //   <Modulus>…</Modulus>
        //   <Exponent>…</Exponent>
        //  </RSAKeyValue>
        [XmlElement("Modulus")]
        byte[] Modulus { get; }

        [XmlElement("Exponent")]
        byte[] Exponent { get; }

        void SetKey(byte[] modulus, byte[] exponent);



    }

    public class IPublicKeyComparer : IEqualityComparer<IPublicKeyData>
    {


        public bool Equals(IPublicKeyData x, IPublicKeyData y)
        {
            return EqualsStatic(x, y);
        }

        private static bool EqualsStatic(IPublicKeyData x, IPublicKeyData y)
        {
            if (x == null)
                return y == null;
            if (y == null)
                return false;

            return x.Exponent.SequenceEqual(y.Exponent) && x.Modulus.SequenceEqual(y.Modulus);
        }

        public int GetHashCode(IPublicKeyData k)
        {
            return GetHashCodeStatic(k);
        }

        private static int GetHashCodeStatic(IPublicKeyData k)
        {
            unchecked
            {
                var result = 0;
                foreach (byte b in k.Exponent.Concat(k.Modulus))
                    result = (result * 31) ^ b;
                return result;
            }
        }
    }

}
