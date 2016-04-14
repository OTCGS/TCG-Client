using System;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Game.Engine.Statemachine
{
    internal class ActivePlayerState : AbstracteState
    {
        public async override Task<AbstracteState> Execute(GameConnectivity connection)
        {

            await Task.Run(() => connection.Engin.InvokeGameRuleMethod( "StartOfTurn"));

            while (connection.Engin.Me == connection.Engin.CurrentTurn)
            {
                // Führe JavaScript aus um die gewünchten Aktioinen zu bekommen.
                var m = (object[])await Task.Run(() => connection.Engin.InvokeGameRuleMethod("GetPlayerActions").ToObject());
                //Warte auf die Eingabe des Nutzers.
                var move = await connection.Engin.WaitForInput(m.Cast<PlayerMove.AbstractAction>());
                //Warte auf Den Abschluss der Aktion.
                await move.Perform();
            }

            return new EndOfTurnState();
        }
    }
}