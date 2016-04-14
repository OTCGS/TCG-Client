using Network.Socket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Relay.Client
{
    public class RelayClientSocket : Socket.IDatagramSocket
    {
        private Socket.IDatagramSocket socket;
        private readonly uint remotePort;
        private readonly string remoteAddress;

        private System.Threading.Tasks.TaskCompletionSource<Messages.Accept> acceptWaiter = new TaskCompletionSource<Messages.Accept>();
        private uint id;
        private ushort timeout;
        private bool connected;

        public event MessageRecivedEvent MessageRecived;

        private bool sendingKeepAlive;
        private readonly byte[] clientData;

        public object OriginalSocket
        {
            get
            {
                return socket;
            }
        }

        public uint Port
        {
            get
            {
                return this.id;
            }
        }

        public static RelayClientSocket Create(byte[] clientData, Network.Socket.IDatagramSocket socket, string server, uint srverPort = Relay.Protocol.PORT)
        {
            var erg = new RelayClientSocket(clientData, server, srverPort);
            erg.socket = Socket.SocketFactory.CreateReliableDatagramsocket(socket);
            erg.socket.MessageRecived += erg.socket_MessageRecived;
            return erg;
        }
        private RelayClientSocket(byte[] clientData, string server, uint srverPort = Relay.Protocol.PORT)
        {
            this.clientData = clientData;
            this.remotePort = srverPort;
            this.remoteAddress = server;

        }

        private void socket_MessageRecived(object sender, Socket.MessageRecivedArgs args)
        {
            // Probleme Mit Hostadress, verschieden strings können den gleichen Server zugeordnet werden.
            //if (args.Host != remoteAddress || args.Port != remotePort)
            //    return;

            var message = Messages.Message.CreateMessageFromData(args.Data);
            switch (message.Type)
            {
                case MessageType.Keepalive:
                    break;

                case MessageType.Request:
                    break;

                case MessageType.Accept:
                    {
                        var response = message as Messages.Accept;
                        this.acceptWaiter.SetResult(response);
                    }
                    break;

                case MessageType.Send:
                    break;

                case MessageType.Relay:
                    {
                        var response = message as Messages.Relay;

                        DataRecived(response.SourceId, response.Data);
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private void DataRecived(uint sourceId, byte[] data)
        {
            if (this.MessageRecived != null)
                MessageRecived(this, new MessageRecivedArgs() { Data = data, Host = null, Port = sourceId });
        }

        private async Task<Messages.Accept> GetAccept()
        {
            var response = await acceptWaiter.Task;
            acceptWaiter = new TaskCompletionSource<Messages.Accept>();
            return response;
        }

        private async Task<UInt32> RequestId()
        {
            var request = new Messages.Request() { Data = this.clientData };
            await Send(request);
            var accept = await GetAccept();

            if (id != 0)
                return id;

            this.id = accept.Id;
            this.timeout = accept.Timeout;
            this.sendingKeepAlive = true;
            var temp = Task.Run(async () => // Variablenzuweisung verhindert das Warning. :(
            {
                while (this.sendingKeepAlive)
                {
                    await Task.Delay(timeout * 1000);
                    await SendKeepAlive();
                }
            });
            this.connected = true;
            return this.id;
        }

        private void Disconnect()
        {
            this.sendingKeepAlive = false;
        }

        private async Task Send(Messages.Message request)
        {
            await socket.Send(request.RawData, remoteAddress, remotePort);
        }

        private async Task SendKeepAlive()
        {
            await Send(new Messages.KeepAlive());
        }

        private async Task SendData(UInt32 id, byte[] data)
        {
            if (!this.connected)
                throw new ArgumentException("Not Conected");
            await Send(new Messages.Send() { TargetId = id, Data = data });
        }

        public Task Send(byte[] data, string remoteHost, uint port)
        {
            if (remoteHost != null)
                throw new ArgumentException("Remotehost must be null. different Hosts not Suported.");
            return this.SendData(port, data);
        }

        public async Task Bind(uint localPort = 0)
        {
            if (localPort != 0)
                throw new ArgumentException("Port must be 0. Choose of port not suported.");


            await this.RequestId();

        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}