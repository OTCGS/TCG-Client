using System;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Store.Game.Engine.Statemachine
{
    internal class ActivePlayer : AbstracteState
    {
        public async override Task<AbstracteState> Execute(GameConnection connection)
        {

            await Task.Run(() => connection.Engin.ScriptEngin.Execute(connection.Engin.GameData.StartOfTurn).GetValue("StartOfTurn").Invoke());

            while (connection.Engin.Me == connection.Engin.CurrentTurn)
            {
                // Führe JavaScript aus um die gewünchten Aktioinen zu bekommen.
                var m = (object[])await Task.Run(() => connection.Engin.ScriptEngin.Execute(connection.Engin.GameData.DeterminatePlayerActions).GetValue("GetPlayerActions").Invoke().ToObject());
                //Warte auf die Eingabe des Nutzers.
                var move = await connection.Engin.WaitForInput(m.Cast<PlayerMove.AbstractAction>());
                //Warte auf Den Abschluss der Aktion.
                await move.Perform();
            }
            return new EndOfTurnState();
        }
    }
}