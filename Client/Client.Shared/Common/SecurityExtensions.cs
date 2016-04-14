using Security.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Security
{
    public static class KeyExporter2
    {
        public static Client.Game.Data.PublicKey ToGameData(this IPublicKeyData key)
        {
            return new Client.Game.Data.PublicKey() { Exponent = key.Exponent, Modulus = key.Modulus };
        }
        public static Security.IPublicKey ToSecurety(this IPublicKeyData key)
        {
            var k = Security.SecurityFactory.CreatePublicKey();
            k.SetKey(key.Modulus, key.Exponent);
            return k;

        }

    }
}
