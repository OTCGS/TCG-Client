using Client.Game.Engine;
using Client.Viewmodel.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Client.Viewmodel;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace Client.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        private GameConnectivity connection;

        TaskCompletionSource<Viewmodel.DeckViewmodel> deckInput = new TaskCompletionSource<Viewmodel.DeckViewmodel>();

        public GamePage()
        {
            this.InitializeComponent();


            this.Loaded += PageLoaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            connection = e.Parameter as GameConnectivity;
            this.game.Connection = connection;
            base.OnNavigatedTo(e);
        }

        private async void PageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var erg = await game.RunGame();
                string winningText;
                switch (erg)
                {
                    case Game.Data.PlayerNumber.None:
                        winningText = "Leider hat keiner gewonnen.";
                        break;
                    case Game.Data.PlayerNumber.Player1:
                        winningText = "Spieler eins hat gewonnen.";
                        break;
                    case Game.Data.PlayerNumber.Player2:
                        winningText = "Spieler zwei hat gewonnen.";
                        break;
                    case Game.Data.PlayerNumber.Any:
                        winningText = "Alle haben gewonnen.";
                        break;
                    default:
                        winningText = "Irgendwas merkwirdiges ist passiert.";
                        break;
                }
                await new Windows.UI.Popups.MessageDialog(winningText).ShowAsync();

            }

            catch (Exception ex)
            {
                await new Windows.UI.Popups.MessageDialog("Es ist ein Fehler aufgetreten.").ShowAsync();
                Logger.LogException(ex);
            }
            this.Frame.Navigate(typeof(HubPage));
        }

        private async Task<IEnumerable<Client.Game.Data.CardInstance>> game_SelectDeckEvent()
        {
            // TODO: Nur Legale Des erlauben.
            var vm = await deckInput.Task;
            if (vm == null)
                return null;
            DeckSelectionView.Visibility = Visibility.Collapsed;
            return vm.Cards.Select(x => x.CardInstance);
        }

        private void SelectDeck_Click(object sender, RoutedEventArgs e)
        {
            var selected = (Viewmodel.DeckViewmodel)DeckList.SelectedItem;
            Logger.Assert(selected != null, "ausgewähltes Deck darf nicht null sein.");
            deckInput.SetResult(selected);
        }

        private void AbortDeckSelection(object sender, RoutedEventArgs e)
        {
            deckInput.TrySetResult(null);
        }

        private void DeckList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeckSelectButton.IsEnabled = DeckList.SelectedItem != null;
        }

        private async Task<IEnumerable<string>> ListEnabledStyleSelector_Filter(Viewmodel.DeckViewmodel obj)
        {
            var vm = obj as Viewmodel.DeckViewmodel;
            Logger.Information("Before Script Initialisation Check.");
            await connection.Engin.InitScriptEngeinWaiter;
            Logger.Information("After Script Initialisation Check.");
            var errors = connection.Engin.GameData.IsDeckLeagal(vm.Cards.Select(x => x.CardData));
            Logger.Information("After Deck is Legal errors returned.");
            return errors;
        }


    }
}