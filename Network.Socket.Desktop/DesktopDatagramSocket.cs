using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Network.Socket.Desktop
{
    internal class DesktopDatagramSocket : IMulticastSuportedDatagramSocket
    {
        private System.Net.Sockets.UdpClient udp;

        public object OriginalSocket
        {
            get
            {
                if (udp == null)
                    CreateClient();
                return udp;
            }
        }

        public async System.Threading.Tasks.Task Send(byte[] data, string remoteHost, uint port)
        {
            if (udp == null)
                throw new InvalidOperationException("Bind must be cald befor");
            await udp.SendAsync(data, data.Length, remoteHost, (int)port);
        }

        public System.Threading.Tasks.Task Bind(uint localPort = 0)
        {
            if (udp != null)
                throw new Exception("Already Bound");

            CreateClient(localPort);
            return System.Threading.Tasks.Task.FromResult(false);
        }

        private async void CreateClient(uint localPort = 0)
        {
            udp = new System.Net.Sockets.UdpClient((int)localPort);
            while (true)
            {
                var b = await udp.ReceiveAsync();
                if (this.MessageRecived != null)
                    this.MessageRecived(udp, new MessageRecivedArgs() { Port = (uint)b.RemoteEndPoint.Port, Host = b.RemoteEndPoint.Address.ToString(), Data = b.Buffer });
            }
        }

        public event MessageRecivedEvent MessageRecived;

        public void Dispose()
        {
            (udp as IDisposable).Dispose();
        }

        public System.Threading.Tasks.Task JounMulticastGroup(string ip)
        {
            var values = ip.Split('.');
            var bytes = values.Select(x => byte.Parse(x)).ToArray();
            if (bytes.Length != 4 && bytes.Length != 16)
                throw new ArgumentException("Wrong Number of Bytes.");
            udp.JoinMulticastGroup(new System.Net.IPAddress(bytes));
            return System.Threading.Tasks.Task.FromResult(false);
        }

        public UInt32 Port
        {
            get
            {
                var p = udp.Client.LocalEndPoint as System.Net.IPEndPoint;
                if (p == null)
                    return 0;
                return (UInt32)p.Port;
            }
        }
    }
}