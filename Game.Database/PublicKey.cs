using SQLite;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Client.Game.Data;

namespace Game.Database
{
    class PublicKey
    {

        [Obsolete("Die Statische Methode Nutzen", true)]
        public PublicKey()
        {

        }

        private PublicKey(byte[] exponent, byte[] modulus)
        {
            Exponent = exponent;
            Modulus = modulus;
        }

        [PrimaryKey, AutoIncrement]
        public int PrimaryKey { get; set; }

        [Indexed]
        public byte[] Modulus { get; set; }
        [Indexed]
        public byte[] Exponent { get; set; }

        public static Task<PublicKey> GetKey(Security.IPublicKey p)
        {
            return GetKey(p.Modulus, p.Exponent);
        }

        public async static Task<PublicKey> GetKey(byte[] modulus, byte[] exponent)
        {
            var asyncTableQuery = Database.db.Table<PublicKey>();
            var where = asyncTableQuery.Where(x => x.Exponent == exponent && x.Modulus == modulus);
            var erg = await  where.FirstOrDefaultAsync();

            if (erg == null)
            {
                erg = new PublicKey(exponent, modulus);
                await Database.db.InsertAsync(erg);
            }
            return erg;

        }


        internal static Task<PublicKey> GetKey(Client.Game.Data.PublicKey p)
        {
            return GetKey(p.Modulus, p.Exponent);
        }

        public Client.Game.Data.PublicKey ToGameData()
        {
            return new Client.Game.Data.PublicKey() { Exponent = this.Exponent, Modulus = this.Modulus };
        }

        public Security.IPublicKey ToIPublicKey()
        {
            var k = Security.SecurityFactory.CreatePublicKey();

            k.SetKey(Modulus, Exponent);
            return k;
        }

    }
}
