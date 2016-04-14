using Client.Game.Engine;
using Client.Game.Engine.EventArgs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Client.Game.Data;
using Windows.Foundation;
using MentalCardGame;
using Client.Game.Data.Games;

namespace Client.Viewmodel.Game
{
    internal class GameViewmodel : DependencyObject
    {
        private const double STACFACTOR = 0.3;

        public ObservableCollection<CardViewmodel> Cards { get; } = new ObservableCollection<CardViewmodel>();

        private readonly Dictionary<MentalCardGame.Card, CardViewmodel> cardLookup = new Dictionary<MentalCardGame.Card, CardViewmodel>(new Common.ReferenceComparer<MentalCardGame.Card>());

        private TaskCompletionSource<Client.Game.Engine.PlayerMove.AbstractAction> nexMove;

        private IEnumerable<Client.Game.Engine.PlayerMove.CardAction> cardActions;
        private IEnumerable<Client.Game.Engine.PlayerMove.RegionAction> regionActions;

        public MultiDiconary<PlayerNumber, string, IList<MentalCardGame.Card>> CardRegion { get; } = new MultiDiconary<PlayerNumber, string, IList<MentalCardGame.Card>>();


        public ObservableCollection<RegionViewmodel> RegionsRect { get; } = new ObservableCollection<RegionViewmodel>();


        private System.Threading.DisposingUsingSemaphore Semaphore { get; } = new System.Threading.DisposingUsingSemaphore();

        public IEnumerable<Client.Game.Engine.PlayerMove.PlayerAction> PlayerActions
        {
            get { return (IEnumerable<Client.Game.Engine.PlayerMove.PlayerAction>)GetValue(PlayerActionsProperty); }
            set { SetValue(PlayerActionsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlayerActions.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlayerActionsProperty =
            DependencyProperty.Register("PlayerActions", typeof(IEnumerable<Client.Game.Engine.PlayerMove.PlayerAction>), typeof(GameViewmodel), new PropertyMetadata(null));

        public void CardClicked(CardViewmodel clickedCard)
        {
            if (nexMove == null)
                return;

            var actions = cardActions.Where(x => x.Card == clickedCard.Card).ToArray();

            if (actions.Any())
            {
                var move = nexMove; // Nicht ausserhalb des if Ziehen, da es nur gelöcht wernde darf wenn es eine Karte gibt.
                nexMove = null;

                if (actions.Length == 1)
                    move.SetResult(actions[0]);
                else
                {
                    //TODO Handle
                }
            }
            else // Überprüfe ob die Karte einer Region angehört, welche eine Action hat.
            {
                var r = Engine.CardRegion.Single(x => x.Value.Contains(clickedCard.Card));
                var rActions = regionActions.Where(x => x.Player == r.Key.Item1 && x.Region == r.Key.Item2).ToArray();
                if (!rActions.Any())
                    return;
                var move = nexMove;
                nexMove = null;

                if (rActions.Length == 1)
                    move.SetResult(rActions[0]);
                else
                {
                    //ToDO Handle multiSelect
                }
            }
        }

        public void RegionClicked(RegionViewmodel clickedCard)
        {
            if (nexMove == null)
                return;

            var actions = regionActions.Where(x => x.Region == clickedCard.RegionName && clickedCard.Player == x.Player).ToArray();

            if (actions.Any())
            {
                var move = nexMove;
                nexMove = null;

                if (actions.Length == 1)
                    move.SetResult(actions[0]);
                else
                {
                    //TODO Handle
                }
            }
        }

        public void PlayerActionClicked(Client.Game.Engine.PlayerMove.PlayerAction action)
        {
            if (nexMove == null)
                return;

            if (this.PlayerActions.Contains(action))
            {
                this.PlayerActions = null;
                var move = nexMove;
                nexMove = null;
                move.SetResult(action);
            }
        }

        public GameEngine Engine
        {
            get { return (GameEngine)GetValue(EngineProperty); }
            set { SetValue(EngineProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Engine.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EngineProperty =
            DependencyProperty.Register("Engine", typeof(GameEngine), typeof(GameViewmodel), new PropertyMetadata(null, EngineChanged));

        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Width.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width", typeof(double), typeof(GameViewmodel), new PropertyMetadata(0.0, SizeChanged));

        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Height.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(GameViewmodel), new PropertyMetadata(0.0, SizeChanged));



        public double CurrentWidth
        {
            get { return (double)GetValue(CurrentWidthProperty); }
            set { SetValue(CurrentWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentWidthProperty =
            DependencyProperty.Register("CurrentWidth", typeof(double), typeof(GameViewmodel), new PropertyMetadata(0.0));





        public double CurrentHeight
        {
            get { return (double)GetValue(CurrentHeightProperty); }
            set { SetValue(CurrentHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentHeightProperty =
            DependencyProperty.Register("CurrentHeight", typeof(double), typeof(GameViewmodel), new PropertyMetadata(0.0));




        private static void SizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as GameViewmodel;
            me.CalculatePositions();
        }




        public string GameText
        {
            get { return (string)GetValue(GameTextProperty); }
            set { SetValue(GameTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GameText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GameTextProperty =
            DependencyProperty.Register("GameText", typeof(string), typeof(GameViewmodel), new PropertyMetadata(""));



        public double CardHeight
        {
            get { return (double)GetValue(CardHeightProperty); }
            set { SetValue(CardHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CardHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CardHeightProperty =
            DependencyProperty.Register("CardHeight", typeof(double), typeof(GameViewmodel), new PropertyMetadata(0.0));

        public double CardWidth
        {
            get { return (double)GetValue(CardWidthProperty); }
            set { SetValue(CardWidthProperty, value); }
        }

        public bool Initilized { get; private set; }

        // Using a DependencyProperty as the backing store for CardWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CardWidthProperty =
            DependencyProperty.Register("CardWidth", typeof(double), typeof(GameViewmodel), new PropertyMetadata(0.0));

        public Thickness Margin
        {
            get { return (Thickness)GetValue(MarginProperty); }
            set { SetValue(MarginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Margin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarginProperty =
            DependencyProperty.Register("Margin", typeof(Thickness), typeof(GameViewmodel), new PropertyMetadata(default(Thickness)));

        private static void EngineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as GameViewmodel;
            var newEngine = e.NewValue as GameEngine;
            var oldEngine = e.OldValue as GameEngine;

            if (oldEngine != null)
            {
                newEngine.CardChanged -= me.EngineCardChanged;
                newEngine.CardsMoved -= me.EngineCardsMoved;
                newEngine.CardsPermutated -= me.EngineCardsPermutated;
                newEngine.CardsAdded -= me.EngineCardsAdded;
                newEngine.WaitForPlayerMove -= me.WaitForPlayerMove;
                newEngine.TextChanged -= me.TextChanged;
                newEngine.MessageShown -= me.ShowMessage;
            }

            if (newEngine != null)
            {
                newEngine.CardChanged += me.EngineCardChanged;
                newEngine.CardsMoved += me.EngineCardsMoved;
                newEngine.CardsPermutated += me.EngineCardsPermutated;
                newEngine.CardsAdded += me.EngineCardsAdded;
                newEngine.WaitForPlayerMove += me.WaitForPlayerMove;
                newEngine.TextChanged += me.TextChanged;
                newEngine.MessageShown += me.ShowMessage;
            }
            me.Initilize();
        }

        private async Task ShowMessage(object sender, ShowMessageEventArgs e)
        {
            if (!this.Dispatcher.HasThreadAccess)
            {
                Task t = null;
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() => t = ShowMessage(sender, e)));
                await t;
                return;
            }
            Windows.UI.Popups.MessageDialog m = new Windows.UI.Popups.MessageDialog(e.Message);
            await m.ShowAsync();
        }

        private async Task TextChanged(object sender, ChangeTextEventArgs e)
        {
            if (!this.Dispatcher.HasThreadAccess)
            {
                Task t = null;
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() => t = TextChanged(sender, e)));
                await t;
                return;
            }
            this.GameText = e.Text;
        }

        private async Task EngineCardsAdded(object sender, AddedCardsEventArgs e)
        {
            if (!this.Dispatcher.HasThreadAccess)
            {
                Task t = null;
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() => t = EngineCardsAdded(sender, e)));
                await t;
                return;
            }
            using (await this.Semaphore.Enter())
            {
                var back = new BitmapImage(new Uri("ms-appx:///Assets/Back.png", UriKind.Absolute));

                foreach (var card in e.NewCards)
                {
                    var model = new CardViewmodel();
                    model.Card = card.Card;
                    cardLookup.Add(card.Card, model);
                    model.X = Width / 2 - CardWidth / 2;
                    model.Y = Height / 2 - CardHeight / 2;
                    model.BackImage = back;
                    if (card.Card.Type.HasValue)
                    {
                        var cardData = Engine.CardLookup[card.Card];
                        var server = cardData.UuidServer.Server;
                        var imageId = cardData.ImageId;
                        var uri = await DDR.GetCardDataImageUri(imageId, server);
                        if (uri != null)
                            model.FrontImage = new BitmapImage(uri);
                    }
                    this.CardRegion[card.Player, card.Region].Insert(card.Index, card.Card);
                    this.Cards.Add(model);
                }
            }
            await CalculatePositions();
        }

        private async Task<Client.Game.Engine.PlayerMove.AbstractAction> WaitForPlayerMove(IEnumerable<Client.Game.Engine.PlayerMove.AbstractAction> actions)
        {
            if (!this.Dispatcher.HasThreadAccess)
            {
                Task t = null;
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() => t = WaitForPlayerMove(actions)));

                return await nexMove.Task;
            }
            if (this.nexMove != null)
                throw new InvalidOperationException("Der letzte Move wurde noch nicht gesetzt.");

            this.cardActions = actions.OfType<Client.Game.Engine.PlayerMove.CardAction>();
            this.regionActions = actions.OfType<Client.Game.Engine.PlayerMove.RegionAction>();
            this.PlayerActions = actions.OfType<Client.Game.Engine.PlayerMove.PlayerAction>();

            this.nexMove = new TaskCompletionSource<Client.Game.Engine.PlayerMove.AbstractAction>();
            return await nexMove.Task;
        }

        private async Task EngineCardsPermutated(object sender, Client.Game.Engine.EventArgs.PermutateCardsEventArgs e)
        {
            if (!this.Dispatcher.HasThreadAccess)
            {
                Task t = null;
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() => t = EngineCardsPermutated(sender, e)));
                await t;
                return;
            }

            if (e.NewValue.Count != e.OldValue.Count)
                throw new ArgumentException("Wenn der Stack Permutiert wurde muss er gnausoviele Karten wie vorher haben.");

            for (int i = 0; i < e.NewValue.Count; i++)
            {
                var oldCard = e.OldValue[i];
                var newCard = e.NewValue[i];
                await EngineCardChanged(sender, new Client.Game.Engine.EventArgs.CardChagnedEventArgs(newCard, oldCard));
            }

            await CalculatePositions();
        }

        private async Task EngineCardsMoved(object sender, Client.Game.Engine.EventArgs.MoveCardEventArgs e)
        {
            if (!this.Dispatcher.HasThreadAccess)
            {
                Task t = null;
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() => t = EngineCardsMoved(sender, e)));
                await t;
                return;
            }

            for (int i = 0; i < e.MovedCards.From.Count; i++)
            {
                var fromStack = CardRegion[e.MovedCards.From[i].Player, e.MovedCards.From[i].Region];
                var toStack = CardRegion[e.MovedCards.To[i].Player, e.MovedCards.To[i].Region];

                var card = fromStack[e.MovedCards.From[i].Index];
                fromStack.RemoveAt(e.MovedCards.From[i].Index);
                toStack.Insert(e.MovedCards.To[i].Index, card);
            }

            await CalculatePositions();
        }

        private async Task EngineCardChanged(object sender, Client.Game.Engine.EventArgs.CardChagnedEventArgs e)
        {
            if (!this.Dispatcher.HasThreadAccess)
            {
                Task t = null;
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() => t = EngineCardChanged(sender, e)));
                await t;
                return;
            }
            using (await this.Semaphore.Enter())
            {

                var m = cardLookup[e.OldValue];


                //HACK Suche die Richtige Instanz
                foreach (var kv in CardRegion)
                {
                    var list = kv.Value;
                    var c = list.Select((x, index) => new { Index = index, Value = x }).Where(x => Object.ReferenceEquals(x.Value, e.OldValue)).ToArray();
                    foreach (var item in c)
                    {
                        list[item.Index] = e.NewValue;
                    }
                }

                m.Card = e.NewValue;
                m.FaceUp = e.NewValue.Type.HasValue; // Sollte sich die Karte Selbst nicht ändern, sondern nur ihr Wert bekannt werden, so wird die überprüfung der Zuordnung nciht gefeuert. Also Hier :P
                if (m.FaceUp)
                {
                    var cardData = Engine.CardLookup[e.NewValue];
                    var server = cardData.UuidServer.Server;
                    var imageId = cardData.ImageId;
                    var uri = await DDR.GetCardDataImageUri(imageId, server);
                    if (uri != null)
                        m.FrontImage = new BitmapImage(uri);
                    // m.FrontImage = new BitmapImage(await DDR.GetCardDataImageUri(Engine.CardLookup[e.NewValue].UuidServer));
                }
                cardLookup.Remove(e.OldValue);
                cardLookup.Add(e.NewValue, m);
            }
        }

        private async void Initilize()
        {
            var back = new BitmapImage(new Uri("ms-appx:///Assets/Back.png", UriKind.Absolute));

            await Engine.InitCardEngeinWaiter; // Warten bis die Engin Initialisiert ist.

            var r = new System.Random();
            using (await this.Semaphore.Enter())
            {
                foreach (var originalRegion in Engine.CardRegion)
                {
                    this.CardRegion.Add(originalRegion.Key, new List<MentalCardGame.Card>());
                    var region = this.CardRegion[originalRegion.Key];
                    foreach (var card in originalRegion.Value)
                    {
                        region.Add(card);
                        var model = new CardViewmodel();
                        model.Card = card;
                        cardLookup.Add(card, model);
                        model.X = Width / 2 - CardWidth / 2;
                        model.Y = Height / 2 - CardHeight / 2;
                        model.BackImage = back;
                        if (card.Type.HasValue)
                        {
                            var cardData = Engine.CardLookup[card];
                            var server = cardData.UuidServer.Server;
                            var imageId = cardData.ImageId;
                            var uri = await DDR.GetCardDataImageUri(imageId, server);
                            if (uri != null)
                                model.FrontImage = new BitmapImage(uri);
                            //model.FrontImage = new BitmapImage(await DDR.GetCardDataImageUri(Engine.CardLookup[card].UuidServer));
                        }

                        this.Cards.Add(model);
                    }
                }
                this.Initilized = true;
            }
            await CalculatePositions();
        }

        private async Task CalculatePositions()
        {
            if (!Initilized)
                return;
            int index = 0;
            using (await this.Semaphore.Enter())
            {

                var regions = this.CardRegion.Select(x => new { RegeionName = x.Key.Item2, RegeionPlayer = x.Key.Item1, Cards = x.Value, RegionInfo = Engine.Regions.Single(y => y.Name == x.Key.Item2 && (y.Player == PlayerNumber.Any || y.Player == x.Key.Item1)) });
                var orderdRegions = regions.OrderBy(x => x.RegionInfo.Position).GroupBy(x => x.RegeionPlayer);
                var regionSizes = regions.Select(x => new { Size = CalculateDesiredRegionSize(x.RegeionPlayer, x.RegeionName, x.RegionInfo, x.Cards), Key = x }).ToDictionary(x => Tuple.Create(x.Key.RegeionPlayer, x.Key.RegeionName), x => x.Size);

                var neutralRowsCount = orderdRegions.Any(x => x.Key == PlayerNumber.None) ? orderdRegions.Single(x => x.Key == PlayerNumber.None).Select(x => x.RegionInfo.Position).Max() + 1 : 0;
                var ownRowsCount = orderdRegions.Any(x => x.Key == Engine.Me) ? orderdRegions.Single(x => x.Key == Engine.Me).Select(x => x.RegionInfo.Position).Max() + 1 : 0;
                var otherRowsCount = orderdRegions.Any(x => x.Key == Engine.Other) ? orderdRegions.Single(x => x.Key == Engine.Other).Select(x => x.RegionInfo.Position).Max() + 1 : 0;

                var neutralRows = orderdRegions.Where(x => x.Key == PlayerNumber.None).SelectMany(x => x).ToLookup(x => x.RegionInfo.Position);
                var ownRows = orderdRegions.Where(x => x.Key == Engine.Me).SelectMany(x => x).ToLookup(x => x.RegionInfo.Position);
                var otherRows = orderdRegions.Where(x => x.Key == Engine.Other).SelectMany(x => x).ToLookup(x => x.RegionInfo.Position);

                // Zeile 0 wird gesondert behandelt, da sie von beiden Spielern geteilt werden.

                var neutralHeight = neutralRows.Select(x => x.Max(r => regionSizes[Tuple.Create(r.RegeionPlayer, r.RegeionName)].Height)).Sum();
                var ownHeight = ownRows.Select(x => x.Max(r => regionSizes[Tuple.Create(r.RegeionPlayer, r.RegeionName)].Height)).Sum();
                var otherHeight = otherRows.Select(x => x.Max(r => regionSizes[Tuple.Create(r.RegeionPlayer, r.RegeionName)].Height)).Sum();

                var completeHeight = neutralHeight + ownHeight + otherHeight;

                var rowTop = 0.0;// this.Height - completeHeight;

                rowTop = Math.Max(0, rowTop);



                for (int i = otherRowsCount - 1; i >= 0; i--)
                {
                    if (!otherRows[i].Any())
                        continue;
                    var thisRowLeft = 0.0;
                    var thisRowHight = otherRows[i].Max(r => regionSizes[Tuple.Create(r.RegeionPlayer, r.RegeionName)].Height);
                    foreach (var r in otherRows[i])
                    {
                        var size = regionSizes[Tuple.Create(r.RegeionPlayer, r.RegeionName)];
                        var rec = this.RegionsRect.FirstOrDefault(x => x.Player == r.RegeionPlayer && r.RegeionName == x.RegionName);
                        if (rec == null)
                        {
                            rec = new RegionViewmodel() { Player = r.RegeionPlayer, RegionName = r.RegeionName };
                            this.RegionsRect.Add(rec);
                        }
                        rec.Rect = new Rect(new Point(thisRowLeft, rowTop), size);
                        LayoutRegion(thisRowLeft, rowTop, size.Width, size.Height, r.RegeionPlayer, r.RegeionName, r.RegionInfo, r.Cards);
                        thisRowLeft += size.Width;
                    }

                    rowTop += thisRowHight;
                }



                for (int i = 0; i < neutralRowsCount; i++)
                {
                    if (!neutralRows[i].Any())
                        continue;
                    var thisRowLeft = 0.0;
                    var thisRowHight = neutralRows[i].Max(r => regionSizes[Tuple.Create(r.RegeionPlayer, r.RegeionName)].Height);
                    foreach (var r in neutralRows[i])
                    {
                        var size = regionSizes[Tuple.Create(r.RegeionPlayer, r.RegeionName)];
                        var rec = this.RegionsRect.FirstOrDefault(x => x.Player == r.RegeionPlayer && r.RegeionName == x.RegionName);
                        if (rec == null)
                        {
                            rec = new RegionViewmodel() { Player = r.RegeionPlayer, RegionName = r.RegeionName };
                            this.RegionsRect.Add(rec);
                        }
                        rec.Rect = new Rect(new Point(thisRowLeft, rowTop), size);

                        LayoutRegion(thisRowLeft, rowTop, size.Width, size.Height, r.RegeionPlayer, r.RegeionName, r.RegionInfo, r.Cards);
                        thisRowLeft += size.Width;
                    }

                    rowTop += thisRowHight;
                }

                for (int i = 0; i < ownRowsCount; i++)
                {
                    if (!ownRows[i].Any())
                        continue;
                    var thisRowLeft = 0.0;
                    var thisRowHight = ownRows[i].Max(r => regionSizes[Tuple.Create(r.RegeionPlayer, r.RegeionName)].Height);
                    foreach (var rr in ownRows[i])
                    {
                        var size = regionSizes[Tuple.Create(rr.RegeionPlayer, rr.RegeionName)];
                        var rec = this.RegionsRect.FirstOrDefault(x => x.Player == rr.RegeionPlayer && rr.RegeionName == x.RegionName);
                        if (rec == null)
                        {
                            rec = new RegionViewmodel() { Player = rr.RegeionPlayer, RegionName = rr.RegeionName };
                            this.RegionsRect.Add(rec);
                        }
                        rec.Rect = new Rect(new Point(thisRowLeft, rowTop), size);

                        LayoutRegion(thisRowLeft, rowTop, size.Width, size.Height, rr.RegeionPlayer, rr.RegeionName, rr.RegionInfo, rr.Cards);
                        thisRowLeft += size.Width;
                    }

                    rowTop += thisRowHight;
                }




                if (Cards.Any())
                {
                    var minx = Cards.Min(x => x.X);
                    var maxx = Cards.Max(x => x.X + CardWidth);
                    var miny = Cards.Min(x => x.Y);
                    var maxy = Cards.Max(x => x.Y + CardHeight);
                    //this.CurrentWidth = Math.Max(0, maxx - this.Width) + Width;
                    //this.CurrentHeight = Math.Max(0, maxy - this.Height) + Height;
                    this.CurrentWidth = maxx;
                    this.CurrentHeight = maxy;
                    this.Margin = new Thickness(minx < 0 ? -minx : 0, miny < 0 ? -miny : 0, Math.Max(0, maxx - this.Width), Math.Max(0, maxy - this.Height));
                }
                else
                {
                    this.CurrentWidth = Width;
                    this.CurrentHeight = Height;
                    this.Margin = new Thickness();
                }
            }
        }

        private void LayoutRegion(double left, double top, double regionWidth, double regionHeight, PlayerNumber regeionPlayer, string regeionName, GameDataRegions regionInfo, IList<MentalCardGame.Card> cards)
        {
            int index = 0;
            switch (regionInfo.RegionType)
            {
                case RegionType.Stack:

                    var sFactor = Math.Min(STACFACTOR, (regionWidth - CardWidth) / cards.Count);
                    sFactor = Math.Min(sFactor, (regionHeight - CardHeight) / cards.Count);
                    sFactor = Math.Max(sFactor, 0);

                    var stackLeft = left + sFactor * cards.Count;
                    var stackTop = top + sFactor * cards.Count;

                    for (int i = 0; i < cards.Count; i++)
                    {
                        var c = cards[i];
                        var m = cardLookup[c];
                        m.FaceUp = c.Type.HasValue;
                        m.Angle = 0;
                        m.Z = index++;
                        m.X = stackLeft - i * STACFACTOR;
                        m.Y = stackTop - i * STACFACTOR;
                    }

                    break;
                case RegionType.Hand:

                    var w = cards.Count * CardWidth / 2;
                    var h = 1.5 * Math.Sqrt(CardWidth * CardWidth + CardHeight * CardHeight);
                    //var h = 100;// + Math.Sqrt(CardWidth * CardWidth + CardHeight * CardHeight);


                    var circleH = Math.Min(1, h / w);

                    var maxRotation = Math.Min(45, ToDegree(Math.Asin(circleH)));

                    if (cards.Count <= 4)
                    {
                        maxRotation = 12.5 * cards.Count / 2;
                    }

                    w *= 1 / (Math.Sin(ToRad(maxRotation)) - Math.Cos(ToRad(90))) / 2;
                    //maxRotation = 45;

                    for (int i = 0; i < cards.Count; i++)
                    {
                        var c = cards[i];
                        var mm = cardLookup[c];
                        mm.FaceUp = c.Type.HasValue;

                        mm.Angle = cards.Count <= 1 ? 0 : 0 - maxRotation + i * maxRotation * 2 / (cards.Count - 1);

                        var translate = (Math.Cos(ToRad(maxRotation)) - Math.Cos(ToRad(90))) * w;
                        mm.X = regionWidth / 2 + Math.Sin(ToRad(mm.Angle)) * w + left - CardWidth / 2;
                        mm.Y = top + regionHeight - Math.Cos(ToRad(mm.Angle)) * w + translate - Math.Sqrt(CardWidth * CardWidth + CardHeight * CardHeight);
                        mm.Z = index++;



                    }
                    break;
                case RegionType.Row:

                    var distens = 20;
                    for (int i = 0; i < cards.Count; i++)
                    {
                        var c = cards[i];
                        var mm = cardLookup[c];
                        mm.FaceUp = c.Type.HasValue;

                        mm.Angle = 0;

                        mm.X = left + i * (distens + CardWidth);
                        mm.Y = top;
                        mm.Z = index++;



                    }

                    //erg.Height = CardHeight;
                    //erg.Width = (CardWidth + distens) * cards.Count;
                    break;
                default:
                    throw new NotSupportedException();
            }


        }
        private Size CalculateDesiredRegionSize(PlayerNumber regeionPlayer, string regeionName, GameDataRegions regionInfo, IList<MentalCardGame.Card> cards)
        {

            var erg = new Size();

            switch (regionInfo.RegionType)
            {
                case RegionType.Stack:
                    erg.Width = CardWidth + cards.Count * STACFACTOR;
                    erg.Height = CardHeight + cards.Count * STACFACTOR;
                    break;
                case RegionType.Hand:

                    var w = cards.Count * CardWidth / 2 + Math.Sqrt(CardWidth * CardWidth + CardHeight * CardHeight);
                    var h = 1.5 * Math.Sqrt(CardWidth * CardWidth + CardHeight * CardHeight);

                    erg.Width = w;
                    erg.Height = h;

                    break;
                case RegionType.Row:
                    var distens = 20;
                    erg.Height = CardHeight;
                    erg.Width = (CardWidth + distens) * cards.Count;
                    break;
                default:
                    throw new NotSupportedException();
            }
            return erg;
        }

        private double ToRad(double angle)
        {
            return angle * Math.PI / 180;
        }
        private double ToDegree(double angle)
        {
            return angle / Math.PI * 180;
        }
    }
}