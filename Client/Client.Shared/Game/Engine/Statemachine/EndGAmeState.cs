using System;
using System.Threading.Tasks;
using Client.Game.Data;

namespace Client.Game.Engine.Statemachine
{
    internal class EndGAmeState : AbstracteState
    {
        public PlayerNumber Player {            get;}

        public EndGAmeState(PlayerNumber player)
        {
            this.Player = player;
        }

        public override Task<AbstracteState> Execute(GameConnectivity connection)
        {
            return Task.FromResult<AbstracteState>(null);
        }
    }
}