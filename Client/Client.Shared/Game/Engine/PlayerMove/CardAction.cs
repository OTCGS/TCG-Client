using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Game.Engine.PlayerMove
{
    class CardAction : AbstractAction
    {

        public CardAction(MentalCardGame.Card card, String name, String description, Func<Task> perform) : base(name, description, perform)
        {
            Card = card;
        }

        public  MentalCardGame.Card Card { get; }

    }
}
