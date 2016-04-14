using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.RawConnections
{
    internal class RelayConnection : RawConnections.RawConnection
    {
        private Relay.Client.RelayClientSocket RelaySocket { get; set; }


        internal RelayConnection(Network.Relay.Client.RelayClientSocket socket, uint remoteId)
            : base(socket)
        {
            this.RelaySocket = socket;
            this.RelayId = socket.Port;
            this.RemoteId = remoteId;
        }

        public override Task Send(byte[] data)
        {

            return socket.Send(data, null, RemoteId);
        }

        private bool isConnected;

        public override bool IsConnected { get { return isConnected; } }

        public uint RelayId { get; private set; }

        public uint RemoteId { get; private set; }


    }
}