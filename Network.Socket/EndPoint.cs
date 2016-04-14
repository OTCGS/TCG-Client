using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Socket
{
    public struct EndPoint
    {
        private byte[] address;

        public byte[] Address
        {
            get { return address; }
            set { address = value; }
        }

        private UInt16 port;

        public UInt16 Port
        {
            get { return port; }
            set { port = value; }
        }
    }
}