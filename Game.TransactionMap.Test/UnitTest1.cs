using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Security;
using System.Linq;
using System.Threading.Tasks;

namespace Game.TransactionMap.Test
{
    [TestClass]
    public class UnitTest1
    {
        private readonly Random r = new Random(0);



        [TestCleanup]
        public void CleanUp()
        {
            var g = new PrivateType(typeof(Graph));
            g.InvokeStatic("Cleanup");
        }

        [TestInitialize]
        public void init()
        {
            var g = new PrivateType(typeof(Graph));
            g.InvokeStatic("Init");

        }


        [TestMethod]
        public void CheckDifferentKeys()
        {
            var k = GenerateKeys(2);
            Assert.AreNotEqual(k[0].FingerPrint(), k[1].FingerPrint(), true, "Die Fingerprints der erzeugten Schlüssel sind identisch.");
        }

        [TestMethod]
        public async Task OwnerTest()
        {
            var server = GenerateKeys(1).First();
            var alice = GenerateKeys(1).First();

            var cardData = GenerateCardData(1, server);
            var card = GenerateCards(1, cardData).First();

            Assert.IsFalse(await Graph.CheckOwner(alice, card));

            await Graph.Trade(server, alice, new Client.Game.Data.CardInstance[] { card }, new Client.Game.Data.CardInstance[] { }, b => server.Sign(b), b => alice.Sign(b));

            Assert.IsTrue(await Graph.CheckOwner(alice, card));
        }

        [TestMethod]
        public async Task GetCardsOwnedByTest()
        {
            var server = GenerateKeys(1).First();
            var alice = GenerateKeys(1).First();
            var bob = GenerateKeys(1).First();

            var cardData = GenerateCardData(3, server);
            var card = GenerateCards(20, cardData);

            var cardsForAlice = card.Take(10).ToArray();
            var cardsForBob = card.Skip(10).ToArray();


            await Graph.Trade(server, alice, cardsForAlice, new Client.Game.Data.CardInstance[] { }, b => server.Sign(b), b => alice.Sign(b));
            await Graph.Trade(server, bob, cardsForBob, new Client.Game.Data.CardInstance[] { }, b => server.Sign(b), b => bob.Sign(b));


            var cardsOfAlice = await Graph.GetCardsOf(alice);
            var cardsOfBob = await Graph.GetCardsOf(bob);
            CollectionAssert.AreEquivalent(cardsOfAlice.ToArray(), cardsForAlice.Select(x => Tuple.Create(x.Id, x.Creator)).ToArray(), "Alice Cards");
            CollectionAssert.AreEquivalent(cardsOfBob.ToArray(), cardsForBob.Select(x => Tuple.Create(x.Id, x.Creator)).ToArray(), "Bob Cards");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task FailIfAWrongSignatureTest()
        {
            var server = GenerateKeys(1).First();
            var alice = GenerateKeys(1).First();
            var bob = GenerateKeys(1).First();
            var cardData = GenerateCardData(3, server);
            var card = GenerateCards(20, cardData);
            var cardsForAlice = card.Take(10).ToArray();
            var cardsForBob = card.Skip(10).ToArray();
            await Graph.Trade(bob, alice, cardsForAlice, new Client.Game.Data.CardInstance[] { }, b => server.Sign(b), b => alice.Sign(b));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task FailIfBWrongSignatureTest()
        {
            var server = GenerateKeys(1).First();
            var alice = GenerateKeys(1).First();
            var bob = GenerateKeys(1).First();
            var cardData = GenerateCardData(3, server);
            var card = GenerateCards(20, cardData);
            var cardsForAlice = card.Take(10).ToArray();
            var cardsForBob = card.Skip(10).ToArray();
            await Graph.Trade(server, bob, cardsForAlice, new Client.Game.Data.CardInstance[] { }, b => server.Sign(b), b => alice.Sign(b));
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task FailIfFirstTradeNotServerTest()
        {
            var server = GenerateKeys(1).First();
            var alice = GenerateKeys(1).First();
            var bob = GenerateKeys(1).First();
            var cardData = GenerateCardData(3, server);
            var card = GenerateCards(20, cardData);
            var cardsForAlice = card.Take(10).ToArray();
            var cardsForBob = card.Skip(10).ToArray();
            await Graph.Trade(alice, bob, cardsForAlice, new Client.Game.Data.CardInstance[] { }, b => alice.Sign(b), b => bob.Sign(b));
        }

        [TestMethod]
        public async Task LastTest()
        {
            Assert.Fail("Dauert zu lange");
            var server = GenerateKeys(1).First();
            var clients = GenerateKeys(50);
            var cardData = GenerateCardData(100, server);
            var cards = GenerateCards(1000, cardData);

            // Karten Initial vergeben
            foreach (var c in cards)
            {
                var currentClient = clients[r.Next(clients.Length)];
                await Graph.Trade(server, currentClient, new Client.Game.Data.CardInstance[] { c }, new Client.Game.Data.CardInstance[] { }, b => server.Sign(b), b => currentClient.Sign(b));
            }

            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            s.Start();
            //KartenTauschen
            for (int i = 0; i < 100; i++)
            {
                foreach (var c in cards)
                {
                    var targetClient = clients[r.Next(clients.Length)];
                    var owner = await Graph.GetOwner(c);
                    var fromClient = clients.Single(x => x.Modulus.SequenceEqual(owner.Modulus) && x.Exponent.SequenceEqual(owner.Exponent));
                    if (targetClient.Equals(fromClient))
                        targetClient = clients.First(x => !x.Equals(fromClient));

                    await Graph.Trade(fromClient, targetClient, new Client.Game.Data.CardInstance[] { c }, new Client.Game.Data.CardInstance[] { }, b => fromClient.Sign(b), b => targetClient.Sign(b));
                }
            }
            s.Stop();
            Console.WriteLine("All Trades:\t " + s.Elapsed);




            s.Restart();
            await Graph.GetCardsOf(clients.First());
            s.Stop();
            Console.WriteLine("GetCardsOf:\t " + s.Elapsed);

            s.Restart();
            var ownerOfFirstCard = await Graph.GetOwner(cards.First());
            s.Stop();
            Console.WriteLine("GetOwner:\t" + s.Elapsed);

            s.Restart();
            await Graph.CheckOwner(clients.First(), cards.First());
            s.Stop();
            Console.WriteLine("CheckOwner:\t" + s.Elapsed);

            var ownerPk = clients.Single(x => x.Modulus.SequenceEqual(ownerOfFirstCard.Modulus) && x.Exponent.SequenceEqual(ownerOfFirstCard.Exponent));
            var tradeWith = clients.First().Equals(ownerPk) ? clients.Skip(1).First() : clients.First();

            s.Restart();
            await Graph.Trade(ownerPk, tradeWith, new Client.Game.Data.CardInstance[] { cards.First() }, new Client.Game.Data.CardInstance[] { }, b => ownerPk.Sign(b), b => tradeWith.Sign(b));
            s.Stop();
            Console.WriteLine("Trade:\t\t" + s.Elapsed);

            var dbFile = new System.IO.FileInfo("db.sqlite");
            Console.WriteLine("DB Size {0}", dbFile.Length);

        }



        public Security.IPrivateKey[] GenerateKeys(int count)
        {
            var erg = new Security.IPrivateKey[count];
            for (int i = 0; i < erg.Length; i++)
                erg[i] = Security.SecurityFactory.CreatePrivateKey();
            return erg;
        }

        public Client.Game.Data.CardData[] GenerateCardData(int count, params Security.IPrivateKey[] serverKeys)
        {
            var erg = new Client.Game.Data.CardData[count];
            int[] createdCards = new int[Math.Max(Editions.Length, serverKeys.Length)];
            for (int i = 0; i < erg.Length; i++)
            {
                var editionNumber = r.Next(Math.Max(Editions.Length, serverKeys.Length));
                erg[i] = new Client.Game.Data.CardData()
                {
                    Id = GenerateSeededGuid(),
                    Edition = Editions[editionNumber % Editions.Length],
                    CardRevision = 0,
                    Creator = new Client.Game.Data.PublicKey()
                    {
                        Exponent = serverKeys[editionNumber % serverKeys.Length].Exponent,
                        Modulus = serverKeys[editionNumber % serverKeys.Length].Modulus
                    },
                    ImageId = GenerateSeededGuid(),
                    Name = String.Format("{0} - {2} [{1}]", Editions[editionNumber % Editions.Length], serverKeys[editionNumber % serverKeys.Length].FingerPrint(), createdCards[editionNumber] - 1),
                };
            }
            return erg;
        }

        public Client.Game.Data.CardInstance[] GenerateCards(int count, params Client.Game.Data.CardData[] cData)
        {
            var erg = new Client.Game.Data.CardInstance[count];
            for (int i = 0; i < erg.Length; i++)
            {
                var data = cData[r.Next(cData.Length)];
                erg[i] = new Client.Game.Data.CardInstance() { Id = GenerateSeededGuid(), CardDataId = data.Id, Creator = data.Creator };
                // Warum laufen die tests durch obwohl nicht alles unterschrieben ist??
            }
            return erg;
        }


        public Guid GenerateSeededGuid()
        {
            var guid = new byte[16];
            r.NextBytes(guid);
            return new Guid(guid);
        }



        private readonly String[] Editions = new string[]{
"Barony of the Idols",
"Barony of the Victories",
"Cat's County of the Slayers",
"Empire of the Long Forked Waters",
"Kingdom of the Lawful Summoners",
"Living Marquis Country",
"Long Rock Country",
"Old Mussel Country",
"Province of the Arrows",
"Province of the Borders",
"Province of the Long Mace",
"Province of the Seven New Spells",
"Sour State of the Witch",
"State of the Black Soldier's Hope",
"State of the Giant Seals",
"Barony of the Marquis' Man",
"Blessed Fief",
"Country of the Spells",
"County of the Meads",
"County of the Sharp Spirit's Grindstone",
"Dying Wealthy Whale Barony",
"Earl's Kingdom of the Princess",
"Earldom of the Unholy Ebony Poisons",
"Fair Lion's Duchy",
"Good Soft Fisher's Marchessies",
"Land of the Duke",
"Navigator's Land of the Mercies",
"Pink Raids Fief",
"Royal Earldom",
"Summer Archers Marchessies",

        };
    }
}
