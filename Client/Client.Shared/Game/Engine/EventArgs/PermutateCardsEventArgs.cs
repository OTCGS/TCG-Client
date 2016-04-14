using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Game.Engine.EventArgs
{
    public class PermutateCardsEventArgs : System.EventArgs
    {
        public PermutateCardsEventArgs(MentalCardGame.Stack newValue, MentalCardGame.Stack oldValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public MentalCardGame.Stack OldValue { get; } 

        public MentalCardGame.Stack NewValue { get; } 
    }
}