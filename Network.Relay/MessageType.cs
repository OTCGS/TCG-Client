using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Network.Relay
{
    public enum MessageType : byte
    {
        Keepalive = 0,
        Request = 1,
        Accept = 2,
        Send = 3,
        Relay = 4
    }
}