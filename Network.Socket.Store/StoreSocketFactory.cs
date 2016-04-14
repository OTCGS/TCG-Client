using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Socket.Store
{
    internal class StoreSocketFactory
    {

        public IMulticastSuportedDatagramSocket PrivCreateDatagramsocket()
        {
            return new StoreDatagramSocket(new Windows.Networking.Sockets.DatagramSocket());
        }


        public ITcpSocket PrivCreateTcpsocket()
        {
            return new StoreTcpSocket();
        }

        public ITcpSocketListener PrivCreateTcpListener()
        {
            return new StoreTcpListener();
        }
    }
}