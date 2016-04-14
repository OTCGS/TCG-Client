using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Store.Game.Engine.Statemachine
{
    internal class StatemachineCore
    {
        public AbstracteState CurrentState { get; private set; }

        public async Task Start(GameConnection connection)
        {
            CurrentState = new OddOrEvanState();
            while (CurrentState != null)
                CurrentState = await CurrentState.Execute(connection);
        }
    }

    internal abstract class AbstracteState
    {
        public abstract Task<AbstracteState> Execute(GameConnection connection);
    }
}