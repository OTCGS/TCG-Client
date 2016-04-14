using Network.Socket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.RawConnections
{
    internal class DirectConnection : RawConnections.RawConnection
    {
        public DirectConnection(Socket.IDatagramSocket socket)
            : base(socket)
        {
        }

        public override bool IsConnected { get { return isConnected; } }

        private bool isConnected;
        private uint targetPort;
        private string targetHost;

        public override Task Send(byte[] data)
        {
            return socket.Send(data, targetHost, targetPort);
        }


        internal void SetOtherPort(string host, uint otherPort)
        {
            this.targetPort = otherPort;
            this.targetHost = host;
            this.isConnected = true;
        }
    }
}