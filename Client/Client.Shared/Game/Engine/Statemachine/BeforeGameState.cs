using MentalCardGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Client.Game.Engine.Statemachine
{
    internal class BeforeGameState : AbstracteState
    {

        private IEnumerable<Data.CardInstance> player1Deck;
        private IEnumerable<Data.CardInstance> player2Deck;

        public BeforeGameState(IEnumerable<Data.CardInstance> player1Deck, IEnumerable<Data.CardInstance> player2Deck)
        {
            this.player1Deck = player1Deck;
            this.player2Deck = player2Deck;
        }

        public async override Task<AbstracteState> Execute(GameConnectivity connection)
        {
            var script = connection.Engin.GameData.ScriptEngin;
            
            // Player 1 Verteilt die Karten der Andere Antwortet nur darauf.
            // Dies geschieht in Init.
            if (connection.Engin.Me == Data.PlayerNumber.Player1)
            {
                await Task.Run(() =>
                {
                    connection.Engin.InvokeGameRuleMethod("Init",
                        Jint.Native.JsValue.FromObject(script, player1Deck.Select(x => connection.Engin.CardLookup[x.CardDataId, x.Creator]).ToArray()),
                        Jint.Native.JsValue.FromObject(script, player2Deck.Select(x => connection.Engin.CardLookup[x.CardDataId, x.Creator]).ToArray()));
                });

            }

            return new StartOfTurnState();
        }

    }
}