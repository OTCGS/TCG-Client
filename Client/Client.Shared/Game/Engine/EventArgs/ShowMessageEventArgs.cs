using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Game.Engine.EventArgs
{
    public class ShowMessageEventArgs : System.EventArgs
    {
        public ShowMessageEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; } 

    }
}