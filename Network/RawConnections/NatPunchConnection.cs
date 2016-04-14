using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.RawConnections
{
    internal class NatPunchConnection : RawConnections.RawConnection
    {
        public NatPunchConnection()
            : base(null)
        {
        }

        public override bool IsConnected
        {
            get { throw new NotImplementedException(); }
        }

        public override Task Send(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}