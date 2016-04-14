using Client.Store.Game.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Store.Game.Engine.EventArgs
{
    public class AddedCardsEventArgs(IEnumerable<AddedCardsEventArgs.CardsAdded> newCards) : System.EventArgs
    {

        public IEnumerable<CardsAdded> NewCards { get; } = newCards;

        public class CardsAdded
        {
            public MentalCardGame.Card Card { get; set; }
            public PlayerNumber Player { get; set; }
            public String Region { get; set; }
            public int Index { get; set; }
        }
    }
}
