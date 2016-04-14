using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Client.Common;
using Windows.UI.Xaml;
using System.Windows.Input;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Viewmodel
{
    class DeckCollectionViewmodel : DependencyObject
    {



        public DeckViewmodel SelectedDeck
        {
            get { return (DeckViewmodel)GetValue(SelectedDeckProperty); }
            set { SetValue(SelectedDeckProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedDeck.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedDeckProperty =
            DependencyProperty.Register(nameof(SelectedDeck), typeof(DeckViewmodel), typeof(DeckCollectionViewmodel), new PropertyMetadata(null, SelectedDeckChanged));

        private static void SelectedDeckChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as DeckCollectionViewmodel;
            me.removeDeckCommand.RaiseCanExecuteChanged();
        }



        public ObservableCollection<DeckViewmodel> Decks
        {
            get { return (ObservableCollection<DeckViewmodel>)GetValue(DecksProperty); }
            set { SetValue(DecksProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Decks.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DecksProperty =
            DependencyProperty.Register(nameof(Decks), typeof(ObservableCollection<DeckViewmodel>), typeof(DeckCollectionViewmodel), new PropertyMetadata(null));



        public ICommand AddDeckCommand { get { return addDeckCommand; } }

        public ICommand RemoveDeckCommand { get { return removeDeckCommand; } }

        private readonly RelayCommand addDeckCommand;
        private readonly RelayCommand<DeckViewmodel> removeDeckCommand;


        public DeckCollectionViewmodel()
        {
            removeDeckCommand = new RelayCommand<DeckViewmodel>(RemoveDeck, deck => deck != null);
            addDeckCommand = new Common.RelayCommand(AddDeck, () => Decks != null);
            LoadData();

        }

        private async void LoadData()
        {

            var ids = await global::Game.Database.Database.GetDeckIds();
            var decks = ids.Select(x => new DeckViewmodel(x));

            Decks = new ObservableCollection<DeckViewmodel>();
            foreach (var d in decks)
                Decks.Add(d);

            if (Decks.Count > 0)
                SelectedDeck = Decks[0];

            addDeckCommand.RaiseCanExecuteChanged();
            Decks.CollectionChanged += Decks_CollectionChanged;
        }

        private async void Decks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (DeckViewmodel item in e.NewItems)
                        await global::Game.Database.Database.CreateOrRenameDeck(item.Id, item.Name);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (DeckViewmodel item in e.OldItems)
                        await global::Game.Database.Database.DeleteDeck(item.Id);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    foreach (DeckViewmodel item in e.OldItems)
                        await global::Game.Database.Database.DeleteDeck(item.Id);
                    foreach (DeckViewmodel item in e.NewItems)
                        await global::Game.Database.Database.CreateOrRenameDeck(item.Id, item.Name);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    foreach (DeckViewmodel item in e.OldItems)
                        await global::Game.Database.Database.DeleteDeck(item.Id);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private void RemoveDeck(DeckViewmodel deck)
        {
            this.Decks.Remove(deck);
            if (SelectedDeck == null)
            {
                if (Decks.Count > 0)
                    SelectedDeck = Decks[0];
                else
                    SelectedDeck = null;
            }
        }

        private void AddDeck()
        {
            var deckViewmodel = new DeckViewmodel();
            this.Decks.Add(deckViewmodel);
            this.SelectedDeck = deckViewmodel;
        }
    }

    class DeckViewmodel : DependencyObject
    {

        private int loadingCounter;
        public DeckViewmodel() : this(Guid.NewGuid()) { }
        public DeckViewmodel(Guid id)
        {
            this.Id = id;
            LoadData();
        }

        internal void IncreaseLoading()
        {
            this.loadingCounter++;
            Logger.Assert(loadingCounter < 0, $"Counter nicht größer 0 {loadingCounter}");
            if (loadingCounter > 0)
                this.IsLoading = true;
        }
        internal void DecreaseLoading()
        {
            this.loadingCounter--;
            Logger.Assert(loadingCounter >= 0, $"Counter kleiner 0 {loadingCounter}");
            if (loadingCounter <= 0)
                this.IsLoading = false;
        }

        private async void LoadData()
        {
            IncreaseLoading();
            this.Name = await global::Game.Database.Database.GetDeckName(this.Id);
            var instances = await global::Game.Database.Database.GetCardsOfDeck(this.Id);
            if (instances != null)
            {
                var viewmodels = await Task.WhenAll(instances.Select(async x =>
                {
                    var vm = new CardViewmodel();
                    await vm.LoadData(x);
                    return vm;
                }));
                this.Cards = new ObservableCollection<CardViewmodel>();
                foreach (var vm in viewmodels)
                    Cards.Add(vm);
            }
            else
                this.Cards = new ObservableCollection<CardViewmodel>();

            this.Cards.CollectionChanged += Cards_CollectionChanged;
            DecreaseLoading();
            this.lodingTask.SetResult(null);
        }

        private async void Cards_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!this.Dispatcher.HasThreadAccess)
                throw new Exception();
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (CardViewmodel item in e.NewItems)
                        await global::Game.Database.Database.AddCardToDeck(this.Id, item.CardInstance);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (CardViewmodel item in e.OldItems)
                        await global::Game.Database.Database.RemoveCardToDeck(this.Id, item.CardInstance);

                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    foreach (CardViewmodel item in e.OldItems)
                        await global::Game.Database.Database.RemoveCardToDeck(this.Id, item.CardInstance);
                    foreach (CardViewmodel item in e.NewItems)
                        await global::Game.Database.Database.AddCardToDeck(this.Id, item.CardInstance);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    foreach (CardViewmodel item in e.OldItems)
                        await global::Game.Database.Database.RemoveCardToDeck(this.Id, item.CardInstance);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        public String Name
        {
            get { return (String)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register(nameof(Name), typeof(String), typeof(DeckViewmodel), new PropertyMetadata("", NameChanged));

        private static async void NameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as DeckViewmodel;
            await global::Game.Database.Database.CreateOrRenameDeck(me.Id, (e.NewValue as string) ?? "");
        }

        internal Guid Id { get; }



        public ObservableCollection<CardViewmodel> Cards
        {
            get { return (ObservableCollection<CardViewmodel>)GetValue(CardsProperty); }
            set { SetValue(CardsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Cards.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CardsProperty =
            DependencyProperty.Register(nameof(Cards), typeof(ObservableCollection<CardViewmodel>), typeof(DeckViewmodel), new PropertyMetadata(null));


        private TaskCompletionSource<object> lodingTask = new TaskCompletionSource<object>();
        public Task LodingWaiter => lodingTask.Task;


        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadedProperty); }
            private set { SetValue(IsLoadedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsLoaded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLoadedProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(DeckViewmodel), new PropertyMetadata(true));



        public IEnumerable<String> Errors
        {
            get { return (IEnumerable<String>)GetValue(ErrorsProperty); }
            set { SetValue(ErrorsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Errors.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorsProperty =
            DependencyProperty.Register("Errors", typeof(IEnumerable<String>), typeof(DeckViewmodel), new PropertyMetadata(null));






    }
}
