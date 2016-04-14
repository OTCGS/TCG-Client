using MentalCardGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Store.Game.Data;

namespace Client.Store.Game.Engine.Statemachine
{
    internal class InnitDataExchangeState : AbstracteState
    {
        public async override Task<AbstracteState> Execute(GameConnection connection)
        {
            var d = new Data.InitData();
            var ownKey = MentalCardGame.CardEngine.CreateGameKey(connection.Engin.Me == Data.PlayerNumber.Player1 ? 0 : 1, new Random());
            d.GameKey = CardEngine.SerializeKey(ownKey).ToString();

            if (connection.Engin.GameData.UsingOwnCards)
            {

                // TODO: Nicht einfach Random Dek erstellen.
                foreach (var item in Enumerable.Range(0, 60).Select(x => new Data.Card() { Id = new Data.CardDataId() { Edition = 0, Number = x, Revision = 0 }, Owner = connection.Engin.Me }))
                {
                    d.Deck.Add(item);
                }
            }
            await connection.SendMessage(d);
            var answer = await connection.Recive<Data.InitData>();
            var otherKey = MentalCardGame.CardEngine.DeSerializeKey(System.Xml.Linq.XElement.Parse(answer.GameKey));

            IEnumerable<Data.Card> player1Deck = connection.Engin.Me == Data.PlayerNumber.Player1 ? d.Deck : answer.Deck;
            IEnumerable<Data.Card> player2Deck = connection.Engin.Me == Data.PlayerNumber.Player2 ? d.Deck : answer.Deck;

            var player1Cards = await Task.WhenAll(player1Deck.Select(async x => await GetCardData(x.Id)));
            var player2Cards = await Task.WhenAll(player2Deck.Select(async x => await GetCardData(x.Id)));


            connection.Engin.CardLookup = new Data.CardLookup(connection.Engin.GameData.StandardCards.Concat(player1Cards).Concat(player2Cards).Distinct());

            CardEngine cardEngine = new MentalCardGame.CardEngine(ownKey, App.RNG, connection.Engin.CardLookup.Count, otherKey, ownKey);

            connection.Engin.Init(cardEngine);



            return new BeforeGameState(player1Deck, player2Deck);
        }

        private Task<CardData> GetCardData(CardDataId id)
        {
            return CardDataStorage.GetCardData(id);
        }
    }
}