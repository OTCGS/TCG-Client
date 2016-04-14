using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Socket.Desktop
{
    internal class DesktopTcPSocket : ITcpSocket
    {
        public DesktopTcPSocket()
        {
            socket = new System.Net.Sockets.TcpClient();
        }

        public object OriginalSocket
        {
            get { return socket; }
        }

        public ushort LocalPort
        {
            get { return (UInt16)(socket.Client.LocalEndPoint as System.Net.IPEndPoint).Port; }
        }

        public event MessageRecivedEvent MessageRecived;

        private System.Net.Sockets.TcpClient socket;
        private System.Net.Sockets.NetworkStream stream;

        public async Task Send(byte[] data)
        {
            if (stream == null)
                throw new InvalidOperationException("Not Conected");
            var lengthData = BitConverter.GetBytes(data.Length);
            await stream.WriteAsync(lengthData, 0, lengthData.Length);
            await stream.WriteAsync(data, 0, data.Length);
        }

        public async Task Connect(string remoteHost, int port)
        {
            await socket.ConnectAsync(remoteHost, port);
            stream = socket.GetStream();
            ReciveLoop(remoteHost, port);
        }

        internal void Connect(System.Net.Sockets.TcpClient client)
        {
            socket = client;

            stream = socket.GetStream();
            var endpoint = client.Client.RemoteEndPoint as System.Net.IPEndPoint;
            ReciveLoop(endpoint.Address.ToString(), endpoint.Port);
        }

        private System.Threading.CancellationTokenSource cancel;

        private void ReciveLoop(string remoteHost, int port)
        {
            cancel = new System.Threading.CancellationTokenSource();
            var cancelToken = cancel.Token;
            Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    var lengthData = new byte[4];
                    int lengthReaded = 0;
                    while (lengthReaded < 4)
                        lengthReaded += await stream.ReadAsync(lengthData, lengthReaded, lengthData.Length - lengthReaded, cancelToken);
                    Logger.Assert(lengthReaded == 4, $"gelesene Länge war ungleich 4 (war: {lengthReaded})");
                    var toRead = BitConverter.ToInt32(lengthData, 0);
                    lengthReaded = 0;
                    var messageData = new byte[toRead];
                    while (lengthReaded < toRead)
                        lengthReaded += await stream.ReadAsync(messageData, lengthReaded, toRead - lengthReaded, cancelToken);
                    if (MessageRecived != null)
                        this.MessageRecived(this, new MessageRecivedArgs() { Data = messageData, Host = remoteHost, Port = (uint)port });
                }
            });
        }

        public void Close()
        {
            cancel.Cancel();
            socket.Close();
        }

        public ushort RemotePort
        {
            get { return (UInt16)(socket.Client.RemoteEndPoint as System.Net.IPEndPoint).Port; }
        }

        public string RemoteHost
        {
            get { return (socket.Client.RemoteEndPoint as System.Net.IPEndPoint).Address.ToString(); }
        }
    }

    internal class DesktopTcpListener : ITcpSocketListener
    {
        public DesktopTcpListener()
        {
        }

        public object OriginalSocket
        {
            get { return listener; }
        }

        public ushort Port
        {
            get { return (UInt16)(listener.LocalEndpoint as System.Net.IPEndPoint).Port; }
        }

        public async Task Bind(int localPort = 0)
        {
            listener = new System.Net.Sockets.TcpListener(localPort);
            listener.Start();
            StartAcceptLoop();
        }

        private void StartAcceptLoop()
        {
            cancel = new System.Threading.CancellationTokenSource();
            var cancelToken = cancel.Token;
            Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    var socket = new DesktopTcPSocket();
                    socket.Connect(client);
                    if (SocketRecived != null)
                        SocketRecived(this, new TcPSocetRecivedArgs() { Socket = socket });
                }
            });
        }

        public event TcPSocetRecivedEvent SocketRecived;

        private System.Net.Sockets.TcpListener listener;
        private System.Threading.CancellationTokenSource cancel;
    }
}