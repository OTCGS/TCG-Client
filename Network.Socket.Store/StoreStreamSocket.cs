using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Network.Socket.Store
{
    internal class StoreTcpSocket : ITcpSocket
    {
        public StoreTcpSocket()
        {
            socket = new Windows.Networking.Sockets.StreamSocket();
        }

        public object OriginalSocket
        {
            get { return socket; }
        }

        public ushort LocalPort
        {
            get { return UInt16.Parse(socket.Information.LocalPort); }
        }

        public ushort RemotePort
        {
            get { return UInt16.Parse(socket.Information.RemotePort); }
        }

        public string RemoteHost
        {
            get { return socket.Information.RemoteHostName.RawName; }
        }

        public async Task Send(byte[] data)
        {
            var lengthData = BitConverter.GetBytes(data.Length);
            await socket.OutputStream.WriteAsync(lengthData.AsBuffer());
            if (data.Length != 0)
                await socket.OutputStream.WriteAsync(data.AsBuffer());
        }

        public async Task Connect(string remoteHost, int port)
        {
            await socket.ConnectAsync(new Windows.Networking.HostName(remoteHost), port.ToString());
            ReciveLoop(remoteHost, port);
        }

        internal void Connect(Windows.Networking.Sockets.StreamSocket socket)
        {
            this.socket = socket;
            ReciveLoop(socket.Information.RemoteHostName.RawName, UInt16.Parse(socket.Information.RemotePort));
        }

        private void ReciveLoop(string remoteHost, int port)
        {
            var stream = socket.InputStream;
            cancel = new System.Threading.CancellationTokenSource();
            var cancelToken = cancel.Token;
            Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    var lengthData = new byte[4].AsBuffer();
                    int lengthReaded = 0;
                    while (lengthReaded < 4) // weiterlesen bis die 4 Bytes des int gelesen sind
                    {
                        var readed = await stream.ReadAsync(lengthData, (uint)(lengthData.Length - lengthReaded), Windows.Storage.Streams.InputStreamOptions.None).AsTask(cancel.Token);
                        lengthReaded += (int)readed.Length;
                        lengthData = readed;

                    }

                    Logger.Assert(lengthReaded == 4, "Es sollten 4 bytes gelesen werden, waren aber " + lengthReaded);
                    var toRead = BitConverter.ToInt32(lengthData.ToArray(), 0);
                    lengthReaded = 0;
                    var messageData = new byte[toRead].AsBuffer();
                    if (toRead != 0)
                    {
                        await stream.ReadAsync(messageData, (uint)(toRead - lengthReaded), Windows.Storage.Streams.InputStreamOptions.None).AsTask(cancel.Token);
                        if (MessageRecived != null)
                            this.MessageRecived(this, new MessageRecivedArgs() { Data = messageData.ToArray(), Host = remoteHost, Port = (uint)port });
                    }
                    else
                    {
                        if (MessageRecived != null)
                            this.MessageRecived(this, new MessageRecivedArgs() { Data = new byte[0], Host = remoteHost, Port = (uint)port });
                    }
                }
            });
        }

        public void Close()
        {
            cancel.Cancel();
            socket.Dispose();
        }

        public event MessageRecivedEvent MessageRecived;

        private Windows.Networking.Sockets.StreamSocket socket;
        private System.Threading.CancellationTokenSource cancel;
    }

    internal class StoreTcpListener : ITcpSocketListener
    {
        public StoreTcpListener()
        {
            listener = new Windows.Networking.Sockets.StreamSocketListener();
        }

        public object OriginalSocket
        {
            get { return listener; }
        }

        public ushort Port
        {
            get { return UInt16.Parse(listener.Information.LocalPort); }
        }

        public async Task Bind(int localPort = 0)
        {
            listener.ConnectionReceived += (sender, e) =>
            {
                var s = new StoreTcpSocket();
                s.Connect(e.Socket);
                if (this.SocketRecived != null)
                    this.SocketRecived(this, new TcPSocetRecivedArgs() { Socket = s });
            };
            await listener.BindServiceNameAsync(localPort.ToString());
        }

        public event TcPSocetRecivedEvent SocketRecived;

        private Windows.Networking.Sockets.StreamSocketListener listener;
    }
}