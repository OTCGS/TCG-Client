using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Network.Socket;
using Security;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Threading;

namespace Network
{
    public class ServerServer : GameServer
    {
        private readonly IService service;
        private readonly IDatagramSocket socket;
        private readonly DatagramSocketEmulator internalsocket;

        private readonly Dictionary<Security.Interfaces.IPublicKeyData, User> KeyUserLookup = new Dictionary<Security.Interfaces.IPublicKeyData, User>(new Security.Interfaces.IPublicKeyComparer());
        private readonly Dictionary<Security.Interfaces.IPublicKeyData, TaskCompletionSource<User>> KeyUserAwaiter = new Dictionary<Security.Interfaces.IPublicKeyData, TaskCompletionSource<User>>(new Security.Interfaces.IPublicKeyComparer());

        public ServerServer(User me, IService service) : base(me)
        {
            this.service = service;
            internalsocket = new DatagramSocketEmulator(service);
            socket = new Socket.ReliableDatagrammSocket(internalsocket, 1024);
            service.DataMessageRecived += Service_DataMessageRecived;
            service.UsersOnServer.CollectionChanged += UsersOnServer_CollectionChanged;
        }

        private async void UsersOnServer_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (Security.Interfaces.IPublicKeyData item in e.NewItems)
                    await service.SendDataMessage(new byte[] { (byte)TransmissionKind.AskUserData }, item);
        }

        private async void Service_DataMessageRecived(Message<byte[]> obj)
        {
            var kind = (TransmissionKind)obj.Data[0];

            User from;
            switch (kind)
            {
                case TransmissionKind.Connect:
                    from = await GetUserFromRecivedData(obj);
                    var connect = new Connect(obj.Data);
                    var multiConnection = new MultiConnection(from, connect.Reason, connect.DataId, connect.DataKey);
                    var directConnection = new RawConnections.ServiceConnection(internalsocket, socket, from);
                    service.IncreaseUse(); // wegen herausgabe von Socket
                    await multiConnection.Init(directConnection);
                    var accept = await FireConnectionRecived(multiConnection);
                    await service.SendDataMessage(new byte[] { (byte)(accept ? TransmissionKind.Accept : TransmissionKind.Disaccept) }, obj.From);
                    break;
                case TransmissionKind.UserData:
                    var xml = Encoding.UTF8.GetString(obj.Data, 1, obj.Data.Length - 1);
                    var user = await Task.Run(() => User.FromXml(xml));
                    if (!KeyUserLookup.ContainsKey(user.PublicKey))
                    {
                        this.KeyUserLookup.Add(user.PublicKey, user);
                        this.users.Add(user);
                    }
                    break;
                case TransmissionKind.AskUserData:
                    var data = new List<byte>(Encoding.UTF8.GetBytes(Me.ToXml()));
                    data.Insert(0, (byte)TransmissionKind.UserData);
                    await service.SendDataMessage(data.ToArray(), obj.From);
                    if (!KeyUserLookup.ContainsKey(obj.From))
                        await service.SendDataMessage(new byte[] { (byte)TransmissionKind.AskUserData }, obj.From);
                    break;
                case TransmissionKind.Accept:
                case TransmissionKind.Disaccept:
                    if (AceptanceAwaiter == null)
                    {
                        Logger.Assert(false, "Wir warten momentan nicht auf eine antwort");
                        return;
                    }
                    AceptanceAwaiter.SetResult(kind == TransmissionKind.Accept);
                    break;
                case TransmissionKind.Text:
                    from = await GetUserFromRecivedData(obj);
                    var msg = Encoding.UTF8.GetString(obj.Data, 1, obj.Data.Length - 1);
                    FireTextMessageRecived(from, msg);
                    break;
                case TransmissionKind.Data:
                    break; // wird wo anders behandelt
                case TransmissionKind.Undefined:
                default:
                    Logger.Assert(false, $"Case {kind} nicht unterstützt.");
                    break;
            }


        }

        private async Task<User> GetUserFromRecivedData(Message<byte[]> obj)
        {
            User from;
            if (KeyUserLookup.ContainsKey(obj.From))
                from = this.KeyUserLookup[obj.From];
            else
            {
                if (!this.KeyUserAwaiter.ContainsKey(obj.From))
                {
                    this.KeyUserAwaiter[obj.From] = new TaskCompletionSource<User>();
                    await service.SendDataMessage(new byte[] { (byte)TransmissionKind.AskUserData }, obj.From);
                }
                from = await this.KeyUserAwaiter[obj.From].Task;
            }

            return from;
        }



        /// <summary>
        /// Dient dazu auf Antworten zum beginnen einer Partie zu warten.
        /// </summary>
        private TaskCompletionSource<bool> AceptanceAwaiter { get; set; }


        public override string Name { get { return service.Name; } }

        public async override Task<IUserConnection> GetConnection(User user, ConnectionReason reason, Guid dataId, IPublicKey dataKey, CancellationToken cancel)
        {
            try
            {
                if (AceptanceAwaiter != null)
                    throw new InvalidOperationException("Es wird noch auf eine Antwort gewartet");
                AceptanceAwaiter = new TaskCompletionSource<bool>();

                var multiConnection = new MultiConnection(user, reason, dataId, dataKey);
                var directConnection = new RawConnections.ServiceConnection(internalsocket, socket, user);
                service.IncreaseUse(); // wegen herausgabe von Socket
                await multiConnection.Init(directConnection);

                var connect = new Connect(reason, dataId, dataKey);
                await service.SendDataMessage(connect.ToByte(), user.PublicKey);
                var answer = await AceptanceAwaiter.Task;
                if (answer)
                    return multiConnection;
                return null;
            }
            finally
            {
                AceptanceAwaiter = null;
            }

        }

        public override Task SendTextMessage(string message)
        {
            var msgBytes = Encoding.UTF8.GetBytes(message);
            var dataArray = new byte[msgBytes.Length + 1];
            dataArray[0] = (byte)TransmissionKind.Text;
            Array.Copy(msgBytes, 0, dataArray, 1, msgBytes.Length);
            return service.SendDataBroadcastMessage(dataArray);
        }

        public interface IService : IDisposable
        {
            string Name { get; }
            ObservableCollection<Security.Interfaces.IPublicKeyData> UsersOnServer { get; }
            event Action<Message<byte[]>> DataMessageRecived;
            Task SendDataMessage(byte[] msg, Security.Interfaces.IPublicKeyData To);
            Task SendDataBroadcastMessage(byte[] msg);
            void IncreaseUse();

        }
        public class Message<T>
        {
            public T Data { get; set; }
            public Security.Interfaces.IPublicKeyData From { get; set; }
        }

        private enum TransmissionKind : byte
        {
            Undefined = 0,
            Connect = 1,
            UserData = 2,
            AskUserData = 3,
            Accept = 4,
            Disaccept = 5,
            Text = 6,
            Data = 7,

        }

        public class Connect
        {
            public Guid DataId { get; }
            public IPublicKey DataKey { get; }
            public ConnectionReason Reason { get; }


            public Connect(ConnectionReason reason, Guid dataId, IPublicKey dataKey)
            {

                this.Reason = reason;
                this.DataId = dataId;
                this.DataKey = dataKey;
            }

            public Connect(byte[] data)
            {
                using (var m = new MemoryStream(data))
                {
                    using (var b = new BinaryReader(m))
                    {
                        var kind = (TransmissionKind)b.ReadByte();
                        if (kind != TransmissionKind.Connect)
                            throw new ArgumentException("Nur COnnect kann zu COnnect deserielisert werden");
                        var count = b.ReadInt32();
                        DataId = new Guid(b.ReadBytes(count));
                        var xml = b.ReadString();
                        if (!String.IsNullOrEmpty(xml))
                        {
                            DataKey = SecurityFactory.CreatePublicKey();
                            DataKey.LoadXml(xml);
                        }
                        Reason = (ConnectionReason)b.ReadByte();

                    }
                }

            }

            public byte[] ToByte()
            {
                using (var m = new MemoryStream())
                {
                    using (var b = new BinaryWriter(m))
                    {
                        b.Write((byte)TransmissionKind.Connect);
                        var bDataId = DataId.ToByteArray();
                        b.Write(bDataId.Length);
                        b.Write(bDataId);
                        b.Write(DataKey.ToXml());
                        b.Write((byte)Reason);
                    }
                    return m.ToArray();
                }


            }
        }

        internal class DatagramSocketEmulator : Network.Socket.IDatagramSocket
        {
            private uint currentCounter = 2;
            internal Dictionary<uint, Security.Interfaces.IPublicKeyData> UserDictionary { get; } = new Dictionary<uint, Security.Interfaces.IPublicKeyData>();
            internal Dictionary<Security.Interfaces.IPublicKeyData, TaskCompletionSource<uint>> UserDictionaryReverse { get; } = new Dictionary<Security.Interfaces.IPublicKeyData, TaskCompletionSource<uint>>(new Security.Interfaces.IPublicKeyComparer());

            public DatagramSocketEmulator(IService service)
            {
                this.service = service;
                service.UsersOnServer.CollectionChanged += Users_CollectionChanged;
                service.DataMessageRecived += Service_DataMessageRecived1;

                foreach (Security.Interfaces.IPublicKeyData item in service.UsersOnServer)
                {
                    var index = currentCounter++;
                    UserDictionary.Add(index, item);
                    if (!UserDictionaryReverse.ContainsKey(item))
                        UserDictionaryReverse.Add(item, new TaskCompletionSource<uint>());
                    UserDictionaryReverse[item].SetResult(index);
                }

            }

            private async void Service_DataMessageRecived1(Message<byte[]> obj)
            {
                if (obj.Data[0] != (byte)TransmissionKind.Data)
                    return;

                if (!UserDictionaryReverse.ContainsKey(obj.From))
                {
                    Logger.Assert(false, "User in Directory nicht gefunden");
                    return;
                }

                var index = await UserDictionaryReverse[obj.From].Task;

                var data = new byte[obj.Data.Length - 1];
                Array.Copy(obj.Data, 1, data, 0, data.Length);

                if (this.MessageRecived != null)
                    this.MessageRecived(this, new MessageRecivedArgs() { Data = data, Host = service.Name, Port = index });
            }

            private async void Users_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                if (e.NewItems != null)
                    foreach (Security.Interfaces.IPublicKeyData item in e.NewItems)
                    {
                        var index = currentCounter++;
                        UserDictionary.Add(index, item);
                        if (!UserDictionaryReverse.ContainsKey(item))
                            UserDictionaryReverse.Add(item, new TaskCompletionSource<uint>());
                        UserDictionaryReverse[item].SetResult(index);
                    }

                if (e.OldItems != null)
                    foreach (Security.Interfaces.IPublicKeyData item in e.OldItems)
                    {
                        Logger.Information($"Key entfehrnt {item.FingerPrint()}");
                        var index = await UserDictionaryReverse[item].Task;
                        UserDictionaryReverse.Remove(item);
                        UserDictionary.Remove(index);
                    }
            }


            public object OriginalSocket { get { return service; } }

            public uint Port { get { return 1; } }

            public event MessageRecivedEvent MessageRecived;

            public Task Bind(uint localPort = 0)
            {
                return Task.FromResult<object>(null);
            }

            public Task Send(byte[] data, string remoteHost, uint port)
            {
                if (!UserDictionary.ContainsKey(port))
                {
                    Logger.Assert(false, "User in Directory nicht gefunden");
                    return Task.FromResult<object>(null);
                }
                var user = UserDictionary[port];
                var toSend = new byte[data.Length + 1];
                toSend[0] = (byte)TransmissionKind.Data;
                Array.Copy(data, 0, toSend, 1, data.Length);
                try
                {
                    return service.SendDataMessage(toSend, user);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    throw;
                }
            }

            #region IDisposable Support
            private readonly IService service;


            public void Dispose()
            {
                this.service.Dispose();
            }

            internal Task<uint> GetPort(IPublicKey publicKey)
            {
                if (!UserDictionaryReverse.ContainsKey(publicKey))
                    UserDictionaryReverse.Add(publicKey, new TaskCompletionSource<uint>());
                return UserDictionaryReverse[publicKey].Task;
            }
            #endregion
        }
    }
}
