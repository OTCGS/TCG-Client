using SQLite;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Client.Game.Data;

namespace Game.TransactionMap
{
    class PublicKey
    {

        private static readonly HashSet<PublicKey> cach = new HashSet<PublicKey>();
        private static readonly System.Threading.SemaphoreSlim semaphor = new System.Threading.SemaphoreSlim(1, 1);

        static PublicKey()
        {
            Graph.Commited += ClearCach;
            Graph.RoledBack += ClearCach;
        }

        private static void ClearCach()
        {
            cach.Clear();
        }

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
            try
            {
                await semaphor.WaitAsync();
                var cachedKey = cach.FirstOrDefault(x => x.Exponent.SequenceEqual(exponent) && x.Modulus.SequenceEqual(modulus));

                if (cachedKey != null)
                    return cachedKey;

                var asyncTableQuery = Graph.db.Table<PublicKey>();
                var where = asyncTableQuery.Where(x => x.Exponent == exponent && x.Modulus == modulus);
                var erg = await where.FirstOrDefaultAsync();

                if (erg == null)
                {
                    erg = new PublicKey(exponent, modulus);
                    await Graph.db.InsertAsync(erg);
                    cach.Add(erg);
                }
                return erg;

            }
            finally
            {
                semaphor.Release();
            }
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

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = obj as PublicKey;

            return other.PrimaryKey == PrimaryKey;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return PrimaryKey;
        }
    }
}
