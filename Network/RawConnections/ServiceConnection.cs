using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Network.RawConnections
{
    class ServiceConnection : RawConnection
    {
        private uint port;

        public ServiceConnection(ServerServer.DatagramSocketEmulator socket, Socket.IDatagramSocket reliableSocekt, User targetUser) : base(socket)
        {
            Init(socket, targetUser);
        }

        private async void Init(ServerServer.DatagramSocketEmulator socket, User targetUser)
        {
            port = await socket.GetPort(targetUser.PublicKey);
        }

        public override bool IsConnected => true;



        public override Task Send(byte[] data)
        {
            return socket.Send(data, "", port);
        }
    }
}
