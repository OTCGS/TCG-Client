using MentalCardGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Store.Game.Engine.Statemachine
{
    internal class BeforeGameState : AbstracteState
    {
        private IEnumerable<Data.Card> player1Deck;
        private IEnumerable<Data.Card> player2Deck;

        public BeforeGameState(IEnumerable<Data.Card> player1Deck, IEnumerable<Data.Card> player2Deck)
        {
            this.player1Deck = player1Deck;
            this.player2Deck = player2Deck;
        }

        public async override Task<AbstracteState> Execute(GameConnection connection)
        {
            var script = connection.Engin.ScriptEngin;

            // Beide müssen ihre Standardfunktionen initialisieren.
            await Task.Run(() => script.Execute(connection.Engin.GameData.StandardFunctions));

            // Player 1 Verteilt die Karten der Andere Antwortet nur darauf.
            if (connection.Engin.Me == Data.PlayerNumber.Player1)
            {
                await Task.Run(() => script.Execute(connection.Engin.GameData.InitGame).GetValue("Init").Invoke(Jint.Native.JsValue.FromObject(script, connection.Engin.GameData.StandardCards.Select(x => connection.Engin.CardLookup[x]).ToArray()), Jint.Native.JsValue.FromObject(script, player1Deck.Select(x =>  connection.Engin.CardLookup[x.Id]).ToArray()), Jint.Native.JsValue.FromObject(script, player2Deck.Select(x => connection.Engin.CardLookup[x.Id]).ToArray())));
                script.ToString();

            }

            return new StartOfTurnState();
        }
    }
}