using Client.Store.Game.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Store.Game.Engine.Statemachine
{
    internal class EndOfTurnState : AbstracteState
    {
        public override async  Task<AbstracteState> Execute(GameConnection connection)
        {

            var erg =    await Task.Run(() => connection.Engin.ScriptEngin.Execute(connection.Engin.GameData.DeterminateWinner).GetValue("DeterminateWinner").Invoke());
            var player = (PlayerNumber)(int)erg.AsNumber();

            switch (player)
            {
                case PlayerNumber.None:
                return new StartOfTurnState();
                    
                case PlayerNumber.Player1:
                case PlayerNumber.Player2:
                case PlayerNumber.Any:
                    return new EndGAmeState(player);
                default:
                    throw new NotImplementedException();
                    
            }

        }
    }
}