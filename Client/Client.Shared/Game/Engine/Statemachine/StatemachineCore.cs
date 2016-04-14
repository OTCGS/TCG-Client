using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Client.Game.Engine.Statemachine
{
    internal class StatemachineCore
    {

        public AbstracteState CurrentState { get; private set; }

        public async Task<Client.Game.Data.PlayerNumber> Start(GameConnectivity connection)
        {
            CurrentState = new OddOrEvanState();
            while (!(CurrentState is EndGAmeState))
                CurrentState = await CurrentState.Execute(connection);
            var endgame = CurrentState as EndGAmeState;
            return endgame.Player;
        }


    }

    internal abstract class AbstracteState
    {
        public abstract Task<AbstracteState> Execute(GameConnectivity connection);
    }


}