using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Store.Game.Engine.EventArgs
{
    public class PermutateCardsEventArgs(MentalCardGame.Stack newValue, MentalCardGame.Stack oldValue) : System.EventArgs
    {
        public MentalCardGame.Stack OldValue { get; } = oldValue;

        public MentalCardGame.Stack NewValue { get; } = newValue;
    }
}