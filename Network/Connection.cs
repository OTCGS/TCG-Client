using Network.RawConnections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public class MultiConnection : IUserConnection
    {

        private readonly List<RawConnection> connections = new List<RawConnection>();


        public event Action<byte[]> Recived;



        public User User { get; }

        public ConnectionReason ConnectionReason { get; }
        public Guid DataId { get; }
        public Security.IPublicKey DataKey { get; }

        public MultiConnection(Network.User user, ConnectionReason reason, Guid dataId, Security.IPublicKey dataKey)
        {
            this.User = user;
            this.ConnectionReason = reason;
            this.DataId = dataId;
            this.DataKey = dataKey;
        }

        internal Task Init(RawConnection connection)
        {
            this.connections.Add(connection);

            connection.Recived += DataRecived;
            return Task.FromResult(false);
        }

        private void DataRecived(byte[] data)
        {
            RequestRecived(data);
        }

        private void RequestRecived(byte[] data)
        {
            if (this.Recived != null)
                Recived(data);
        }

        private async Task InternalSend(byte[] message)
        {
            var c = connections.FirstOrDefault(x => x.IsConnected);
            if (c == null)
                throw new InvalidOperationException("No Connection Availible");

            await c.Send(message);
        }

        public async Task Send(byte[] data)
        {
            await InternalSend(data);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var connection in connections)
                        connection.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources. 
        // ~MultiConnection() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}