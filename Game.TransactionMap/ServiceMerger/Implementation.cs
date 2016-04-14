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

        public static async Task<TransferImplimentation> GetImplementation(Transfare v)
        {
            return new TransferImplimentation()
            {
                CardId = v.CardID,
                CardTransferIndex = v.CardTransferIndex,
                Creator = (await v.CardCreator).ToGameData(),
                Giver = (await v.Sender).ToGameData(),
                Recipient = (await v.Reciver).ToGameData(),
                PreviousTransactionHash = await v.PreviousTransactionHash
            };
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

        public static async Task<TransactionImplimentation> GetImplementation(Transaction v)
        {
            return new TransactionImplimentation()
            {
                A = (await v.A).ToGameData(),
                B = (await v.B).ToGameData(),
                Hash = v.Hash,
                SignatureA = v.SigA,
                SignatureB = v.SigB,
                Transfers = await Task.WhenAll((await v.Transfares).Select(x => TransferImplimentation.GetImplementation(x)))
            };
        }
    }
}
