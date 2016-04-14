using MentalCardGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Game.Data;

namespace Client.Game.Engine.Statemachine
{
    internal class InnitDataExchangeState : AbstracteState
    {
        private readonly IEnumerable<CardInstance> deck;

        public InnitDataExchangeState(IEnumerable<CardInstance> deck)
        {
            this.deck = deck;
        }

        public async override Task<AbstracteState> Execute(GameConnectivity connection)
        {
            var d = new Data.InitData();
            var ownKey = MentalCardGame.CardEngine.CreateGameKey(connection.Engin.Me == Data.PlayerNumber.Player1 ? 0 : 1, new Random());
            d.GameKey = CardEngine.SerializeKey(ownKey).ToString();

            foreach (var item in deck)
                d.Deck.Add(item);

            await connection.SendMessage(d);
            var answer = await connection.Recive<Data.InitData>();
            var otherKey = MentalCardGame.CardEngine.DeSerializeKey(System.Xml.Linq.XElement.Parse(answer.GameKey));

            IEnumerable<Data.CardInstance> player1Deck = connection.Engin.Me == Data.PlayerNumber.Player1 ? d.Deck : answer.Deck;
            IEnumerable<Data.CardInstance> player2Deck = connection.Engin.Me == Data.PlayerNumber.Player2 ? d.Deck : answer.Deck;

            var player1Cards = await Task.WhenAll(player1Deck.Select(async x => await GetCardData(new UuidServer() { Uuid = x.CardDataId, Server = x.Creator })));
            var player2Cards = await Task.WhenAll(player2Deck.Select(async x => await GetCardData(new UuidServer() { Uuid = x.CardDataId, Server = x.Creator })));


            connection.Engin.CardLookup = new Data.CardLookup(player1Cards.Concat(player2Cards).Distinct());

            CardEngine cardEngine = new MentalCardGame.CardEngine(ownKey, App.RNG, connection.Engin.CardLookup.Count, otherKey, ownKey);

            await connection.Engin.InitCardEngein(cardEngine);



            return new BeforeGameState(player1Deck, player2Deck);
        }

        private Task<CardData> GetCardData(UuidServer data)
        {
            return DDR.GetCardData(data);
        }
    }
}