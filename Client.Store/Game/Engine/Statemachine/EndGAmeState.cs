using System;
using System.Threading.Tasks;
using Client.Store.Game.Data;

namespace Client.Store.Game.Engine.Statemachine
{
    internal class EndGAmeState : AbstracteState
    {
        private PlayerNumber player;

        public EndGAmeState(PlayerNumber player)
        {
            this.player = player;
        }

        public override Task<AbstracteState> Execute(GameConnection connection)
        {
            return Task.FromResult<AbstracteState>(null);
        }
    }
}