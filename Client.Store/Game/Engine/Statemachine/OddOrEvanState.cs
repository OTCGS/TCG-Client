using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Store.Game.Engine.Statemachine
{
    internal class OddOrEvanState() : AbstracteState
    {
        private const int MAX_TIME_TO_WAIT = 5000;

        private int Waittime { get; set; } = 500;

        public async override Task<AbstracteState> Execute(GameConnection connection)
        {
            int timeToWait = (int)(App.RNG.Next() % Waittime);
            var task = connection.Recive();
            var t1 = Task.Delay(timeToWait);

            // Wir Warten eine Zufällige Zeit, und schauen ob sich der Andere zuerst meldet
            var winner = await Task.WhenAny(t1, task);

            Data.WhoIsOdd message;
            if (winner == t1)                   // Die Zeit ist abgelaufen, ohne das der Andere sich gemeldet hat.
                message = Data.WhoIsOdd.MeOdd;  // Also versuche ich dran zu kommen.
            else                                // Der Andere hat sich gemeldet bevor ich mich
                message = Data.WhoIsOdd.YouOdd; // gerührt habe, also lassen wir ihm seinen Willen

            await connection.SendMessage(message);
            var answer = (Data.WhoIsOdd)await task;

            if (answer != message) // Die Messages müssen ungleich sein, sonst sind wir uns nicht einig.
            {
                // Wir sind uns einig
                System.Diagnostics.Debug.WriteLine(message);
                return new DetermBeginnerState(message);
            }
            else
            {
                // Neue Runde neues Glück
                Waittime = Math.Min(Waittime * 2, MAX_TIME_TO_WAIT);
                return this;
            }
        }
    }
}