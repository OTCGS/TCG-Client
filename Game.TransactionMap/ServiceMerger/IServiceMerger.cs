using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Client.Game.Data;
using System.Linq;

namespace Game.TransactionMap.ServiceMerger
{
    public interface IServiceMerger
    {
        Task<IEnumerable<ITransaction>> GetHeads();
        Task<ITransaction> GetTransaction(byte[] hash);
        Task SubmitTransactions(IEnumerable<ITransaction> transactions);
    }

    public interface ITransaction
    {
        Client.Game.Data.PublicKey A { get; }

        Client.Game.Data.PublicKey B { get; }

        byte[] SignatureA { get; }

        byte[] SignatureB { get; }

        IEnumerable<ITransfer> Transfers { get; }

        byte[] Hash { get; }

    }
    public interface ITransfer
    {
        Client.Game.Data.PublicKey Giver { get; }

        Client.Game.Data.PublicKey Recipient { get; }

        Client.Game.Data.PublicKey Creator { get; }

        Guid CardId { get; }

        int CardTransferIndex { get; }

        byte[] PreviousTransactionHash { get; }
    }

    
}
