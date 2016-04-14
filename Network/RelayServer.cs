using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Network
{
    internal class RelayServer : GameServer
    {
        private bool running;

        public override string Name { get { return name; }  }

        private readonly Dictionary<User, uint> idLookup = new Dictionary<User, uint>();
        private static readonly byte[] INIT_CONECTION_MAGIC = new byte[] { 0x9F };
        private string name;

        public uint RelayId { get; private set; }

        private Relay.Client.RelayClientSocket RelaySocket { get; set; }

        public RelayServer(User me) : base(me)
        {
        }

        public async Task Init(String server, uint port)
        {
            this.name = server;

            var underlyingSocket = Socket.SocketFactory.CreateDatagramsocket();
            await underlyingSocket.Bind();

            var userData = UTF8Encoding.UTF8.GetBytes(Me.ToXml());

            RelaySocket = Relay.Client.RelayClientSocket.Create(userData, underlyingSocket, server, port);

            server = server.TrimEnd('/');
            var uri = new Uri(server + ":" + port.ToString(), UriKind.Absolute);

            StartPoll(uri);

            await RelaySocket.Bind();
            this.RelayId = RelaySocket.Port;

            WaitForConnections();

        }

        private async void WaitForConnections()
        {
            var relayConnection = new RawConnections.RelayConnection(this.RelaySocket, 0);
            var initiator = new InitConnectivity(relayConnection);
            while (true)
            {
                var id = await initiator.Recive();
                var user = idLookup.FirstOrDefault(x => x.Value == id).Key;
                relayConnection = new RawConnections.RelayConnection(this.RelaySocket, id);
                initiator = new InitConnectivity(relayConnection);
                if (user == null)
                {
                    await initiator.SendMessage(0);
                    continue;
                }
                await initiator.SendMessage(this.RelayId);
                this.FireConnectionRecived(new RawConnections.UserConnectionEnvalop(user, relayConnection, ConnectionReason.Unkonwon, Guid.Empty, null));
            }
        }

        public override async Task<IUserConnection> GetConnection(User user, ConnectionReason reason, Guid dataId, Security.IPublicKey dataKey, System.Threading.CancellationToken cancel)
        {
            var id = idLookup[user];

            var relayConnection = new RawConnections.RelayConnection(this.RelaySocket, id);
            var initConection = new InitConnectivity(relayConnection);
            await initConection.SendMessage(this.RelayId);
            if (id != await initConection.Recive())
                throw new Exception("Verbindung verweigert");

            return new RawConnections.UserConnectionEnvalop(user, relayConnection, reason, dataId, dataKey);

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.running = false;
        }

        private async void StartPoll(Uri uri)
        {
            running = true;
            while (running)
            {
                var request = System.Net.HttpWebRequest.CreateHttp(uri);
                var response = await request.GetResponseAsync();

                using (var stream = new System.IO.StreamReader(response.GetResponseStream()))
                {
                    var xmlText = await stream.ReadToEndAsync();
                    var xml = XElement.Parse(xmlText);
                    var clients = xml.Nodes().OfType<XElement>().Where(x => x.Name == "Client").Select(x =>
                    {
                        var id = uint.Parse(x.Nodes().OfType<XElement>().First(y => y.Name == "Id").Value);
                        var user = Network.User.FromXml(x.Nodes().OfType<XElement>().First(y => y.Name == "User").ToString());

                        return new { Id = id, User = user };

                    });



                    var usersToAdd = clients.Select(x => x.User).Where(x => !this.Users.Contains(x)).Distinct().ToList();
                    var usersToRemove = this.Users.Where(x => !clients.Select(y => y.User).Contains(x)).ToList();

                    foreach (var u in usersToRemove)
                    {
                        this.users.Remove(u);
                        this.idLookup.Remove(u);
                    }
                    foreach (var u in usersToAdd)
                    {
                        this.users.Add(u);
                        this.idLookup.Add(u, clients.First(x => x.User.Equals(u)).Id);
                    }

                }
                await Task.Delay(1000 * 20);
            }
        }

        public override Task SendTextMessage(string message)
        {
            throw new NotImplementedException();
        }

        private class InitConnectivity : AbstractConnectivity<uint>
        {
            public InitConnectivity(IConnection connection) : base(connection, INIT_CONECTION_MAGIC)
            {

            }
            protected override Task<uint> ConvertFromByte(byte[] data)
            {
                return Task.FromResult(BitConverter.ToUInt32(data, 0));
            }

            protected override Task<byte[]> ConvertToByte(uint data)
            {
                return Task.FromResult(BitConverter.GetBytes(data));
            }
        }
    }
}