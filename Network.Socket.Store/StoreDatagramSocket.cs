using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace Network.Socket.Store
{
    public class StoreDatagramSocket : IMulticastSuportedDatagramSocket
    {
        public StoreDatagramSocket(Windows.Networking.Sockets.DatagramSocket originalSocket)
        {
            this.originalSocket = originalSocket;
            originalSocket.MessageReceived += originalSocket_MessageReceived;
        }

        private async void originalSocket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender, Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
        {
            if (this.MessageRecived == null)
                return;

            var bytelist = new List<byte>();
            using (var stream = args.GetDataStream().AsStreamForRead())
            {
                int read;
                do
                {
                    var b = new byte[256];
                    read = await stream.ReadAsync(b, 0, b.Length);
                    bytelist.AddRange(b.Take(read));
                } while (read != 0);
            }
            var bytes = bytelist.ToArray();

            this.MessageRecived(sender, new Socket.MessageRecivedArgs() { Data = bytes, Host = args.RemoteAddress.ToString(), Port = uint.Parse(args.RemotePort) });
        }

        public object OriginalSocket
        {
            get { return originalSocket; }
        }

        public async Task Send(byte[] data, string remoteHost, uint port)
        {
            try
            {
                if (remoteHost.StartsWith("https://"))
                    remoteHost = remoteHost.Substring("https://".Length);
                else if (remoteHost.StartsWith("http://"))
                    remoteHost = remoteHost.Substring("http://".Length);


                var hostname = new Windows.Networking.HostName(remoteHost);
                using (var stream = await this.originalSocket.GetOutputStreamAsync(hostname, port.ToString()))
                    await stream.WriteAsync(data.AsBuffer());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task Bind(uint localPort)
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            await originalSocket.BindServiceNameAsync(localPort.ToString(), icp.NetworkAdapter);

        }

        private string CurrentIPAddress()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp != null && icp.NetworkAdapter != null)
            {
                var hostname =
                    NetworkInformation.GetHostNames()
                        .SingleOrDefault(
                            hn =>
                            hn.IPInformation != null && hn.IPInformation.NetworkAdapter != null
                            && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                            == icp.NetworkAdapter.NetworkAdapterId);

                if (hostname != null)
                {
                    // the ip address
                    return hostname.CanonicalName;
                }
            }

            return string.Empty;
        }

        public event Socket.MessageRecivedEvent MessageRecived;

        private Windows.Networking.Sockets.DatagramSocket originalSocket;

        public void Dispose()
        {
            originalSocket.Dispose();
        }

        public Task JounMulticastGroup(string ip)
        {
            originalSocket.JoinMulticastGroup(new Windows.Networking.HostName(ip));
            return Task.FromResult(false);
        }

        public string Host { get { return originalSocket.Information.LocalAddress.RawName; } }

        public UInt32 Port
        {
            get
            {
                if (originalSocket.Information.LocalPort == null)
                    return 0;
                return UInt32.Parse(originalSocket.Information.LocalPort);
            }
        }
    }
}