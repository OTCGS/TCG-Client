using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Network.Relay.Messages
{
    public class KeepAlive : Message
    {
        public override byte[] RawData
        {
            get { return MAGIC.Concat(new byte[] { (byte)this.Type }).ToArray(); }
            set { CheckTypeAndMagic(value); }
        }

        public KeepAlive()
            : base(MessageType.Keepalive)
        {
        }
    }
}