using Client.Game.Engine;
using Client.Viewmodel.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Benutzersteuerelement" ist unter http://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace Client.Controls.Game
{
    public sealed partial class GameControl : UserControl
    {


        private GameEngine Engine { get { return Connection.Engin; } }

        private Viewmodel.Game.GameViewmodel GameViewmodel { get { return this.DataContext as Viewmodel.Game.GameViewmodel; } }




        public GameConnectivity Connection
        {
            get { return (GameConnectivity)GetValue(ConnectionProperty); }
            set { SetValue(ConnectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Connection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConnectionProperty =
            DependencyProperty.Register(nameof(Connection), typeof(GameConnectivity), typeof(GameControl), new PropertyMetadata(null, ConnectionChanged));

        private static void ConnectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as GameControl;
            var oldvalue = e.OldValue as GameConnectivity;
            if (oldvalue?.Engin != null)
                oldvalue.Engin.SelectDeckEvent -= me.Engine_SelectDeckEvent;
            if (e.NewValue != null)
                me.Engine.SelectDeckEvent += me.Engine_SelectDeckEvent;
        }

        private Task<IEnumerable<Client.Game.Data.CardInstance>> Engine_SelectDeckEvent()
        {
            return SelectDeckEvent();
        }

        public event Func<Task<IEnumerable<Client.Game.Data.CardInstance>>> SelectDeckEvent;


        public GameControl()
        {
            this.InitializeComponent();
        }


        public Task<Client.Game.Data.PlayerNumber> RunGame()
        {
            if (Connection == null)
            {
                throw new InvalidOperationException("Connection Property nicht gesetzt.");
            }
            try
            {
                GameViewmodel.Engine = Engine;
                return Connection.Init();
            }
            catch (Exception ex)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
                Logger.LogException(ex);
                return Task.FromResult(Client.Game.Data.PlayerNumber.None);
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
            var c = sender as Controls.Game.CardControl;

            this.GameViewmodel.CardClicked(c.DataContext as Viewmodel.Game.CardViewmodel);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            GameViewmodel.PlayerActionClicked(e.ClickedItem as Client.Game.Engine.PlayerMove.PlayerAction);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var m = (sender as FrameworkElement).DataContext as RegionViewmodel;
            this.GameViewmodel.RegionClicked(m);

        }
    }
}
