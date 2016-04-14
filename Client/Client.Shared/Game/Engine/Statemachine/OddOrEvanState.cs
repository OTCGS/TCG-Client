using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Game.Engine.Statemachine
{
    internal class OddOrEvanState : AbstracteState
    {

        public async override Task<AbstracteState> Execute(GameConnectivity connection)
        {


            var peek = connection.Peek();
            while (true)
            {
                var timer = Task.Delay(500);

                var task = await Task.WhenAny(timer, peek);
                if (task == peek)
                {
                    await connection.SendMessage(Data.Confirmation.Ok);
                    break;
                }
                else
                    await connection.SendMessage(Data.Confirmation.Error);
            }

            while ((await connection.Recive<Data.Confirmation>()) != Data.Confirmation.Ok) ; // Queu leeren. Das letzte Confirmation ist OK.


            var randomNumber = App.RNG.Next();
            await connection.SendMessage(new Data.RandomNumber() { Value = randomNumber });
            //var tmp= await connection.Recive<Data.WhoIsOdd>();
            var recive = await connection.Recive<Data.RandomNumber>();




            Data.WhoIsOdd message;
            if (randomNumber < recive.Value)
                message = Data.WhoIsOdd.MeOdd;  // Also versuche ich dran zu kommen.
            else                                // Der Andere hat sich gemeldet bevor ich mich
                message = Data.WhoIsOdd.YouOdd; // gerührt habe, also lassen wir ihm seinen Willen

            await connection.SendMessage(message);
            var reciveOddOrEvan = await connection.Recive<Data.WhoIsOdd>();

            var answer = reciveOddOrEvan;

            if (answer != message) // Die Messages müssen ungleich sein, sonst sind wir uns nicht einig.
            {
                // Wir sind uns einig
                Logger.Information(message.ToString());
                return new DetermBeginnerState(message);
            }
            else
            {
                // Neue Runde neues Glück
                return this;
            }
        }
    }
}