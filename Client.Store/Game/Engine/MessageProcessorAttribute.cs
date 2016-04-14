using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Store.Game.Engine
{
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal sealed class MessageProcessorAttribute : Attribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        private readonly Type messageType;

        // This is a positional argument
        public MessageProcessorAttribute(Type messageType)
        {
            this.messageType = messageType;
        }

        public Type TargetMessage
        {
            get { return messageType; }
        }
    }
}