using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.RawConnections
{
    internal abstract class RawConnection : IConnection
    {
        public abstract bool IsConnected { get; }

        public event Action<byte[]> Recived;

        protected readonly Socket.IDatagramSocket socket;
        private bool running;

        public RawConnection(Network.Socket.IDatagramSocket socket)
        {
            this.socket = socket;
            socket.MessageRecived += SocketMessageRecived;
        }

        protected void SocketMessageRecived(object sender, Socket.MessageRecivedArgs args)
        {
            bool discard;
            var data = OnMessageRecived(sender, args, out discard);
            if (discard)
                return;
            FireRecive(data);
        }

        protected void FireRecive(byte[] data)
        {
            if (Recived != null)
                Recived(data);
        }

        protected virtual byte[] OnMessageRecived(object sender, Socket.MessageRecivedArgs args, out bool discard)
        {
            discard = false;
            return args.Data;
        }

        public abstract Task Send(byte[] data);

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    socket.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources. 
        // ~RawConnection() {
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