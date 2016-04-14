using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Network.Socket;
using Security;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Network
{
    internal class LocalServer : GameServer
    {
        private const string M_GROUP = "224.5.6.7";

        private readonly Guid guid = Guid.NewGuid();
        private Socket.IDatagramSocket socket;
        private const int M_PORT = 40404;


        private readonly Dictionary<Tuple<String, uint>, User> UserLookup = new Dictionary<Tuple<String, uint>, User>();
        private const int GUID_LENGTH = 16;


        private System.Threading.SemaphoreSlim semaphor = new System.Threading.SemaphoreSlim(1, 1);

        internal LocalServer(User me) : base(me)
        {
            this.Name = "Lokales Netzwerk";
            SearchForPeers();
        }

        protected async void SearchForPeers()
        {
            if (DesignMode.Enabled)
                return;
            var socket = Socket.SocketFactory.CreateDatagramsocket();
            imageSocket = Socket.SocketFactory.CreateTcpListener();

            imageSocket.SocketRecived += (sender, e) =>
            {
                if (Me.Image == null)
                    e.Socket.Send(new byte[0]);
                else
                    e.Socket.Send(Me.Image);
            };
            await imageSocket.Bind();

            socket.MessageRecived += socket_MessageRecived;
            await socket.Bind(40404);
            await socket.JounMulticastGroup(M_GROUP);
            this.socket = socket;

            var data = GenerateJoin();
            await Send(data);
        }

        public override async Task SendTextMessage(string message)
        {
            var data = GenerateJoin(message);
            await Send(data);
        }

        private byte[] GenerateJoin(string message = "")
        {
            byte[] data;
            using (var mem = new System.IO.MemoryStream())
            {
                System.IO.BinaryWriter b = new System.IO.BinaryWriter(mem);
                b.Write((byte)MessageTypes.Join);
                b.Write(Me.PublicKey.ToXml());
                b.Write(Me.Name);
                b.Write(imageSocket.Port);
                b.Write(message);
                data = mem.ToArray();
            }
            return data;
        }

        private async Task<bool> ProcessJoin(byte[] data, Socket.MessageRecivedArgs args)
        {
            try
            {
                using (var mem = new System.IO.MemoryStream(data))
                {
                    var b = new System.IO.BinaryReader(mem);
                    var type = (MessageTypes)b.ReadByte();
                    var doNotRespond = type.HasFlag(MessageTypes.DoNotRespond);
                    if (!type.HasFlag(MessageTypes.Join))
                        return false;
                    var cert = b.ReadString();
                    var name = b.ReadString();
                    var imagePort = b.ReadUInt16();
                    var message = b.ReadString();

                    var user = new User();
                    user.PublicKey = Security.SecurityFactory.CreatePublicKey().LoadXml(cert);
                    user.Name = name;

                    try
                    {
                        var tcp = Network.Socket.SocketFactory.CreateTcpsocket();
                        TaskCompletionSource<byte[]> image = new TaskCompletionSource<byte[]>();
                        tcp.MessageRecived += (s, e) =>
                        {
                            try
                            {
                                if (e.Data.Length == 0)
                                    image.SetResult(null);
                                else
                                    image.SetResult(e.Data);
                            }
                            catch (Exception ex)
                            {
                                image.SetException(ex);
                            }
                        };
                        await tcp.Connect(args.Host, imagePort);

                        user.Image = await image.Task;
                    }
                    catch (Exception)
                    { }
                    if (!this.Users.Contains(user))
                        this.users.Add(user);

                    UserLookup[new Tuple<string, uint>(args.Host, args.Port)] = user;

                    if (!String.IsNullOrWhiteSpace(message))
                        this.FireTextMessageRecived(user, message);

                    if (!doNotRespond)
                    {
                        var join = GenerateJoin();
                        join[0] |= (byte)MessageTypes.DoNotRespond;
                        await Send(join, args.Host, args.Port);
                    }

                    return true;
                }
            }
            catch (Exception)
            {
                System.Diagnostics.Debugger.Break();
            }
            return false;
        }

        private async Task Send(byte[] p, string host = M_GROUP, uint port = M_PORT)
        {
            await socket.Send(guid.ToByteArray().Concat(p).ToArray(), host, port);
        }

        private async void socket_MessageRecived(object sender, Socket.MessageRecivedArgs args)
        {
            if (guid.ToByteArray().SequenceEqual(args.Data.Take(GUID_LENGTH)))
                return;

            try
            {

                await semaphor.WaitAsync();
                var data = args.Data.Skip(GUID_LENGTH).ToArray();

                var type = (MessageTypes)data[0];
                var doNotRespond = type.HasFlag(MessageTypes.DoNotRespond);

                await ProcessJoin(data, args);
                ProcessLeave(data, args);
                await ProcessConnect(data, args);
                ProcessAcceptConnect(data, args);

            }
            finally
            {
                semaphor.Release();

            }

        }

        private void ProcessAcceptConnect(byte[] data, MessageRecivedArgs args)
        {
            using (var mem = new System.IO.MemoryStream(data))
            {
                var b = new System.IO.BinaryReader(mem);
                var type = (MessageTypes)b.ReadByte();
                if (!type.HasFlag(MessageTypes.AcceptConnect))
                    return;
                var accepted = b.ReadBoolean();
                if (AcceptConnectionGuard.ContainsKey(new Tuple<string, uint>(args.Host, args.Port)))
                {
                    var guard = AcceptConnectionGuard[new Tuple<string, uint>(args.Host, args.Port)];
                    guard.SetResult(accepted);
                }

            }
        }

        private async Task ProcessConnect(byte[] data, Socket.MessageRecivedArgs args)
        {

            using (var mem = new System.IO.MemoryStream(data))
            {
                var b = new System.IO.BinaryReader(mem);
                var type = (MessageTypes)b.ReadByte();
                if (!type.HasFlag(MessageTypes.Connect))
                    return;

                var reason = (ConnectionReason)b.ReadByte();
                var port = b.ReadUInt32();
                var dataId = new Guid(b.ReadBytes(16));
                var dataKeyModulusLength = b.ReadInt32();
                var dataKeyModulus = b.ReadBytes(dataKeyModulusLength);
                var dataKeyExponentLength = b.ReadInt32();
                var dataKeyExponent = b.ReadBytes(dataKeyExponentLength);

                IPublicKey dataKey;
                if (dataKeyModulus.Length == 0)
                    dataKey = null;
                else
                {
                    dataKey = Security.SecurityFactory.CreatePublicKey();
                    dataKey.SetKey(dataKeyModulus, dataKeyExponent);
                }
                if (ConnectionGuard.ContainsKey(new Tuple<string, uint>(args.Host, args.Port)))
                {
                    var guard = ConnectionGuard[new Tuple<string, uint>(args.Host, args.Port)];
                    guard.SetResult(Tuple.Create(port, reason, dataId, dataKey));
                }
                else
                {
                    var user = UserLookup[new Tuple<string, uint>(args.Host, args.Port)];
                    if (user == null)
                    {
                        System.Diagnostics.Debugger.Break();
                        return;
                    }
                    var connection = new MultiConnection(user, reason, dataId, dataKey);

                    var socket = Socket.SocketFactory.CreateReliableDatagramsocket();
                    await socket.Bind();
                    var direcConnection = new RawConnections.DirectConnection(socket);

                    await this.Send(GenerateConnect(socket.Port, reason, dataId, dataKey), args.Host, args.Port);

                    direcConnection.SetOtherPort(args.Host, port);
                    await connection.Init(direcConnection);
                    var accept = await FireConnectionRecived(connection);

                    var acceptMessage = GenerateAcceptConnect(accept);
                    await Send(acceptMessage, args.Host, args.Port);

                }
            }
        }

        public override async Task<IUserConnection> GetConnection(User user, ConnectionReason reason, Guid dataId, IPublicKey dataKey, System.Threading.CancellationToken cancel)
        {
            try
            {

                await semaphor.WaitAsync();
                var userData = this.UserLookup.FirstOrDefault(x => x.Value.Equals(user)).Key;
                if (userData == null)
                    throw new ArgumentException("User Not Found");

                if (ConnectionGuard.ContainsKey(userData))
                    ConnectionGuard[userData].SetException(new TimeoutException("Annother Call tried to reach this user lead to a Timout on this Call"));

                var connection = new MultiConnection(user, reason, dataId, dataKey);

                var socket = Socket.SocketFactory.CreateReliableDatagramsocket();
                await socket.Bind();
                var direcConnection = new RawConnections.DirectConnection(socket);

                var task = new TaskCompletionSource<Tuple<uint, ConnectionReason, Guid, IPublicKey>>();
                ConnectionGuard[userData] = task;

                await this.Send(GenerateConnect(socket.Port, reason, dataId, dataKey), userData.Item1, userData.Item2);

                // An Dieser Stelle muss der Lock aufgehoben werden. Da wir auf eine Antwort warten müssen :(
                semaphor.Release();
                var otherPort = await task.Task;
                await semaphor.WaitAsync();


                ConnectionGuard.Remove(userData);
                direcConnection.SetOtherPort(userData.Item1, otherPort.Item1);
                await connection.Init(direcConnection);

                // Überprüfen ob bereits auf eine Connection gewartet wird
                if (AcceptConnectionGuard.ContainsKey(userData))
                    AcceptConnectionGuard[userData].SetException(new TimeoutException("Annother Call tried to reach this user lead to a Timout on this Call"));
                var acceptWait = new TaskCompletionSource<bool>();
                AcceptConnectionGuard[userData] = acceptWait;

                // An Dieser Stelle muss der Lock aufgehoben werden. Da wir auf eine Antwort warten müssen :(
                semaphor.Release();
                var accept = await acceptWait.Task;
                await semaphor.WaitAsync();

                AcceptConnectionGuard.Remove(userData);
                if (accept)
                    return connection;
                return null;
            }
            finally
            {
                try
                {
                    semaphor.Release();
                }
                catch (System.Threading.SemaphoreFullException)
                {
                    // Im ungünstigen fall könnte eine exception fliegen, nachdem das semaphor releasd wurde, aber bevor es wider geWaited wurde
                    // (Schreibt man das so?? Hört sich furchbar falsch an ( -_-); )
                }

            }
        }

        /// <summary>
        /// Warted auf die Connection Parameter des anderen Users
        /// </summary>
        private Dictionary<Tuple<string, uint>, TaskCompletionSource<Tuple<uint, ConnectionReason, Guid, IPublicKey>>> ConnectionGuard = new Dictionary<Tuple<string, uint>, TaskCompletionSource<Tuple<uint, ConnectionReason, Guid, IPublicKey>>>();
        /// <summary>
        /// Warted darauf ob der andere User die übermittelte Connection annehmen will.
        /// </summary>
        private Dictionary<Tuple<string, uint>, TaskCompletionSource<bool>> AcceptConnectionGuard = new Dictionary<Tuple<string, uint>, TaskCompletionSource<bool>>();
        private Socket.ITcpSocketListener imageSocket;

        public override string Name { get; }

        private byte[] GenerateConnect(UInt32 port, ConnectionReason reason, Guid dataId, IPublicKey dataKey)
        {
            byte[] data;
            using (var mem = new System.IO.MemoryStream())
            {



                System.IO.BinaryWriter b = new System.IO.BinaryWriter(mem);
                b.Write((byte)MessageTypes.Connect);
                b.Write((byte)reason);
                b.Write(port);
                b.Write(dataId.ToByteArray());
                if (dataKey == null)
                {
                    b.Write(0);
                    b.Write(0);
                }
                else
                {
                    b.Write(dataKey.Modulus.Length);
                    b.Write(dataKey.Modulus);
                    b.Write(dataKey.Exponent.Length);
                    b.Write(dataKey.Exponent);
                }
                data = mem.ToArray();
            }
            return data;
        }

        private byte[] GenerateAcceptConnect(bool accept)
        {
            byte[] data;
            using (var mem = new System.IO.MemoryStream())
            {
                System.IO.BinaryWriter b = new System.IO.BinaryWriter(mem);
                b.Write((byte)MessageTypes.AcceptConnect);
                b.Write(accept);
                data = mem.ToArray();
            }
            return data;
        }

        private byte[] GenerateLeave()
        {
            byte[] data;
            using (var mem = new System.IO.MemoryStream())
            {
                System.IO.BinaryWriter b = new System.IO.BinaryWriter(mem);
                b.Write((byte)MessageTypes.Leave);
                data = mem.ToArray();
            }
            return data;
        }


        private void ProcessLeave(byte[] data, Socket.MessageRecivedArgs args)
        {

            using (var mem = new System.IO.MemoryStream(data))
            {
                var b = new System.IO.BinaryReader(mem);
                var type = (MessageTypes)b.ReadByte();
                var doNotRespond = type.HasFlag(MessageTypes.DoNotRespond);
                if (!type.HasFlag(MessageTypes.Leave))
                    return;
            }

            var userdata = Tuple.Create(args.Host, args.Port);
            if (!this.UserLookup.ContainsKey(userdata))
                return;
            var user = this.UserLookup[userdata];
            this.users.Remove(user);

        }

        public async Task Leave()
        {
            await this.Send(GenerateLeave());

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DisposintLeave();
        }

        private async void DisposintLeave()
        {
            await semaphor.WaitAsync(10000);
            await this.Leave();
            this.socket.Dispose();
            semaphor.Dispose();
        }

        [Flags]
        private enum MessageTypes : byte
        {
            Join = 1,
            Leave = 2,
            DoNotRespond = 4,
            Connect = 8,
            AcceptConnect = 16,
        }




    }
}