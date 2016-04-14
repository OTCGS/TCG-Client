using Client.Common;
using Security;
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

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace Client.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class NetworkLobby : Page
    {



        /// <summary>
        /// NavigationHelper wird auf jeder Seite zur Unterstützung bei der Navigation verwendet und 
        /// Verwaltung der Prozesslebensdauer
        /// </summary>
        public NavigationHelper NavigationHelper { get; }


        public Viewmodel.BaseNetworkViewmodel Model { get { return this.DataContext as Viewmodel.BaseNetworkViewmodel; } }

        public NetworkLobby()
        {
            this.NavigationHelper = new NavigationHelper(this);
            this.InitializeComponent();
            this.NavigationHelper.LoadState += navigationHelper_LoadState;
            this.NavigationHelper.SaveState += navigationHelper_SaveState;
        }



        /// <summary>
        /// Füllt die Seite mit Inhalt auf, der bei der Navigation übergeben wird. Gespeicherte Zustände werden ebenfalls
        /// bereitgestellt, wenn eine Seite aus einer vorherigen Sitzung neu erstellt wird.
        /// </summary>
        /// <param name="sender">
        /// Die Quelle des Ereignisses, normalerweise <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Ereignisdaten, die die Navigationsparameter bereitstellen, die an
        /// <see cref="Frame.Navigate(Type, Object)"/> als diese Seite ursprünglich angefordert wurde und
        /// ein Wörterbuch des Zustands, der von dieser Seite während einer früheren
        /// beibehalten wurde. Der Zustand ist beim ersten Aufrufen einer Seite NULL.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Behält den dieser Seite zugeordneten Zustand bei, wenn die Anwendung angehalten oder
        /// die Seite im Navigationscache verworfen wird.  Die Werte müssen den Serialisierungsanforderungen
        /// von <see cref="SuspensionManager.SessionState"/> entsprechen.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses, normalerweise <see cref="NavigationHelper"/></param>
        /// <param name="e">Ereignisdaten, die ein leeres Wörterbuch zum Auffüllen bereitstellen
        /// serialisierbarer Zustand.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper-Registrierung

        /// Die in diesem Abschnitt bereitgestellten Methoden werden einfach verwendet, um
        /// damit NavigationHelper auf die Navigationsmethoden der Seite reagieren kann.
        /// 
        /// Platzieren Sie seitenspezifische Logik in Ereignishandlern für  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// und <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// Der Navigationsparameter ist in der LoadState-Methode verfügbar 
        /// zusätzlich zum Seitenzustand, der während einer früheren Sitzung beibehalten wurde.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigationHelper.OnNavigatedTo(e);
            var uri = e.Parameter as string;
            if (uri != null)
            {
                var vm = new Viewmodel.ServerNetworkViewmodel(uri);
                this.DataContext = vm;
                vm.AcceptedGameConnection += PlayConnectionRecived;
                vm.AcceptedTradeConnection += LocalNetworkViewmodel_AcceptedTradeConnection;
            }
            else
            {
                var vm = new Viewmodel.LocalNetworkViewmodel();
                this.DataContext = vm;
                vm.AcceptedGameConnection += PlayConnectionRecived;
                vm.AcceptedTradeConnection += LocalNetworkViewmodel_AcceptedTradeConnection;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            NavigationHelper.OnNavigatedFrom(e);
            Model.Dispose();

        }


        #endregion


        private void MessageTextboxKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                Model.SendMessageCommand.Execute(textBox.Text);
                textBox.Text = "";


            }
        }

        private async void PlayConnectionRecived(Network.IUserConnection c)
        {

            var rule = await DDR.GetRule(c.DataId, c.DataKey.ToGameData());
            var engin = new Game.Engine.GameConnectivity(c, Viewmodel.UserDataViewmodel.Instance.LoggedInUser, rule);
            Frame.Navigate(typeof(GamePage), engin);
        }

        private void userList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.BottomAppBar.IsOpen = userList.SelectedItem != null;
        }

        private async void LocalNetworkViewmodel_AcceptedTradeConnection(Network.IUserConnection c)
        {
            try
            {
                this.IsEnabled = false;
                var mergConnection = new Trade.MergeConnectivity(c);
                await mergConnection.Merge();

                var tradeConnection = new Trade.TradeConnectivity(c);
                Frame.Navigate(typeof(TradePage), tradeConnection);


            }
            finally
            {
                this.IsEnabled = true;
            }
        }
    }
}
