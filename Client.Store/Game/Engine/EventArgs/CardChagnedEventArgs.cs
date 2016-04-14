using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Store.Game.Engine.EventArgs
{
    public class CardChagnedEventArgs(MentalCardGame.Card newValue, MentalCardGame.Card oldValue) : System.EventArgs
    {
        public MentalCardGame.Card OldValue { get; } = oldValue;

        public MentalCardGame.Card NewValue { get; } = newValue;
    }
}