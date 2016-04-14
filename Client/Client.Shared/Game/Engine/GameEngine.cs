using Client.Game.Data;
using Client.Game.Data.Games;
using MentalCardGame;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;

namespace Client.Game.Engine
{
    //[PropertyChanged.ImplementPropertyChanged]
    public class GameEngine : INotifyPropertyChanged
    {

        private readonly TaskCompletionSource<object> initCardEngeinWaiter = new TaskCompletionSource<object>();
        private readonly TaskCompletionSource<object> initScriptEngeinWaiter = new TaskCompletionSource<object>();
        public event AsyncEventHandler<EventArgs.MoveCardEventArgs> CardsMoved;
        public event AsyncEventHandler<EventArgs.CardChagnedEventArgs> CardChanged;
        public event AsyncEventHandler<EventArgs.PermutateCardsEventArgs> CardsPermutated;
        public event AsyncEventHandler<EventArgs.AddedCardsEventArgs> CardsAdded;
        public event AsyncEventHandler<EventArgs.ChangeTextEventArgs> TextChanged;
        public event AsyncEventHandler<EventArgs.ShowMessageEventArgs> MessageShown;
        public event Func<IEnumerable<PlayerMove.AbstractAction>, Task<PlayerMove.AbstractAction>> WaitForPlayerMove;


        public event Func<Task<IEnumerable<CardInstance>>> SelectDeckEvent;
        public event PropertyChangedEventHandler PropertyChanged;

        public async Task<IEnumerable<CardInstance>> SelectDeck()
        {
            while (SelectDeckEvent == null)
            {
                Logger.Information($"{nameof(SelectDeckEvent)} in {nameof(GameEngine)} noch nicht gesetzt."); // Busiy waiting ist nicht schön, da der Fall 
                await Task.Delay(200);                        //im normalfall aber nicht auftritt lohnt sich die erhöte Komplexität nicht die ein andere Weg eingehen würde.
            }
            return await SelectDeckEvent();
        }


        private readonly Dictionary<string, Int32> numberVariables = new Dictionary<string, int>();
        private readonly Dictionary<string, string> stringVariables = new Dictionary<string, string>();

        private async Task FireCardsMoved(EventArgs.MoveCardEventArgs e)
        {
            if (CardsMoved != null)
                await CardsMoved(this, e);
        }

        private async Task FireCardChanged(EventArgs.CardChagnedEventArgs e)
        {
            if (CardChanged != null)
                await CardChanged(this, e);
        }

        private async Task FireCardsPermutated(EventArgs.PermutateCardsEventArgs e)
        {
            if (CardsPermutated != null)
                await CardsPermutated(this, e);
        }

        public async Task FireCardsAdded(EventArgs.AddedCardsEventArgs e)
        {
            if (CardsAdded != null)
                await CardsAdded(this, e);
        }

        public async Task FireTextChanged(EventArgs.ChangeTextEventArgs e)
        {
            if (TextChanged != null)
                await TextChanged(this, e);
        }
        public async Task FireShowMessage(EventArgs.ShowMessageEventArgs e)
        {
            if (MessageShown != null)
                await MessageShown(this, e);
        }

        public GameEngine(GameConnectivity connection, Data.Games.Ruleset gameData)
        {
            Connection = connection;
            this.GameData = gameData;
        }


        public GameConnectivity Connection { get; private set; }

        public Data.PlayerNumber CurrentTurn { get; internal set; } = PlayerNumber.Player1;

        private PlayerNumber me;
        public Data.PlayerNumber Me
        {
            get { return me; }
            set
            {
                me = value;
                if (this.InitScriptEngeinWaiter.IsCompleted)
                    this.GameData.ScriptEngin.SetValue("Me", me);
                FirePropertyChanged();
            }
        }

        public PlayerNumber Other { get { return (Me == PlayerNumber.Player1) ? PlayerNumber.Player2 : PlayerNumber.Player1; } }

        public MultiDiconary<PlayerNumber, string, MentalCardGame.Stack> CardRegion { get; } = new MultiDiconary<PlayerNumber, string, Stack>();

        public MultiDiconary<PlayerNumber, string, Data.PlayerNumber> RegionVisibleFor { get; } = new MultiDiconary<PlayerNumber, string, PlayerNumber>();

        public CardEngine CardEngine { get; private set; }

        public CardLookup CardLookup { get; internal set; }

        public Task InitCardEngeinWaiter { get { return initCardEngeinWaiter.Task; } }
        public Task InitScriptEngeinWaiter { get { return initScriptEngeinWaiter.Task; } }

        public Data.Games.Ruleset GameData { get; private set; }

        public IEnumerable<GameDataRegions> Regions { get; private set; }






        internal async Task InitCardEngein(CardEngine engine)
        {
            Regions = (GameData.InvokeGameRuleMethod("GetRegions").ToObject() as System.Collections.IEnumerable).Cast<GameDataRegions>().ToArray();
            CardEngine = engine;
            foreach (var r in Regions)
            {
                switch (r.Player)
                {
                    case PlayerNumber.None:
                        CardRegion.Add(Data.PlayerNumber.None, r.Name, engine.CreateStack());
                        RegionVisibleFor.Add(Data.PlayerNumber.None, r.Name, r.VisibleForPlayer);
                        break;

                    case PlayerNumber.Player1:
                        CardRegion.Add(Data.PlayerNumber.Player1, r.Name, engine.CreateStack());
                        RegionVisibleFor.Add(Data.PlayerNumber.Player1, r.Name, r.VisibleForPlayer);
                        break;

                    case PlayerNumber.Player2:
                        CardRegion.Add(Data.PlayerNumber.Player2, r.Name, engine.CreateStack());
                        RegionVisibleFor.Add(Data.PlayerNumber.Player2, r.Name, r.VisibleForPlayer);
                        break;

                    case PlayerNumber.Any:
                        CardRegion.Add(Data.PlayerNumber.Player1, r.Name, engine.CreateStack());
                        CardRegion.Add(Data.PlayerNumber.Player2, r.Name, engine.CreateStack());
                        RegionVisibleFor.Add(Data.PlayerNumber.Player1, r.Name, r.VisibleForPlayer);
                        RegionVisibleFor.Add(Data.PlayerNumber.Player2, r.Name, r.VisibleForPlayer);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            this.initCardEngeinWaiter.SetResult(this);

        }

        internal async Task InitScriptEngine()
        {

            Logger.Information("Begin Init script.");
            /**
             * Liefert den Stack zurück welcher eine Region repräsentiert.
             */
            // declare function Region(player: Player, name: String): Stack;
            this.GameData.ScriptEngin.SetValue("Region", new Func<PlayerNumber, string, MentalCardGame.Stack>((Data.PlayerNumber player, String region) => CardRegion[player, region]));

            /**
             * Spieler 1.
             */
            // declare var Player1: Player;
            this.GameData.ScriptEngin.SetValue("Player1", PlayerNumber.Player1);

            /**
             * Spieler 2.
             */
            // declare var Player2: Player;
            this.GameData.ScriptEngin.SetValue("Player2", PlayerNumber.Player2);

            /**
             * Kein Spieler
             */
            // declare var PlayerNone: Player;
            this.GameData.ScriptEngin.SetValue("PlayerNone", PlayerNumber.None);

            /**
             * Alle Spieler.
             */
            // declare var PlayerAny: Player;
            this.GameData.ScriptEngin.SetValue("PlayerAny", PlayerNumber.Any);

            /**
             * Man selbst.
             */
            // declare var Me: Player;
            this.GameData.ScriptEngin.SetValue("Me", Me);

            /**
             * Der andere Spieler.
             */
            // declare var Other: Player;
            this.GameData.ScriptEngin.SetValue("Other", Other);

            /**
             * Gibt den Aktuellen Spieler zurück.
             */
            // declare function CurrentPlayer(): Player;
            this.GameData.ScriptEngin.SetValue("CurrentPlayer", new Func<PlayerNumber>(() => CurrentTurn));

            /** 
             * Beendete den Zug des Spielers.
             */
            // declare function EndTurn(): void;
            this.GameData.ScriptEngin.SetValue("EndTurn", new Action(() =>
            {
                this.GameData.InvokeGameRuleMethod("EndOfTurn");
                EndTurn().Wait();
            }));

            /**
             * Zeigt eine/mehrere Karten einem/allen Spielern.
             * @param playerToShow Der Spieler dem die Karten gezeigt werden sollen.
             * @param positionPlayer Der Spieler dem die Region, in welcher sich die Karte befindet, zugeordnet ist.
             * @param positionRegionName Der Name der Region, in welchem sich die Karte befindet.
             * @param positionIndex Der Index der Karte in der Region.
             */
            // declare function ShowCard(playerToShow: Player, positionPlayer: Player[], positionRegionName: string[], positionIndex: number[]): void;
            this.GameData.ScriptEngin.SetValue("ShowCard", new Action<PlayerNumber, object[], object[], object[]>((PlayerNumber playerToShow, object[] positionPlayer, object[] positionRegionName, object[] positionIndex) =>
            {
                var list = new Tuple<PlayerNumber, string, int>[positionPlayer.Length];
                if (positionPlayer.Length != positionIndex.Length || positionIndex.Length != positionRegionName.Length)
                    throw new ArgumentException("Es muss die selbe anzahl aller Positions Argumente vorhanden sein.");
                for (int i = 0; i < list.Length; i++)
                    list[i] = Tuple.Create((PlayerNumber)(double)positionPlayer[i], (string)positionRegionName[i], (int)(double)positionIndex[i]);
                var t = ShowPlayerCard(playerToShow, list);
                t.Wait();
            }));

            /**
             * Mischt einen Stapel (Region).
             * @param player Der Spieler dem die zu mischende Region zugeteilt ist.
             * @param name Der Name der zu mischenden Region.
             */
            // declare function ShuffleStack(player: Player, name: String): void;
            this.GameData.ScriptEngin.SetValue("ShuffleStack", new Action<PlayerNumber, String>((PlayerNumber player, String name) => ShuffleCards(player, name).Wait()));

            /**
             * Verschiebt eine oder Mehrere Karten von einer Position zu einer anderen. Achtung jeder parameter muss die Selbe länge besitzen. Index 0 aller Parameter verschiebt die erste Karte bis Index n verschiebt die n+1te Karte. 
             * @param fromPositionPlayer Der Spieler welchem die QuellRegion zugeschrieben ist.
             * @param fromPositionRegionName Der Name der QuellRegion.
             * @param fromPositionIndex Der QuellIndex.
             * @param toPositionPlayer Der Spieler welchem die ZielRegion zugeschrieben ist.
             * @param toPositionRegionName Der Name der ZielRegion.
             * @param toPositionIndex Der ZielIndex.
             */
            // declare function MoveCard(fromPositionPlayer: Player[], fromPositionRegionName: string[], fromPositionIndex: number[], toPositionPlayer: Player[], toPositionRegionName: string[], toPositionIndex: number[]): void;
            this.GameData.ScriptEngin.SetValue("MoveCard", new Action<object[], object[], object[], object[], object[], object[]>((object[] fromPositionPlayer, object[] fromPositionRegionName, object[] fromPositionIndex, object[] toPositionPlayer, object[] toPositionRegionName, object[] toPositionIndex) =>
            {
                var list = new Tuple<PlayerNumber, string, int, PlayerNumber, string, int>[fromPositionPlayer.Length];
                if (fromPositionPlayer.Length != fromPositionIndex.Length || fromPositionIndex.Length != fromPositionRegionName.Length || toPositionPlayer.Length != toPositionIndex.Length || toPositionIndex.Length != toPositionRegionName.Length || fromPositionIndex.Length != toPositionIndex.Length)
                    throw new ArgumentException("Es muss die selbe anzahl aller Positions Argumente vorhanden sein.");
                for (int i = 0; i < list.Length; i++)
                    list[i] = Tuple.Create((PlayerNumber)(double)fromPositionPlayer[i], (string)fromPositionRegionName[i], (int)(double)fromPositionIndex[i], (PlayerNumber)(double)toPositionPlayer[i], (string)toPositionRegionName[i], (int)(double)toPositionIndex[i]);
                var t = MoveCard(list);
                t.Wait();
            }));

            /**
             * Erzeugt eine/mehrer Karte und fügt diese dem Spiel hinzu. Achtung jeder parameter muss die Selbe länge besitzen. Index 0 aller Parameter erzeugt die erste Karte bis Index n erzeugt die n+1te Karte. In den Meisten Spielen werden die Karten zu Begin erstellt.
             * @param card Ein Array mit den zahlen der Karten welche erstellt werden sollen.
             * @param positionPlayer Der Player, welcher der Region zugeordnet ist, wo die Karte erstellt werden soll.
             * @param positionRegionName Der Name der Region in welcher die Karte erstellt werden soll.
             * @param positionIndex Der Index an der die Karte eingefügt werden soll.
             */
            // declare function AddCards(card: number[], positionPlayer: Player[], positionRegionName: string[], positionIndex: number[]): void;
            this.GameData.ScriptEngin.SetValue("AddCards", new Action<object[], object[], object[], object[]>((object[] card, object[] positionPlayer, object[] positionRegionName, object[] positionIndex) =>
            {
                var list = new Tuple<int, PlayerNumber, string, int>[positionPlayer.Length];
                if (positionPlayer.Length != positionIndex.Length || positionIndex.Length != positionRegionName.Length || positionPlayer.Length != card.Length)
                    throw new ArgumentException("Es muss die selbe anzahl aller Positions Argumente vorhanden sein.");
                for (int i = 0; i < list.Length; i++)
                    list[i] = Tuple.Create((int)(double)card[i], (PlayerNumber)(double)positionPlayer[i], (string)positionRegionName[i], (int)(double)positionIndex[i]);
                var t = AddCards(list);
                t.Wait();
            }));

            /**
            * Setzt den Text der angezeigt werden soll. Z.B. Punkte.
            * @param text Der Text der Angezeigt werden soll.
            */
            // declare function ChangeText(text: string): void;
            this.GameData.ScriptEngin.SetValue("ChangeText", new Action<string>((string text) =>
            {
                var t = ChangeText(text);
                t.Wait();
            }));

            /**
            * Setzt den Text der angezeigt werden soll. Z.B. Punkte.
            * @param text Der Text der Angezeigt werden soll.
            */
            // declare function ShowMessage(text: string): void;
            this.GameData.ScriptEngin.SetValue("ShowMessage", new Action<string>((string message) =>
            {
                var t = ShowMessage(message);
                t.Wait();
            }));

            /**
             * Erzeugt eine Zufällige Natürliche Zahl.
             */
            //declare function Random(): number;
            this.GameData.ScriptEngin.SetValue("Random", new Func<uint>(() => GenerateRandom().Result));

            /**
             * Erzeugt eine Aktion welche der Spieler losgelößt von allem anderen machen kann. (z.B. Zugende)
             * @param name Der Name der Aktion.
             * @param description Die Beschreibung der Aktion.
             * @param func Die Funktion welche aufgerufen werden soll.
             */
            // declare function CreatePlayerAction(name: string, description: string, func: () => void): AbstractAction;
            this.GameData.ScriptEngin.SetValue("CreatePlayerAction", new Func<String, String, Jint.Native.JsValue, PlayerMove.PlayerAction>((name, description, callback) =>
            {
                var objectInstance = GameData.GetJSRulesObject();
                return new PlayerMove.PlayerAction(name, description, () => Task.Run(() => callback.Invoke(objectInstance, new Jint.Native.JsValue[0])));
            }));

            /**
             * Erzeugt eine Aktion welche zu dieser Region gehört.
             * @param regionPlayerNumber der zugehörige Spieler zur Region
             * @param regionName der Name der Region welche die Aktion bekommen soll.
             * @param name Der Name der Aktion.
             * @param description Die Beschreibung der Aktion.
             * @param callback Die Funktion welche aufgerufen werden soll.
             */
            //declare function CreateRegionAction(regionPlayerNumber: Player, regionName: string, name: string, description: string, callback: () => void): AbstractAction;
            this.GameData.ScriptEngin.SetValue("CreateRegionAction", new Func<PlayerNumber, string, String, String, Jint.Native.JsValue, PlayerMove.RegionAction>((regionPlayerNumber, regionName, name, description, callback) =>
            {

                var objectInstance = GameData.GetJSRulesObject();
                return new PlayerMove.RegionAction(regionPlayerNumber, regionName, name, description, () => Task.Run(() => callback.Invoke(objectInstance, new Jint.Native.JsValue[0])));
            }));

            /**
             * Erzeugt eine Aktion die der Nutzer mit dieser Karte auslösen kann.
             * @param card Die Karte welche diese Aktion bekommen soll (z.B. Jede Karte auf der Hand bekommt die Aktion Auspielen)
             * @param name Der Name der zur Aktion angezeigt werden soll.
             * @param description die Beschreibung der Aktion.
             * @param callback Die Funktion welche aufgerufen werden soll wird Die Aktion ausgelößt.
             */
            // declare function CreateCardAction(card: MentalCard, name: string, description: string, callback: () => void): AbstractAction;
            this.GameData.ScriptEngin.SetValue("CreateCardAction", new Func<MentalCardGame.Card, String, String, Jint.Native.JsValue, PlayerMove.CardAction>((card, name, description, callback) =>
            {
                var objectInstance = GameData.GetJSRulesObject();
                return new PlayerMove.CardAction(card, name, description, () => Task.Run(() => callback.Invoke(objectInstance, new Jint.Native.JsValue[0])));
            }));

            /**
             * Gibt die CardData zu einer bestimmten Nummer zurück. 
             * @param cardNumber Die Nummer der Karte von welcher man die Daten haben will.
             */
            // declare function GetCardData(cardNumber: number) : Card
            this.GameData.ScriptEngin.SetValue("GetCardData", new Func<int, Data.CardData>((number) => this.CardLookup[number]));

            /**
              * Lädt eine Nummer, welche mit SetNumber gespeichert wurde.
              * @param name der Variablen.
              */
            // declare function GetNumber(name: string): number;
            this.GameData.ScriptEngin.SetValue("GetNumber", new Func<string, int?>(name =>
            {
                if (this.numberVariables.ContainsKey(name))
                    return numberVariables[name];
                return null;
            }));

            /**
             * Speichert eine Numer in einer Variablen ab. Im gegensatz zu einer normal definierten Variable weiß auch der andere Spieler das diese existiert, und kann auf sie zugreifen.
             * @param name der Variablenname.
             */
            //declare function SetNumber(name: string, value: number): void;
            this.GameData.ScriptEngin.SetValue("SetNumber", new Action<string, int>((name, value) =>
            {
                this.SetNumber(name, value).Wait();
            }));

            /**
             * Lädt einen String welcher mit SetString gespeichert wurde. 
             * @param name der Variablenname.
             */
            //declare function GetString(name: string): string;
            this.GameData.ScriptEngin.SetValue("GetString", new Func<string, String>(name =>
            {
                if (this.stringVariables.ContainsKey(name))
                    return stringVariables[name];
                return null;
            }));

            /**
             * Speichert einen String in einer Variablen ab. Im gegensatz zu einer normal definierten Variable weiß auch der andere Spieler das diese Existiert, und kann auf sie zugreifen.
             * @param name der Variablenname.
             */
            //declare function SetString(name: string, value: string):void;
            this.GameData.ScriptEngin.SetValue("SetString", new Action<string, String>((name, value) =>
            {
                this.SetString(name, value).Wait();
            }));

            /**
             * Erzeugt eine Region.
             * @param player Der Spieler dem die Region zugeordnet sein kann. Kann auch Player.None oder Player.Any sein.
             * @param visibleForPlayer Welche Spieler karten in dieser Region automatisch sehen.
             * @param name Der Name der Region, muss pro player eindeutig sein.
             * @param position gibt an wie weit die Region von der Mitte entfernt ist.
             * @param regionType gibt an wie die Region dargestellt werden soll.
             */
            //declare function ConstructRegion(player: Player, visibleForPlayer: Player, name: string, position: number, regionType: RegionType): GameDataRegions;
            this.GameData.ScriptEngin.SetValue("ConstructRegion", new Func<PlayerNumber, PlayerNumber, string, int, RegionType, GameDataRegions>((player, visibleForPlayer, name, position, regionType) =>
              {
                  return new GameDataRegions(player, visibleForPlayer, name, position, regionType);
              }));


            var script = await GameData.Script.JS;
            await GameData.PrepareRules();

            Logger.Information("Ende Init Script.");

            initScriptEngeinWaiter.SetResult(null);

        }


        public async Task SetNumber(string name, int value)
        {
            var msg = new Data.SetNumber() { Name = name, Value = value };
            await Connection.SendMessage(msg);
            this.numberVariables[name] = value;
        }

        [MessageProcessor(typeof(SetNumber))]
        public async Task ProcessSetNumber()
        {
            var msg = await Connection.Recive<SetNumber>();
            this.numberVariables[msg.Name] = msg.Value;

        }

        public async Task SetString(string name, string value)
        {
            var msg = new Data.SetString() { Name = name, Value = value };
            await Connection.SendMessage(msg);
            this.stringVariables[name] = value;
        }

        [MessageProcessor(typeof(SetString))]
        public async Task ProcessSetString()
        {
            var msg = await Connection.Recive<SetString>();
            this.stringVariables[msg.Name] = msg.Value;

        }


        public async Task AddCards(params Tuple<int, PlayerNumber, string, int>[] cards)
        {
            var msg = new Data.AddCards();
            msg.CardsToAdd.AddRange(
            cards.Select(x => new Data.AddedCard() { CardNumber = x.Item1, Position = new CardPosition() { Player = x.Item2, Region = x.Item3, Index = x.Item4 } })
                );

            await Connection.SendMessage(msg);

            var addedCards = new List<EventArgs.AddedCardsEventArgs.CardsAdded>();
            foreach (var c in msg.CardsToAdd)
            {
                var newCard = CardEngine.CreateCard(c.CardNumber);
                addedCards.Add(new EventArgs.AddedCardsEventArgs.CardsAdded() { Card = newCard, Index = c.Position.Index, Player = c.Position.Player, Region = c.Position.Region });
                this.CardRegion[c.Position.Player, c.Position.Region].Insert(c.Position.Index, newCard);
            }
            await FireCardsAdded(new EventArgs.AddedCardsEventArgs(addedCards));
            await this.AutomaticalCardVisibilitySet(msg.CardsToAdd.Select(x => Tuple.Create(x.Position.Player, x.Position.Region, x.Position.Index)));
        }

        [MessageProcessor(typeof(AddCards))]
        public async Task ProcessAddCards()
        {
            var msg = await Connection.Recive<AddCards>();
            var addedCards = new List<EventArgs.AddedCardsEventArgs.CardsAdded>();
            foreach (var c in msg.CardsToAdd)
            {
                var newCard = CardEngine.CreateCard(c.CardNumber);
                addedCards.Add(new EventArgs.AddedCardsEventArgs.CardsAdded() { Card = newCard, Index = c.Position.Index, Player = c.Position.Player, Region = c.Position.Region });
                this.CardRegion[c.Position.Player, c.Position.Region].Insert(c.Position.Index, newCard);
            }
            await FireCardsAdded(new EventArgs.AddedCardsEventArgs(addedCards));
        }

        public async Task ShowMessage(string message)
        {
            var msg = new Data.ShowMessage();
            msg.Message = message;

            await Connection.SendMessage(msg);

            await FireShowMessage(new EventArgs.ShowMessageEventArgs(message));
        }

        [MessageProcessor(typeof(ShowMessage))]
        public async Task ProcessShowMessage()
        {
            var msg = await Connection.Recive<ShowMessage>();
            await FireShowMessage(new EventArgs.ShowMessageEventArgs(msg.Message));
        }
        public async Task ChangeText(string text)
        {
            var msg = new Data.ChangeText();
            msg.Text = text;

            await Connection.SendMessage(msg);

            await FireTextChanged(new EventArgs.ChangeTextEventArgs(text));
        }

        [MessageProcessor(typeof(ChangeText))]
        public async Task ProcessChangeText()
        {
            var msg = await Connection.Recive<ChangeText>();
            await FireTextChanged(new EventArgs.ChangeTextEventArgs(msg.Text));
        }

        public async Task ShowPlayerCard(PlayerNumber playerToShow, params Tuple<PlayerNumber, string, int>[] cardPositions)
        {
            var msg = new ShowCardsToPlayer() { PlayerToShow = playerToShow };
            msg.Cards.AddRange(cardPositions.Select(x => new CardPosition() { Player = x.Item1, Region = x.Item2, Index = x.Item3 }));
            await Connection.SendMessage(msg);

            if (playerToShow == Me)
            {
                var answer = await Connection.Recive<SendSecrets>();
                var recivedPositions = answer.CardSecrets.Select(x => Tuple.Create(x.Card.Player, x.Card.Region, x.Card.Index));
                foreach (var c in cardPositions)
                    if (!recivedPositions.Contains(c))
                        throw new GameException("Es waren nicht alle angeforderten Karten gesendet worden.");

                foreach (var s in answer.CardSecrets)
                {
                    CardRegion[s.Card.Player, s.Card.Region][s.Card.Index].UnmaskCard(CardEngine.DeSerializeUncoverdSecrets(XElement.Parse(s.Secret)));
                    await FireCardChanged(new EventArgs.CardChagnedEventArgs(CardRegion[s.Card.Player, s.Card.Region][s.Card.Index], CardRegion[s.Card.Player, s.Card.Region][s.Card.Index]));
                }
            }
            else if (playerToShow == Other)
            {
                var msg2 = new SendSecrets();
                msg2.CardSecrets.AddRange(cardPositions.Select(x => new CardSecret() { Card = new CardPosition() { Player = x.Item1, Region = x.Item2, Index = x.Item3 }, Secret = CardEngine.SerializeProveUncoverdSecrets(CardRegion[x.Item1, x.Item2][x.Item3].UncoverSecrets()).ToString() }));
                await Connection.SendMessage(msg2);
            }
            else if (playerToShow == PlayerNumber.Any)
            {
                var msg2 = new SendSecrets();
                msg2.CardSecrets.AddRange(cardPositions.Select(x => new CardSecret() { Card = new CardPosition() { Player = x.Item1, Region = x.Item2, Index = x.Item3 }, Secret = CardEngine.SerializeProveUncoverdSecrets(CardRegion[x.Item1, x.Item2][x.Item3].UncoverSecrets()).ToString() }));
                await Connection.SendMessage(msg2);

                var answer = await Connection.Recive<SendSecrets>();
                var recivedPositions = answer.CardSecrets.Select(x => Tuple.Create(x.Card.Player, x.Card.Region, x.Card.Index));
                foreach (var c in cardPositions)
                    if (!recivedPositions.Contains(c))
                        throw new GameException("Es waren nicht alle angeforderten Karten gesendet worden.");

                foreach (var s in answer.CardSecrets)
                {
                    CardRegion[s.Card.Player, s.Card.Region][s.Card.Index].UnmaskCard(CardEngine.DeSerializeUncoverdSecrets(XElement.Parse(s.Secret)));
                    await FireCardChanged(new EventArgs.CardChagnedEventArgs(CardRegion[s.Card.Player, s.Card.Region][s.Card.Index], CardRegion[s.Card.Player, s.Card.Region][s.Card.Index]));
                }
            }
        }

        [MessageProcessor(typeof(ShowCardsToPlayer))]
        public async Task ProcessShowPlayerCard()
        {
            var msg = await Connection.Recive<ShowCardsToPlayer>();

            if (msg.PlayerToShow == Me)
            {
                var answer = await Connection.Recive<SendSecrets>();
                var recivedPositions = answer.CardSecrets.Select(x => Tuple.Create(x.Card.Player, x.Card.Region, x.Card.Index)).ToArray();
                foreach (var c in msg.Cards.Select(x => Tuple.Create(x.Player, x.Region, x.Index)))
                    if (!recivedPositions.Contains(c))
                        throw new GameException("Es waren nicht alle angeforderten Karten gesendet worden.");

                foreach (var s in answer.CardSecrets)
                {
                    CardRegion[s.Card.Player, s.Card.Region][s.Card.Index].UnmaskCard(CardEngine.DeSerializeUncoverdSecrets(XElement.Parse(s.Secret)));
                    await FireCardChanged(new EventArgs.CardChagnedEventArgs(CardRegion[s.Card.Player, s.Card.Region][s.Card.Index], CardRegion[s.Card.Player, s.Card.Region][s.Card.Index]));
                }
            }
            else if (msg.PlayerToShow == Other)
            {
                var msg2 = new SendSecrets();
                msg2.CardSecrets.AddRange(msg.Cards.Select(x => new CardSecret() { Card = new CardPosition() { Player = x.Player, Region = x.Region, Index = x.Index }, Secret = CardEngine.SerializeProveUncoverdSecrets(CardRegion[x.Player, x.Region][x.Index].UncoverSecrets()).ToString() }));
                await Connection.SendMessage(msg2);
            }
            else if (msg.PlayerToShow == PlayerNumber.Any)
            {
                var msg2 = new SendSecrets();
                msg2.CardSecrets.AddRange(msg.Cards.Select(x => new CardSecret() { Card = new CardPosition() { Player = x.Player, Region = x.Region, Index = x.Index }, Secret = CardEngine.SerializeProveUncoverdSecrets(CardRegion[x.Player, x.Region][x.Index].UncoverSecrets()).ToString() }));
                await Connection.SendMessage(msg2);

                var answer = await Connection.Recive<SendSecrets>();
                var recivedPositions = answer.CardSecrets.Select(x => Tuple.Create(x.Card.Player, x.Card.Region, x.Card.Index)).ToArray();
                foreach (var c in msg.Cards.Select(x => Tuple.Create(x.Player, x.Region, x.Index)))
                    if (!recivedPositions.Contains(c))
                        throw new GameException("Es waren nicht alle angeforderten Karten gesendet worden.");

                foreach (var s in answer.CardSecrets)
                {
                    CardRegion[s.Card.Player, s.Card.Region][s.Card.Index].UnmaskCard(CardEngine.DeSerializeUncoverdSecrets(XElement.Parse(s.Secret)));
                    await FireCardChanged(new EventArgs.CardChagnedEventArgs(CardRegion[s.Card.Player, s.Card.Region][s.Card.Index], CardRegion[s.Card.Player, s.Card.Region][s.Card.Index]));
                }
            }
        }

        public async Task ShuffleCards(PlayerNumber player, String region)
        {
            var w = System.Diagnostics.Stopwatch.StartNew();

            await Connection.SendMessage(new ShuffleStack() { Player = player, Region = region });
            await PermutateCards(player, region);
            await ProcessPermutateCards();

            await AutomaticalCardVisibilitySet(Enumerable.Range(0, CardRegion[player, region].Count).Select(x => Tuple.Create(player, region, x)));

            w.Stop();
            Logger.Information(System.String.Format("ShuffleCards brauchte {0}", w.Elapsed));
        }

        [MessageProcessor(typeof(ShuffleStack))]
        public async Task ProcessShuffleCards()
        {
            var shuffle = await Connection.Recive<ShuffleStack>();
            await ProcessPermutateCards();
            await PermutateCards(shuffle.Player, shuffle.Region);
        }

        public async Task MoveCard(params Tuple<PlayerNumber, string, int, PlayerNumber, string, int>[] cardsMovements)
        {
            var w = System.Diagnostics.Stopwatch.StartNew();

            var order = new MoveCard();
            order.From.AddRange(cardsMovements.Select(x => new CardPosition() { Player = x.Item1, Region = x.Item2, Index = x.Item3 }));
            order.To.AddRange(cardsMovements.Select(x => new CardPosition() { Player = x.Item4, Region = x.Item5, Index = x.Item6 }));
            var msg = Connection.SendMessage(order);
            foreach (var c in cardsMovements)
            {
                PlayerNumber playerFrom = c.Item1;
                string regionFrom = c.Item2;
                int indexFrom = c.Item3;

                PlayerNumber playerTo = c.Item4;
                string regionTo = c.Item5;
                int indexTo = c.Item6;

                var fromStack = CardRegion[playerFrom, regionFrom];
                var toStack = CardRegion[playerTo, regionTo];

                var card = fromStack[indexFrom];
                fromStack.RemoveAt(indexFrom);
                toStack.Insert(indexTo, card);
            }
            await FireCardsMoved(new EventArgs.MoveCardEventArgs(order));
            await msg;

            // Sichtbarkeit Automatisch setzen
            await AutomaticalCardVisibilitySet(cardsMovements.Select(x => Tuple.Create(x.Item4, x.Item5, x.Item6)));

            w.Stop();
            Logger.Information(System.String.Format("MoveCard brauchte {0}", w.Elapsed));
        }

        private async Task AutomaticalCardVisibilitySet(IEnumerable<Tuple<PlayerNumber, string, int>> cardsMovements)
        {
            var g = cardsMovements.GroupBy(x => new { ToPlayer = x.Item1, ToRegion = x.Item2 });
            foreach (var group in g)
            {
                if (RegionVisibleFor[group.Key.ToPlayer, group.Key.ToRegion] != PlayerNumber.None)
                    await this.ShowPlayerCard(RegionVisibleFor[group.Key.ToPlayer, group.Key.ToRegion], group.Select(x => Tuple.Create(x.Item1, x.Item2, x.Item3)).ToArray());
            }
        }

        [MessageProcessor(typeof(MoveCard))]
        public async Task ProcessMoveCard()
        {
            var order = await Connection.Recive<MoveCard>();

            if (order.From.Count != order.To.Count)
                throw new GameException("Anzahl der Gesendeten Elemente sollte glech sein beim verschieben von Karten.");
            for (int i = 0; i < order.From.Count; i++)
            {
                var fromStack = CardRegion[order.From[i].Player, order.From[i].Region];
                var toStack = CardRegion[order.To[i].Player, order.To[i].Region];

                var card = fromStack[order.From[i].Index];
                fromStack.RemoveAt(order.From[i].Index);
                toStack.Insert(order.To[i].Index, card);
            }
            await FireCardsMoved(new EventArgs.MoveCardEventArgs(order));
        }

        public async Task<uint> GenerateRandom()
        {
            var w = System.Diagnostics.Stopwatch.StartNew();

            var rNumber = App.RNG.Next();
            var rBuffer = BitConverter.GetBytes(rNumber).AsBuffer();
            var sha512 = Windows.Security.Cryptography.Core.HashAlgorithmProvider.OpenAlgorithm(Windows.Security.Cryptography.Core.HashAlgorithmNames.Sha512);
            var rHash = sha512.HashData(rBuffer);
            var msg = new GenerateRandomNumber() { HashedNumber = rHash.ToArray() };
            await Connection.SendMessage(msg);
            var answer = await Connection.Recive<RandomNumber>();
            var msg2 = new RandomNumber() { Value = rNumber };
            await Connection.SendMessage(msg2);

            w.Stop();
            Logger.Information(System.String.Format("GenerateRandom brauchte {0}", w.Elapsed));

            return rNumber ^ answer.Value;
        }

        [MessageProcessor(typeof(GenerateRandomNumber))]
        public async Task<uint> ProcessGenerateRandom()
        {
            var answer = new RandomNumber() { Value = App.RNG.Next() };
            await Connection.SendMessage(answer);

            var order = await Connection.Recive<GenerateRandomNumber>();
            var rHash = order.HashedNumber;
            var sha512 = Windows.Security.Cryptography.Core.HashAlgorithmProvider.OpenAlgorithm(Windows.Security.Cryptography.Core.HashAlgorithmNames.Sha512);

            var msg = await Connection.Recive<RandomNumber>();
            var rBytes = BitConverter.GetBytes(msg.Value);
            if (!sha512.HashData(rBytes.AsBuffer()).ToArray().SequenceEqual(rHash))
                throw new GameException("Fehler beim brechnen von Random. Eventuell betrügender Client");
            return msg.Value ^ answer.Value;
        }

        private async Task PermutateCards(PlayerNumber player, String region)
        {
            var w = System.Diagnostics.Stopwatch.StartNew();
            var toShuffle = this.CardRegion[player, region];
            var permutatedStack = toShuffle.Shuffle();
            await Connection.SendMessage(new PermutateStack() { Player = player, Region = region, PermutatedSteak = CardEngine.SerializeStack(permutatedStack).ToString() });
            var time = w.Elapsed;
            var phase1 = permutatedStack.ProveShuffle(1);
            await Connection.SendMessage(new Prove() { Data = CardEngine.SerializeProveShufflePhase1Output(phase1).ToString() });
            var prove = await Connection.Recive<Prove>();
            var phase2 = CardEngine.DeSerializeProveShufflePhase2Output(System.Xml.Linq.XElement.Parse(prove.Data));
            var phase3 = permutatedStack.ProveShufflePhase3(phase1, phase2);
            await Connection.SendMessage(new Prove() { Data = CardEngine.SerializeProveShufflePhase3Output(phase3).ToString() });
            var confirmation = await Connection.Recive<Confirmation>();
            if (confirmation != Confirmation.Ok)
                throw new GameException("Anderer Client aktziptiert permutation nicht. :(");
            this.CardRegion[player, region] = permutatedStack;
            await FireCardsPermutated(new EventArgs.PermutateCardsEventArgs(permutatedStack, toShuffle));

            w.Stop();
            Logger.Information(System.String.Format("PermutateCards brauchte {0}", w.Elapsed));
        }

        private async Task ProcessPermutateCards()
        {
            var shuffleStack = await Connection.Recive<PermutateStack>();

            PlayerNumber player = shuffleStack.Player;
            String region = shuffleStack.Region;
            Stack toShuffle = CardRegion[player, region];
            Stack permutatedStack = CardEngine.DeSerializeStack(System.Xml.Linq.XElement.Parse(shuffleStack.PermutatedSteak));

            var prove = await Connection.Recive<Prove>();
            var phase1 = CardEngine.DeSerializeProveShufflePhase1Output(System.Xml.Linq.XElement.Parse(prove.Data));
            var phase2 = permutatedStack.ProveShufflePhase2(CardRegion[player, region], phase1);
            await Connection.SendMessage(new Prove() { Data = CardEngine.SerializeProveShufflePhase2Output(phase2).ToString() });
            prove = await Connection.Recive<Prove>();
            var phase3 = CardEngine.DeSerializeProveShufflePhase3Output(System.Xml.Linq.XElement.Parse(prove.Data));
            var erg = permutatedStack.ProveShufflePhase4(phase1, phase2, phase3);
            if (!erg)
            {
                await Connection.SendMessage(Confirmation.Error);
                throw new GameException("Anderer Client hat fehlerhafte Permutation gesendet. :(");
            }
            CardRegion[player, region] = permutatedStack;
            await FireCardsPermutated(new EventArgs.PermutateCardsEventArgs(permutatedStack, toShuffle));
            await Connection.SendMessage(Confirmation.Ok);
        }

        public async Task EndTurn()
        {
            var w = System.Diagnostics.Stopwatch.StartNew();
            await Connection.SendMessage(new TurnEnd());
            CurrentTurn = CurrentTurn == Data.PlayerNumber.Player1 ? Data.PlayerNumber.Player2 : Data.PlayerNumber.Player1;
            w.Stop();
            Logger.Information(System.String.Format("TurnEnd brauchte {0}", w.Elapsed));
        }

        [MessageProcessor(typeof(TurnEnd))]
        public async Task ProcessEndTurn()
        {
            var shuffleStack = await Connection.Recive<TurnEnd>();
            CurrentTurn = CurrentTurn == Data.PlayerNumber.Player1 ? Data.PlayerNumber.Player2 : Data.PlayerNumber.Player1;
        }

        internal Task<PlayerMove.AbstractAction> WaitForInput(IEnumerable<PlayerMove.AbstractAction> actions)
        {
            if (this.WaitForPlayerMove == null)
                throw new InvalidOperationException("Es kann keiner Antworten.");
            if (this.WaitForPlayerMove.GetInvocationList().Length != 1)
                throw new InvalidOperationException("Es Darf nur eine Gui Angemeldet sein.");

            return WaitForPlayerMove(actions);
        }

        internal Jint.Native.JsValue InvokeGameRuleMethod(string methodName, params Jint.Native.JsValue[] parameter)
        {
            return this.GameData.InvokeGameRuleMethod(methodName, parameter);
        }

        private void FirePropertyChanged([CallerMemberName] string property = null)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}