using Client.Store.Game.Engine;
using Client.Store.Ui.Viewmodel.Game;
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

namespace Client.Store
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        private GameConnection connection;

        private GameEngine Engine { get { return connection.Engin; } }

        private Ui.Viewmodel.Game.GameViewmodel GameViewmodel { get { return this.DataContext as Ui.Viewmodel.Game.GameViewmodel; } }

        public GamePage()
        {
            this.InitializeComponent();

            //var binding = new Binding() {  Path=new PropertyPath("ActualWith")};
            //binding.Source = this;
            //binding.

            this.Loaded += PageLoaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            connection = e.Parameter as GameConnection;
            GameViewmodel.Engine = Engine;
            base.OnNavigatedTo(e);
        }

        private async void PageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await connection.Init();
                this.Frame.Navigate(typeof(Lobby));
            }
            catch (Exception ex)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debugger.Break();
        }

        private void grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.GameViewmodel.Width = e.NewSize.Width;
            this.GameViewmodel.Height = e.NewSize.Height;
            this.GameViewmodel.CardHeight = 146;
            this.GameViewmodel.CardWidth = 100;
        }

        private void CardControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        { 
            var c = sender as Ui.Controls.Game.CardControl;
            
            this.GameViewmodel.CardClicked(c.DataContext as Ui.Viewmodel.Game.CardViewmodel);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            GameViewmodel.PlayerActionClicked(e.ClickedItem as Game.Engine.PlayerMove.PlayerAction);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var m = (sender as FrameworkElement).DataContext as RegionViewmodel;
            this.GameViewmodel.RegionClicked(m);
            
        }
    }
}