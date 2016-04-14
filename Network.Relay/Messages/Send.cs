using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Network.Relay.Messages
{
    public class Send : Message
    {
        public override byte[] RawData
        {
            get
            {
                var bMagic = MAGIC;
                var bType = new byte[] { (byte)this.Type };
                var btargetId = BitConverter.GetBytes(TargetId);
                var bData = Data ?? new byte[0];
                return bMagic.Concat(bType).Concat(btargetId).Concat(bData).ToArray();
            }
            set
            {
                CheckTypeAndMagic(value);
                this.TargetId = BitConverter.ToUInt32(value, 2);
                this.Data = value.Skip(6).ToArray();
            }
        }

        public UInt32 TargetId { get; set; }

        public byte[] Data { get; set; }

        public Send()
            : base(MessageType.Send)
        {
        }
    }
}