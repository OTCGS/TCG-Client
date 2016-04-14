using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Game.Engine.EventArgs
{
    public class CardChagnedEventArgs : System.EventArgs
    {
        public CardChagnedEventArgs(MentalCardGame.Card newValue, MentalCardGame.Card oldValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
        public MentalCardGame.Card OldValue { get; }

        public MentalCardGame.Card NewValue { get; }
    }
}