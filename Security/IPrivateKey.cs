using Misc.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Security
{
    [XmlClass("RSAKeyValue")]
    public interface IPrivateKey : IPublicKey
    {
        //<RSAKeyValue>
        //   <Modulus>…</Modulus>
        //   <Exponent>…</Exponent>
        //   <P>…</P>
        //   <Q>…</Q>
        //   <DP>…</DP>
        //   <DQ>…</DQ>
        //   <InverseQ>…</InverseQ>
        //   <D>…</D>
        //</RSAKeyValue>

        [XmlElement("P")]
        byte[] P { get; }

        [XmlElement("Q")]
        byte[] Q { get; }

        [XmlElement("DP")]
        byte[] DP { get; }

        [XmlElement("DQ")]
        byte[] DQ { get; }

        [XmlElement("InverseQ")]
        byte[] InverseQ { get; }

        [XmlElement("D")]
        byte[] D { get; }

        void SetKey(byte[] modulus, byte[] exponent, byte[] p, byte[] q, byte[] dp, byte[] dq, byte[] inverseQ, byte[] d);


#if DEBUG
        Task<byte[]> Sign(byte[] toSign, [CallerMemberName] string callerName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLine = -1);
#else
        Task<byte[]> Sign(byte[] toSign);
#endif


    }
}