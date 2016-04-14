using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Store.Game.Engine.Statemachine
{
    internal class StartOfTurnState : AbstracteState
    {
        public override Task<AbstracteState> Execute(GameConnection connection)
        {
            if (connection.Engin.CurrentTurn == connection.Engin.Me)
                return Task.FromResult<AbstracteState>(new ActivePlayer());
            else
                return Task.FromResult<AbstracteState>(new PassivePlayerState());
        }
    }
}