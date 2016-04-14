using Client.Game.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Game.Engine.EventArgs
{
    public class AddedCardsEventArgs : System.EventArgs
    {

        public AddedCardsEventArgs(IEnumerable<AddedCardsEventArgs.CardsAdded> newCards)
        {
            NewCards = newCards;
        }

        public IEnumerable<CardsAdded> NewCards { get; }

        public class CardsAdded
        {
            public MentalCardGame.Card Card { get; set; }
            public PlayerNumber Player { get; set; }
            public String Region { get; set; }
            public int Index { get; set; }
        }
    }
}
