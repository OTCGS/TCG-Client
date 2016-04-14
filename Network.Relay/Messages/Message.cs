using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Network.Relay.Messages
{
    public abstract class Message
    {
        internal static readonly byte[] MAGIC = new byte[] { 0xA0 };

        public abstract byte[] RawData { get; set; }

        public MessageType Type { get; private set; }

        protected Message(MessageType type)
        {
            this.Type = type;
        }

        internal void CheckTypeAndMagic(byte[] value)
        {
            if (!MAGIC.SequenceEqual(value.Take(MAGIC.Length)))
                throw new ArgumentException("Using Wrong Magic");
            if (((MessageType)value[1]) != this.Type)
                throw new ArgumentException("Wrong MessageType");
        }

        public static bool IsMessage(byte[] data)
        {
            return MAGIC.SequenceEqual(data.Take(MAGIC.Length));
        }

        public static Message CreateMessageFromData(byte[] data)
        {
            var type = (MessageType)data[1];
            switch (type)
            {
                case MessageType.Keepalive:
                    return new KeepAlive() { RawData = data };

                case MessageType.Request:
                    return new Request() { RawData = data };

                case MessageType.Accept:
                    return new Accept() { RawData = data };

                case MessageType.Send:
                    return new Send() { RawData = data };

                case MessageType.Relay:
                    return new Relay() { RawData = data };

                default:
                    throw new NotSupportedException("This Type is not Suportet");
            }
        }
    }
}