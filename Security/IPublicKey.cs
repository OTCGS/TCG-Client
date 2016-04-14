using Misc.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Security.Interfaces;
using System.Runtime.CompilerServices;

namespace Security
{
    /// <summary>
    ///
    /// </summary>



    [XmlClass("RSAKeyValue")]
    public interface IPublicKey : IPublicKeyData
    {

        [XmlIgnore]
        bool ValidParameter { get; }

        //bool Veryfiy(byte[] toVeryfy, byte[] signiture);

#if DEBUG
        bool Veryfiy(byte[] toVeryfy, byte[] signiture, [CallerMemberName] string callerName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLine = -1);
#else
        bool Veryfiy(byte[] toVeryfy, byte[] signiture);
#endif

    }

    public static class KeyExporter
    {
        public static string ToXml(this IPublicKey key)
        {
            Misc.Serialization.XmlSerilizer<IPublicKey> o = new XmlSerilizer<IPublicKey>();
            return o.Serialize(key);
        }

        public static string ToPrivateXml(this IPrivateKey key)
        {
            Misc.Serialization.XmlSerilizer<IPrivateKey> o = new XmlSerilizer<IPrivateKey>();
            return o.Serialize(key);
        }

        public static IPublicKey LoadXml(this IPublicKey key, string xml)
        {
            Misc.Serialization.XmlSerilizer<IPublicKey> o = new XmlSerilizer<IPublicKey>();
            o.AddFactoryMethod(() => key);
            return o.Deserilize(xml);
        }

        public static IPrivateKey LoadPrivateXml(this IPrivateKey key, string xml)
        {
            Misc.Serialization.XmlSerilizer<IPrivateKey> o = new XmlSerilizer<IPrivateKey>();
            o.AddFactoryMethod(() => key);
            return o.Deserilize(xml);
        }


        public static string FingerPrint(this IPublicKeyData k)
        {
            return Convert.ToBase64String(Security.SecurityFactory.HashMd5(k.Modulus.Concat(k.Exponent).ToArray()));
        }

        public static bool EqualsIPublicKeyData(this IPublicKeyData a,IPublicKeyData b)
        {
          return   a.Exponent.SequenceEqual(b.Exponent) && a.Modulus.SequenceEqual(b.Modulus);
        }



    }
}