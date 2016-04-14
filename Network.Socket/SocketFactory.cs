using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Socket
{
    public static partial class SocketFactory
    {

        public static IDatagramSocket CreateReliableDatagramsocket(IDatagramSocket socket = null, int maxPackageSize = 1024)
        {

            if (socket == null)
                return new ReliableDatagrammSocket(fac.PrivCreateDatagramsocket(), maxPackageSize);
            return new ReliableDatagrammSocket(socket, maxPackageSize);

        }

        public static IMulticastSuportedDatagramSocket CreateDatagramsocket()
        {

            return fac.PrivCreateDatagramsocket();
        }

        public static ITcpSocket CreateTcpsocket()
        {

            return fac.PrivCreateTcpsocket();
        }

        public static ITcpSocketListener CreateTcpListener()
        {

            return fac.PrivCreateTcpListener();
        }
    }
}