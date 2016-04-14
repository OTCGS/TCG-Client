using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public abstract  class GameServer : IDisposable
    {
        protected User Me { get; private set; }

        protected GameServer(User me)
        {
            this.Me = me;
        }

        protected readonly ObservableCollection<User> users = new ObservableCollection<User>();
        private static LocalServer localServer;
        private bool disposed;

        public event Func<IUserConnection, Task<bool>> ConnectionRecived;
        public event Action<User, String> TextMessageRecived;

        protected void FireTextMessageRecived(User user, String msg)
        {
            if (this.TextMessageRecived != null)
                TextMessageRecived(user, msg);
        }

        public abstract Task SendTextMessage(String message);

        public ReadOnlyObservableCollection<User> Users
        {
            get { return new ReadOnlyObservableCollection<User>(users); }
        }

        public static async Task<GameServer> GetServer(string uri, User me)
        {

            bool isRelayServer = true;

            if (isRelayServer)
            {

                var server = new RelayServer(me);

                await server.Init(uri, (uint)Network.Relay.Protocol.PORT);
                return server;

            }

            throw new NotImplementedException();

        }

        public static GameServer GetLocalPeers(User user)
        {
            if (localServer == null || localServer.disposed)
                localServer = new LocalServer(user);
            return (localServer);
        }

        /// <summary>
        /// Erstellt eine Connection zu dem Angegebenen Nutzer
        /// </summary>
        /// <param name="user">Der Nutzer zu dem eine Verbindung erstellt werden soll</param>
        /// <returns>Gibt eine Verbindung zu dem gewünschten Nutzer zurück. Oder null falls dieser die Verbindung verweigert.</returns>
        public abstract Task<IUserConnection> GetConnection(User user, ConnectionReason reason, Guid dataId, Security.IPublicKey dataKey, System.Threading.CancellationToken cancel = default(System.Threading.CancellationToken));

        protected Task<bool> FireConnectionRecived(IUserConnection c)
        {
            if (ConnectionRecived != null)
            {
                if (ConnectionRecived.GetInvocationList().Length != 1)
                    throw new InvalidOperationException("Es dürfte nur 1 listener angemeldet sein");
                return ConnectionRecived(c);
            }
            return Task.FromResult(false);
        }
        #region Dispose

        ~GameServer()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.

                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                // Note disposing has been done.
                disposed = true;

            }
        }

        #endregion

        public abstract string Name { get;  }
    }
}