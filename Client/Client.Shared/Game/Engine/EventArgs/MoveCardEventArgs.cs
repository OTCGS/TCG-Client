using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Game.Engine.EventArgs
{
    public class MoveCardEventArgs : System.EventArgs
    {

        public MoveCardEventArgs(Data.MoveCard movedCards)
        {
            MovedCards = movedCards;
        }

        public Data.MoveCard MovedCards { get; set; } 
    }
}