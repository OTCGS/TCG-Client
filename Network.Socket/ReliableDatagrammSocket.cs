using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Network.Socket
{
    public class ReliableDatagrammSocket : IDatagramSocket
    {
        private static readonly byte[] MAGIC = new byte[] { 0xF3 };

        private readonly IDatagramSocket original;
        private readonly int maxSize;

        private const int SHORT_HEADER_LENGTH = 4;
        private const int LONG_HEADER_LENGTH = 6;

        private readonly ConcurrentDictionary<Tuple<string,uint,ushort>, AwaitableConcurrentQueue<IReciver>> recivingBackingstore = new ConcurrentDictionary<Tuple<string, uint, ushort>, AwaitableConcurrentQueue<IReciver>>();
        private readonly ConcurrentDictionary<Tuple<string, uint, ushort>, AwaitableConcurrentQueue<ISender>> sendingBackingstore = new ConcurrentDictionary<Tuple<string, uint, ushort>, AwaitableConcurrentQueue<ISender>>();
        private readonly ConcurrentDictionary<Tuple<string, uint, ushort>, object> recivedId = new ConcurrentDictionary<Tuple<string, uint, ushort>, object>();

        public ReliableDatagrammSocket(IDatagramSocket original, int maxSize)
        {
            this.original = original;
            this.maxSize = maxSize;
            original.MessageRecived += Original_MessageRecived;
        }

        private bool IsMagic(byte[] headder)
        {
            return MAGIC.SequenceEqual(headder.Take(MAGIC.Length));
        }

        private void SetMagic(byte[] headder)
        {
            Array.Copy(MAGIC, headder, MAGIC.Length);
        }

        private async void Original_MessageRecived(object sender, MessageRecivedArgs args)
        {
            if (!IsMagic(args.Data))
                return;

            var msg = GenerateHeadder(args.Data);
            
            if (msg is ISender)
            {
                AwaitableConcurrentQueue<ISender> queue;
                var succes = this.sendingBackingstore.TryGetValue(Tuple.Create(args.Host,args.Port, msg.Id), out  queue);
                if (!succes)
                    return;
                queue.Enqueue(msg as ISender);
            }
            if (msg is IReciver)
            {
                if (recivedId.ContainsKey(Tuple.Create(args.Host, args.Port, msg.Id)))
                {
                    if (msg is EndCommunicationHeadder || msg is SingelDataHadder)
                        await Send(new AcknolageEndComunicationHeadder(msg.Id), args.Host, args.Port);

                    return;
                }
                AwaitableConcurrentQueue<IReciver> queue;
                var succes = this.recivingBackingstore.TryGetValue(Tuple.Create(args.Host, args.Port, msg.Id), out  queue);
                if (!succes)
                {
                    var newqueue = new AwaitableConcurrentQueue<IReciver>();
                    queue = this.recivingBackingstore.GetOrAdd(Tuple.Create(args.Host, args.Port, msg.Id), newqueue);
                    if (newqueue == queue)
                    {
                        ProcessRecivint(queue, args.Host, args.Port);
                    }
                }
                queue.Enqueue(msg as IReciver);
            }
        }

        private System.Threading.SemaphoreSlim reciveGuard = new System.Threading.SemaphoreSlim(1, 1);

        private async void ProcessRecivint(AwaitableConcurrentQueue<IReciver> queue, string host, uint port)
        {
            await Task.Yield();
            await reciveGuard.WaitAsync();
            var finished = false;
            var msg = await queue.DeQueue();
            var id = msg.Id;
            DataHadder[] data = null;
            if (msg is StartComunication)
            {
                await Send(new AcknolageStartComunicationHeadder(id), host, port);
                data = new DataHadder[(msg as StartComunication).PackageCount];
            }
            else if (msg is SingelDataHadder)
            {
                await Send(new AcknolageEndComunicationHeadder(id), host, port);
                var d = msg as SingelDataHadder;
                FireMessageRecived(new MessageRecivedArgs() { Data = d.PackageLoad, Host = host, Port = port });
                recivedId.GetOrAdd(Tuple.Create(host, port, msg.Id), this);
                finished = true;
            }
            else
                finished = true;

            if (msg is EndCommunicationHeadder)
                await Send(new AcknolageEndComunicationHeadder(id), host, port);

            while (!finished)
            {
                msg = await queue.DeQueue();
                if (msg is DataHadder)
                {
                    var d = msg as DataHadder;
                    data[d.Index] = d;
                }
                else if (msg is StartComunication)
                {
                    // Nochmals senden vieleicht hatt er es nicht bekommen
                    await Send(new AcknolageStartComunicationHeadder(id), host, port);
                }
                else if (msg is EndCommunicationHeadder)
                {
                    var missingData = data.Select((x, index) => new { D = x, Index = (UInt16)index }).Where(x => x.D == null).Select(x => x.Index);
                    if (missingData.Any())
                    {
                        foreach (var missing in missingData)
                            await Send(new ResendDataHeadder(id, missing), host, port);
                    }
                    else
                    {
                        recivedId.GetOrAdd(Tuple.Create(host, port, msg.Id), this);
                        await Send(new AcknolageEndComunicationHeadder(id), host, port);
                        FireMessageRecived(new MessageRecivedArgs() { Data = data.SelectMany(x => x.PackageLoad).ToArray(), Host = host, Port = port });
                        finished = true;
                    }
                }
                else if (msg is ErrorHeadder)
                {
                    finished = true;
                }
                else
                {
                    throw new System.Net.ProtocolViolationException("Type Nicht unterstützt: " + msg.Type);
                }
            }
            AwaitableConcurrentQueue<IReciver> @null;
            while (!recivingBackingstore.TryRemove(Tuple.Create(host, port, msg.Id), out @null)) ;
            reciveGuard.Release();
        }

        private MessageHeadder GenerateHeadder(Byte[] b)
        {
            var m = (Message)b[1];
            switch (m)
            {
                case Message.StartComunication:
                    return new StartComunication(b);

                case Message.AcknolageStartComunication:
                    return new AcknolageStartComunicationHeadder(b);

                case Message.Data:
                    return new DataHadder(b);

                case Message.ResendData:
                    return new ResendDataHeadder(b);

                case Message.EndComunication:
                    return new EndCommunicationHeadder(b);

                case Message.AcknolageEndComunication:
                    return new AcknolageEndComunicationHeadder(b);

                case Message.Error:
                    return new ErrorHeadder(b);

                case Message.SingelPackage:
                    return new SingelDataHadder(b);

                default:
                    throw new NotImplementedException();
            }
        }

        private void FireMessageRecived(MessageRecivedArgs args)
        {
            if (this.MessageRecived != null)
                MessageRecived(this, args);
        }

        public object OriginalSocket { get { return original; } }

        public uint Port { get { return original.Port; } }

        public int MaximumPackageSize { get { return maxSize; } }

        public event MessageRecivedEvent MessageRecived;

        public Task Bind(uint localPort = 0)
        {
            return original.Bind(localPort);
        }

        public void Dispose()
        {
            original.Dispose();
            reciveGuard.Dispose();
        }

        private UInt16 nextID;

        private UInt16 GenerateNextId()
        {
            return ++nextID;
        }

        public async Task Send(byte[] data, string remoteHost, uint port)
        {
            const int timeout = 200;
            var id = GenerateNextId();
            var message = GenerateMessages(data, id).ToArray();
            var start = new StartComunication(id, (UInt16)message.Length);
            var queue = sendingBackingstore.GetOrAdd(Tuple.Create(remoteHost, port, id), new AwaitableConcurrentQueue<ISender>());

            if (message.Length == 1)
            {
                var shortMsg = new SingelDataHadder(id, message[0].PackageLoad);
                var waitForAcknolage = queue.DeQueue();
                MessageHeadder acknolageMessage = null;
                do
                {
                    await Send(shortMsg, remoteHost, port);
                    var timeoutTask = Task.Delay(timeout);
                    var winner = await Task.WhenAny(waitForAcknolage, timeoutTask);
                    if (winner != timeoutTask)
                        acknolageMessage = (await waitForAcknolage) as AcknolageEndComunicationHeadder;
                } while (acknolageMessage == null);

                return;
            }
            else
            {
                var waitForAcknolage = queue.DeQueue();
                MessageHeadder acknolageMessage = null;
                do
                {
                    await Send(start, remoteHost, port);
                    var timeoutTask = Task.Delay(timeout);
                    var winner = await Task.WhenAny(waitForAcknolage, timeoutTask);
                    if (winner != timeoutTask)
                        acknolageMessage = (await waitForAcknolage) as AcknolageStartComunicationHeadder;
                } while (acknolageMessage == null);

                foreach (var m in message)
                    await Send(m, remoteHost, port);

                var finishMessage = new EndCommunicationHeadder(id);
                AcknolageEndComunicationHeadder acknolageFinish = null;
                do
                {
                    await Send(finishMessage, remoteHost, port);

                    var sendAgaine = false;
                    while (!sendAgaine) // Falls der Andre Client uns mehrere Resend Header Sendet, arbeiten wir diese erst einemal ab Bevor wir nochmal versuchen die Komunikation zu beenden.
                    {
                        var timeoutTask = Task.Delay(timeout);
                        var winner = await Task.WhenAny(waitForAcknolage, timeoutTask);
                        if (winner != timeoutTask)
                        {
                            var msg = await waitForAcknolage;
                            waitForAcknolage = queue.DeQueue();
                            if (msg is ErrorHeadder)
                                throw new System.Net.WebException();
                            if (msg is AcknolageEndComunicationHeadder)
                                acknolageFinish = msg as AcknolageEndComunicationHeadder;
                            else if (msg is ResendDataHeadder)
                            {
                                var resend = msg as ResendDataHeadder;
                                await Send(message[resend.Index], remoteHost, port);
                            }
                            else if (msg is AcknolageStartComunicationHeadder)
                            {
                                // Ignorieren, wir habens schonal erhalten.
                            }
                            else
                                throw new System.Net.ProtocolViolationException("Type not Suported" + msg.Type.ToString());
                        }
                        else
                            sendAgaine = true; // bei einem Timeout sollten wir nochmal senden.
                    }
                } while (acknolageFinish == null);
            }
        }

        private async Task Send(MessageHeadder start, string remoteHost, uint port)
        {
            await original.Send(start.Bytes, remoteHost, port);
        }

        private IEnumerable<DataHadder> GenerateMessages(byte[] data, UInt16 id)
        {
            int dataSize = MaximumPackageSize - LONG_HEADER_LENGTH;
            UInt16 packageCount = (UInt16)Math.Ceiling((double)data.Length / dataSize);
            for (int i = 0, packageIndex = 0; i < data.Length; i += dataSize, packageIndex++)
            {
                // Erstelle Package. Header + Der rest der Nutzdaten oder falls dies
                // zuviele sind, so viele wie möglich.
                int amountOfDataInThisPackage = Math.Min(data.Length - i, dataSize);
                var b = new byte[amountOfDataInThisPackage];
                Array.Copy(data, i, b, 0, amountOfDataInThisPackage);
                yield return new DataHadder(id, (UInt16)packageIndex, b);
            }
        }

        #region Headder

        private class StartComunication : MessageHeadder, IReciver
        {
            public UInt16 PackageCount { get; private set; }

            public StartComunication(UInt16 id, UInt16 packageCount)
            {
                this.PackageCount = packageCount;
                this.Id = id;

                Bytes = new byte[6];
                Bytes[0] = MAGIC[0];
                Bytes[1] = (byte)Message.StartComunication;
                Array.Copy(BitConverter.GetBytes(id), 0, Bytes, 2, 2);
                Array.Copy(BitConverter.GetBytes(packageCount), 0, Bytes, 4, 2);
            }

            public StartComunication(byte[] b)
            {
                if (b.Length != 6)
                    throw new ArgumentException("Länge des Package Stimmt nicht");
                if (b[0] != MAGIC[0])
                    throw new ArgumentException("Falsches Magic Byte");
                if (b[1] != (byte)Message.StartComunication)
                    throw new ArgumentException("Falscher Message Type");
                this.Bytes = b;
                this.Id = BitConverter.ToUInt16(b, 2);
                this.PackageCount = BitConverter.ToUInt16(b, 4);
            }
        }

        private class AcknolageStartComunicationHeadder : ShortHeadder, ISender
        {
            public AcknolageStartComunicationHeadder(UInt16 id) : base(id, Message.AcknolageStartComunication)
            {
            }

            public AcknolageStartComunicationHeadder(byte[] b) : base(b, Message.AcknolageStartComunication)
            {
            }
        }

        private class DataHadder : MessageHeadder, IReciver
        {
            public UInt16 Index { get; private set; }

            public byte[] PackageLoad { get; private set; }

            public DataHadder(UInt16 id, UInt16 index, byte[] data)
            {
                this.Id = id;
                this.Index = index;
                Bytes = new byte[6 + data.Length];
                Bytes[0] = MAGIC[0];
                Bytes[1] = (byte)Message.Data;
                Array.Copy(BitConverter.GetBytes(id), 0, Bytes, 2, 2);
                Array.Copy(BitConverter.GetBytes(index), 0, Bytes, 4, 2);
                PackageLoad = new byte[data.Length];
                Array.Copy(data, PackageLoad, data.Length);
                Array.Copy(data, 0, Bytes, 6, data.Length);
            }

            public DataHadder(byte[] b)
            {
                if (b.Length < 4)
                    throw new ArgumentException("Länge des Package Stimmt nicht");
                if (b[0] != MAGIC[0])
                    throw new ArgumentException("Falsches Magic Byte");
                if (b[1] != (byte)Message.Data)
                    throw new ArgumentException("Falscher Message Type");
                this.Bytes = b;
                this.Id = BitConverter.ToUInt16(b, 2);
                this.Index = BitConverter.ToUInt16(b, 4);
                PackageLoad = new byte[b.Length - 6];
                Array.Copy(b, 6, PackageLoad, 0, PackageLoad.Length);
            }
        }

        private class ResendDataHeadder : MessageHeadder, ISender
        {
            public UInt16 Index { get; private set; }

            public ResendDataHeadder(UInt16 id, UInt16 index)
            {
                this.Index = index;
                this.Id = id;

                Bytes = new byte[6];
                Bytes[0] = MAGIC[0];
                Bytes[1] = (byte)Message.ResendData;
                Array.Copy(BitConverter.GetBytes(id), 0, Bytes, 2, 2);
                Array.Copy(BitConverter.GetBytes(index), 0, Bytes, 4, 2);
            }

            public ResendDataHeadder(byte[] b)
            {
                if (b.Length != 6)
                    throw new ArgumentException("Länge des Package Stimmt nicht");
                if (b[0] != MAGIC[0])
                    throw new ArgumentException("Falsches Magic Byte");
                if (b[1] != (byte)Message.ResendData)
                    throw new ArgumentException("Falscher Message Type");
                this.Bytes = b;
                this.Id = BitConverter.ToUInt16(b, 2);
                this.Index = BitConverter.ToUInt16(b, 4);
            }
        }

        private class EndCommunicationHeadder : ShortHeadder, IReciver
        {
            public EndCommunicationHeadder(UInt16 id) : base(id, Message.EndComunication)
            {
            }

            public EndCommunicationHeadder(byte[] b) : base(b, Message.EndComunication)
            {
            }
        }

        private class AcknolageEndComunicationHeadder : ShortHeadder, ISender
        {
            public AcknolageEndComunicationHeadder(UInt16 id) : base(id, Message.AcknolageEndComunication)
            {
            }

            public AcknolageEndComunicationHeadder(byte[] b) : base(b, Message.AcknolageEndComunication)
            {
            }
        }

        private class ErrorHeadder : ShortHeadder
        {
            public ErrorHeadder(UInt16 id) : base(id, Message.Error)
            {
            }

            public ErrorHeadder(byte[] b) : base(b, Message.Error)
            {
            }
        }

        private class SingelDataHadder : MessageHeadder, IReciver
        {
            public byte[] PackageLoad { get; private set; }

            public SingelDataHadder(UInt16 id, byte[] data)
            {
                this.Id = id;
                Bytes = new byte[4 + data.Length];
                Bytes[0] = MAGIC[0];
                Bytes[1] = (byte)Message.SingelPackage;
                Array.Copy(BitConverter.GetBytes(id), 0, Bytes, 2, 2);
                PackageLoad = new byte[data.Length];
                Array.Copy(data, PackageLoad, data.Length);
                Array.Copy(data, 0, Bytes, 4, data.Length);
            }

            public SingelDataHadder(byte[] b)
            {
                if (b.Length < 4)
                    throw new ArgumentException("Länge des Package Stimmt nicht");
                if (b[0] != MAGIC[0])
                    throw new ArgumentException("Falsches Magic Byte");
                if (b[1] != (byte)Message.SingelPackage)
                    throw new ArgumentException("Falscher Message Type");
                this.Bytes = b;
                this.Id = BitConverter.ToUInt16(b, 2);
                PackageLoad = new byte[b.Length - 4];
                Array.Copy(b, 4, PackageLoad, 0, PackageLoad.Length);
            }
        }

        private class ShortHeadder : MessageHeadder
        {
            public ShortHeadder(UInt16 id, Message message)
            {
                this.Id = id;
                Bytes = new byte[4];
                Bytes[0] = MAGIC[0];
                Bytes[1] = (byte)message;
                Array.Copy(BitConverter.GetBytes(id), 0, Bytes, 2, 2);
            }

            public ShortHeadder(byte[] b, Message message)
            {
                if (b.Length != 4)
                    throw new ArgumentException("Länge des Package Stimmt nicht");
                if (b[0] != MAGIC[0])
                    throw new ArgumentException("Falsches Magic Byte");
                if (b[1] != (byte)message)
                    throw new ArgumentException("Falscher Message Type");
                this.Bytes = b;
                this.Id = BitConverter.ToUInt16(b, 2);
            }
        }

        private class MessageHeadder : IMessageHeadder
        {
            public byte[] Bytes { get; protected set; }

            public UInt16 Id { get; protected set; }

            public Message Type { get { return (Message)Bytes[1]; } }
        }

        private interface IMessageHeadder
        {
            byte[] Bytes { get; }

            ushort Id { get; }

            Message Type { get; }
        }

        private interface ISender : IMessageHeadder { }

        private interface IReciver : IMessageHeadder { }

        private enum Message : byte
        {
            // +------------------------------------------------------------------------
            // |                         Starte Komunikation                           |
            // +--------------+------------------+------------------+------------------+
            // |  Magic       |    Message       |      ID          |   Package Count  |
            // +--------------+------------------+------------------+------------------+
            // |   1 Byte     |   1 byte = 0x1   |   2 byte uint16  |   2 byte uint16  |
            // |              |                  |                  |                  |
            // +--------------+------------------+------------------+------------------+
            StartComunication = 0x1,

            // +-----------------------------------------------------
            // |          Bestätige Start der Komunikation          |
            // +--------------+------------------+------------------+
            // |  Magic       |     Message      |      ID          |
            // +--------------+------------------+------------------+
            // |   1 Byte     |   1 byte = 0x2   |   2 byte uint16  |
            // |              |                  |                  |
            // +--------------+------------------+------------------+
            AcknolageStartComunication = 0x2,

            // +-------------------------------------------------------------------------------------------
            // |                                     Daten Packet                                         |
            // +--------------+------------------+------------------+------------------+------------------+
            // |  Magic       |     Message      |      ID          |      Index       |      Data        |
            // +--------------+------------------+------------------+------------------+------------------+
            // |   1 Byte     |   1 byte = 0x3   |   2 byte uint16  |   2 byte uint16  |     Variable     |
            // |              |                  |                  |                  |                  |
            // +--------------+------------------+------------------+------------------+------------------+
            Data = 0x3,

            // +------------------------------------------------------------------------
            // |                          Wiederhole Packet                            |
            // +--------------+------------------+------------------+------------------+
            // |  Magic       |      Message     |      ID          |      Index       |
            // +--------------+------------------+------------------+------------------+
            // |   1 Byte     |   1 byte = 0x4   |   2 byte uint16  |   2 byte uint16  |
            // |              |                  |                  |                  |
            // +--------------+------------------+------------------+------------------+
            ResendData = 0x4,

            // +-----------------------------------------------------
            // |           Erbitte Ende der Komunikation            |
            // +--------------+------------------+------------------+
            // |  Magic       |      Message     |      ID          |
            // +--------------+------------------+------------------+
            // |   1 Byte     |   1 byte = 0x5   |   2 byte uint16  |
            // |              |                  |                  |
            // +--------------+------------------+------------------+
            EndComunication = 0x5,

            // +-----------------------------------------------------
            // |          Bestätige Ende der Komunikation           |
            // +--------------+------------------+------------------+
            // |  Magic       |      Message     |      ID          |
            // +--------------+------------------+------------------+
            // |   1 Byte     |   1 byte = 0x6   |   2 byte uint16  |
            // |              |                  |                  |
            // +--------------+------------------+------------------+
            AcknolageEndComunication = 0x6,

            // +-----------------------------------------------------
            // |                     Fehler                         |
            // +--------------+------------------+------------------+
            // |  Magic       |      Message     |      ID          |
            // +--------------+------------------+------------------+
            // |   1 Byte     |   1 byte = 0x7   |   2 byte uint16  |
            // |              |                  |                  |
            // +--------------+------------------+------------------+
            Error = 0x7,

            // +------------------------------------------------------------------------
            // |                            Einzel Packet                              |
            // +--------------+------------------+------------------+------------------+
            // |  Magic       |     Message      |      ID          |      Data        |
            // +--------------+------------------+------------------+------------------+
            // |   1 Byte     |   1 byte = 0x8   |   2 byte uint16  |     Variable     |
            // |              |                  |                  |                  |
            // +--------------+------------------+------------------+------------------+
            SingelPackage = 0x8,
        }

        #endregion Headder
    }
}