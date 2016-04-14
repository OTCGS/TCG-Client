using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Network.Relay.Messages
{
    public class Relay : Message
    {
        public override byte[] RawData
        {
            get
            {
                var bMagic = MAGIC;
                var bType = new byte[] { (byte)this.Type };
                var bSourceId = BitConverter.GetBytes(SourceId);
                var bData = Data ?? new byte[0];
                return bMagic.Concat(bType).Concat(bSourceId).Concat(bData).ToArray();
            }
            set
            {
                CheckTypeAndMagic(value);
                this.SourceId = BitConverter.ToUInt32(value, 2);
                this.Data = value.Skip(6).ToArray();
            }
        }

        public UInt32 SourceId { get; set; }

        public byte[] Data { get; set; }

        public Relay()
            : base(MessageType.Relay)
        {
        }
    }
}