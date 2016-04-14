using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Store.Game.Engine.EventArgs
{
    public class MoveCardEventArgs(Data.MoveCard movedCards) : System.EventArgs
    {
        public Data.MoveCard MovedCards { get; set; } = movedCards;
    }
}