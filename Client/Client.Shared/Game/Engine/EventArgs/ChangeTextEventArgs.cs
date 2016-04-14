using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Game.Engine.EventArgs
{
    public class ChangeTextEventArgs : System.EventArgs
    {
        public ChangeTextEventArgs(string text)
        {
            Text = text;
        }

        public string Text { get; } 

    }
}