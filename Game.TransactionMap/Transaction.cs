using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Misc;

namespace Game.TransactionMap
{
    class Transaction
    {
        [PrimaryKey, AutoIncrement]
        public int PrimaryKey { get; set; }

        [Ignore]
        public Task<PublicKey> A
        {
            get
            {
                return Graph.db.FindAsync<PublicKey>(AFk);
            }
        }

        [Ignore]
        public Task<PublicKey> B
        {
            get
            {
                return Graph.db.FindAsync<PublicKey>(BFk);
            }
        }

        [Ignore]
        public Task<List<Transfare>> Transfares
        {
            get
            {
                return Graph.db.Table<Transfare>().Where(x => x.ParentFK == PrimaryKey).ToListAsync();

            }
        }


        public int AFk { get; set; }
        public int BFk { get; set; }

        public byte[] Hash { get; set; }
        public byte[] SigA { get; set; }
        public byte[] SigB { get; set; }

        internal async Task<bool> CheckSig()
        {
            var pkA = await A;
            var pkB = await B;

            var tosign = await GenereateBytesToSign();
            if (!pkA.ToIPublicKey().Veryfiy(tosign, SigA))
                return false;
            if (!pkB.ToIPublicKey().Veryfiy(tosign, SigB))
                return false;

            return true;
        }

        internal async Task<bool> CheckHash()
        {
            var buffer = Security.SecurityFactory.HashSha256(await GenereateBytesToSign());

            return buffer.SequenceEqual(Hash);

        }

        internal async Task<byte[]> GenereateBytesToSign()
        {
            var pka = await A;
            var pkb = await B;

            var pkLookup = new Dictionary<int, PublicKey>();
            pkLookup.Add(pka.PrimaryKey, pka);
            pkLookup.Add(pkb.PrimaryKey, pkb);


            var buffer = new List<byte>();

            buffer.AddRange(pka.Modulus);
            buffer.AddRange(pka.Exponent);

            buffer.AddRange(pkb.Modulus);
            buffer.AddRange(pkb.Exponent);

            var bytearraycomparer = new Misc.Portable.ByteArrayComparer();


            var data = await Task.WhenAll((await Transfares).Select(async x => new { Creator = await x.CardCreator, Transfare = x }));

            foreach (var transfare in data.OrderBy(x => x.Transfare.CardID.ToBigEndianBytes(), bytearraycomparer).ThenBy(x => x.Creator.Modulus, bytearraycomparer).ThenBy(x => x.Creator.Exponent, bytearraycomparer).Select(x => x.Transfare))
            {
                // Generate Hash
                buffer.AddRange(transfare.CardID.ToBigEndianBytes());

                var cardCreator = (await transfare.CardCreator);
                buffer.AddRange(cardCreator.Modulus);
                buffer.AddRange(cardCreator.Exponent);


                buffer.AddRange(Misc.BitConverter.GetBytes(transfare.CardTransferIndex));

                buffer.AddRange(pkLookup[transfare.SenderFK].Modulus);
                buffer.AddRange(pkLookup[transfare.SenderFK].Exponent);

                buffer.AddRange(pkLookup[transfare.ReciverFk].Modulus);
                buffer.AddRange(pkLookup[transfare.ReciverFk].Exponent);

                buffer.AddRange((await transfare.PreviousTransactionHash) ?? new byte[0]);

            }

            var erg = buffer.ToArray();

            return erg;
        }

        internal async Task<bool> CheckPreTransactions()
        {
            // Eine transaktion ist gültig, wenn alle Transfares eine Gültile transaction haben

            foreach (var t in await Transfares)
            {
                if (t.CardTransferIndex == 0)
                { //Sonderfall karte wird erstellt. 

                }
                else
                {

                    // Überprüfe eigentümer der vorherigen karte
                    if (t.SenderFK != (await t.PreviousTransfare).ReciverFk)
                        return false;

                    var cIndex = t.CardTransferIndex - 1;
                    var possybleTransfares = await Graph.db.Table<Transfare>().Where(x => x.CardID == t.CardID && x.CardCreatorFK == t.CardCreatorFK && x.CardTransferIndex == cIndex).ToListAsync();

                    if (!possybleTransfares.Any(x => x.PrimaryKey == t.PreviousTransfareFK || x.PreviousTransfareFK == t.PreviousTransfareFK))
                        return false;
                }



            }

            return true;
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
            var other = obj as Transaction;
            return other.PrimaryKey == this.PrimaryKey;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return PrimaryKey;
        }
    }
}
