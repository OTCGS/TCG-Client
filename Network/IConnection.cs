using System;
using System.Threading.Tasks;

namespace Network
{
    public interface IConnection : IDisposable
    {
        event Action<byte[]> Recived;

        Task Send(byte[] data);
    }

    public interface IUserConnection : IConnection
    {
        User User { get; }

        ConnectionReason ConnectionReason { get; }

        Guid DataId { get; }
        Security.IPublicKey DataKey { get; }
    }

    public enum ConnectionReason : byte
    {
        Unkonwon = 0,
        Play = 1,
        Trade = 2
    }
}