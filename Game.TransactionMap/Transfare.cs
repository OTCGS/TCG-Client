using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Game.TransactionMap
{
    class Transfare
    {
        [PrimaryKey, AutoIncrement]
        public int PrimaryKey { get; set; }

        public int CardTransferIndex { get; set; }
        public Guid CardID { get; set; }

        [Ignore]
        public Task<PublicKey> Sender
        {
            get
            {
                return Graph.db.FindAsync<PublicKey>(SenderFK);
            }
        }

        [Ignore]
        public Task<PublicKey> Reciver
        {
            get
            {
                return Graph.db.FindAsync<PublicKey>(ReciverFk);
            }
        }

        [Ignore]
        public Task<Transaction> Parent
        {
            get
            {
                return Graph.db.FindAsync<Transaction>(ParentFK);
            }
        }

        [Ignore]
        public Task<Transfare> PreviousTransfare
        {
            get
            {
                return Graph.db.FindAsync<Transfare>(PreviousTransfareFK);
            }
        }


        [Ignore]
        public Task<PublicKey> CardCreator
        {
            get
            {
                return Graph.db.FindAsync<PublicKey>(CardCreatorFK);
            }
        }

        [Ignore]
        public Task<byte[]> PreviousTransactionHash
        {
            get
            {
                return Task.Run<byte[]>(async () =>
                {
                    var p = await Graph.db.FindAsync<Transfare>(PreviousTransfareFK);
                    if (p == null)
                        return null;
                    return (await p.Parent).Hash;

                });
            }
        }

        public int SenderFK { get; set; }
        public int ReciverFk { get; set; }
        public int PreviousTransfareFK { get; set; }
        public int ParentFK { get; set; }
        public int CardCreatorFK { get; set; }
        public bool Valid { get; internal set; }

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

            var other = obj as Transfare;
            return other.PrimaryKey == this.PrimaryKey;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return PrimaryKey;
        }
    }
}
