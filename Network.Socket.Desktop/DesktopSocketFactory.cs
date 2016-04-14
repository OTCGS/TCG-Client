using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Network.Socket.Desktop
{
    internal class DesktopSocketFactory
    {

        public IMulticastSuportedDatagramSocket PrivCreateDatagramsocket()
        {
            return new DesktopDatagramSocket();
        }


        public ITcpSocket PrivCreateTcpsocket()
        {
            return new DesktopTcPSocket();
        }

        public ITcpSocketListener PrivCreateTcpListener()
        {
            return new DesktopTcpListener();
        }
    }
}