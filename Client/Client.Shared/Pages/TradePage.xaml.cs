using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Client.Trade;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Client.Common;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Client.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TradePage : Page
    {

        private Viewmodel.TradeViewModel Model { get { return DataContext as Viewmodel.TradeViewModel; } }

        public NavigationHelper NavigationHelper { get; }

        /// <summary>
        /// wird gesetzt falls zurück gedrückt wird damit wir nicht mehr auf das session beendet event reagieren.
        /// </summary>
        bool endedConnection = false;

        public TradePage()
        {
            this.NavigationHelper = new NavigationHelper(this);
            this.InitializeComponent();
            this.NavigationHelper.LoadState += navigationHelper_LoadState;
            this.NavigationHelper.SaveState += navigationHelper_SaveState;
            Model.SessionEnded += Model_SessionEnded;
        }

        private void Model_SessionEnded()
        {
            if (!endedConnection)
                NavigationHelper.GoBack();
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {

        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationHelper.OnNavigatedTo(e);
            var connection = e.Parameter as Trade.TradeConnectivity;
            this.Model.TradeConnection = connection;
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            endedConnection = true;
            base.OnNavigatedFrom(e);
            NavigationHelper.OnNavigatedFrom(e);
            await Model.QuitConnection();
            Model.Dispose();
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var newCollection = (sender as GridView).SelectedItems.Cast<Viewmodel.CardViewmodel>();
            Model.SelectedCardsToOffer.UpdateCollection(newCollection);
        }

        private void MessageTextboxKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                Model.SendMessageCommand.Execute(textBox.Text);
                textBox.Text = "";
            }
        }
    }
}
