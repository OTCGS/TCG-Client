using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Network.Relay.Messages
{
    public class Request : Message
    {
        public override byte[] RawData
        {
            get
            {
                var bMagic = MAGIC;
                var bType = new byte[] { (byte)this.Type };

                var bData = Data ?? new byte[0];
                return bMagic.Concat(bType).Concat(bData).ToArray();
            }
            set
            {
                CheckTypeAndMagic(value);
                this.Data = value.Skip(MAGIC.Length+1).ToArray();
            }
        }

        public byte[] Data { get; set; }

        public Request()
            : base(MessageType.Request)
        {
        }
    }
}