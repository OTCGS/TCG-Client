using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.TransactionMap.ServiceMerger
{
    internal class TransferImplimentation : ITransfer
    {
        public Guid CardId
        {
            get;
            set;
        }

        public int CardTransferIndex
        {
            get; set;
        }

        public Client.Game.Data.PublicKey Creator
        {
            get; set;
        }

        public Client.Game.Data.PublicKey Giver
        {
            get; set;
        }

        public byte[] PreviousTransactionHash
        {
            get; set;

        }

        public Client.Game.Data.PublicKey Recipient
        {
            get; set;
        }

        
    }
    internal class TransactionImplimentation : ITransaction
    {
        public Client.Game.Data.PublicKey A
        {
            get;
            set;
        }

        public Client.Game.Data.PublicKey B
        {
            get;
            set;
        }

        public byte[] Hash
        {
            get;
            set;
        }

        public byte[] SignatureA
        {
            get;
            set;
        }

        public byte[] SignatureB
        {
            get;
            set;
        }

        public IEnumerable<ITransfer> Transfers
        {
            get;
            set;
        }

     
    }
}
