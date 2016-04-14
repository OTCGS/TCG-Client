using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Network.Relay.Messages
{
    public class Accept : Message
    {
        public override byte[] RawData
        {
            get
            {
                var bMagic = MAGIC;
                var bType = new byte[] { (byte)this.Type };
                var bID = BitConverter.GetBytes(Id);
                var bTimeout = BitConverter.GetBytes(Timeout);
                var bAddressFamily = new byte[] { (byte)this.AddressFamily };
                var bAdress = Address ?? new byte[0];
                var bPort = BitConverter.GetBytes(Port);
                return bMagic.Concat(bType).Concat(bID).Concat(bTimeout).Concat(bAddressFamily).Concat(bAdress).Concat(bPort).ToArray();
            }
            set
            {
                CheckTypeAndMagic(value);
                if (value.Length != 27 && value.Length != 15)
                    throw new ArgumentException("Length of the Data is wrong");

                this.Id = BitConverter.ToUInt32(value, 2);
                this.Timeout = BitConverter.ToUInt16(value, 6);
                this.AddressFamily = (AddressFamily)value[8];
                switch (AddressFamily)
                {
                    case AddressFamily.IPv4:
                        this.Address = value.Skip(9).Take(4).ToArray();
                        this.Port = BitConverter.ToUInt16(value, 13);
                        break;

                    case AddressFamily.IPv6:
                        this.Address = value.Skip(9).Take(16).ToArray();
                        this.Port = BitConverter.ToUInt16(value, 25);
                        break;

                    default:
                        throw new NotSupportedException("This AddressFamily is not Supportet");
                }
            }
        }

        public UInt32 Id { get; set; }

        /// <summary>
        /// Timeout in Sekunden nachdem der Server den Client aus seiner Verwaltung schmeißt
        /// </summary>
        public UInt16 Timeout { get; set; }

        public AddressFamily AddressFamily { get; set; }

        public byte[] Address { get; set; }

        public UInt16 Port { get; set; }

        public Accept()
            : base(MessageType.Accept)
        {
        }
    }
}