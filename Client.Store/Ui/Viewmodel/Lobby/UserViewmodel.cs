using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Windows.UI.Xaml;

namespace Client.Store.Viewmodel.Lobby
{
    public class UserViewmodel : DependencyObject
    {
        // Using a DependencyProperty as the backing store for Connection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConnectionProperty =
            DependencyProperty.Register("Connection", typeof(Network.MultiConnection), typeof(UserViewmodel), new PropertyMetadata(null, ConnectionChanged));

        // Using a DependencyProperty as the backing store for IsConnected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsConnectedProperty =
            DependencyProperty.Register("IsConnected", typeof(bool), typeof(UserViewmodel), new PropertyMetadata(false, IsConnectedChanged));

        private readonly Common.RelayCommand initConnectionCommand;
        private readonly ObservableCollection<String> messages = new ObservableCollection<string>();
        private readonly Common.RelayCommand<string> postMassageCommand;
        private readonly Network.User user;
        private ServerViewmodel server;

        public UserViewmodel(Network.User user, ServerViewmodel server)
        {
            this.user = user;
            this.server = server;
            server.Server.ConnectionRecived += Server_ConnectionRecived;
            initConnectionCommand = new Common.RelayCommand(async () =>
            {
                var c = await this.server.Server.GetConnection(this.user);
                this.Server_ConnectionRecived(c);
            }, () => !this.IsConnected);
            postMassageCommand = new Common.RelayCommand<string>(async str =>
            {
                this.AddMessage(str);
                await this.Connection.Send(UTF8Encoding.UTF8.GetBytes(str));
            }, str => this.IsConnected);
        }

        public Network.IConnection Connection
        {
            get { return (Network.MultiConnection)GetValue(ConnectionProperty); }
            set { SetValue(ConnectionProperty, value); }
        }

        public Common.RelayCommand InitConnectionCommand
        {
            get { return initConnectionCommand; }
        }

        public bool IsConnected
        {
            get { return (bool)GetValue(IsConnectedProperty); }
            set { SetValue(IsConnectedProperty, value); }
        }

        public ObservableCollection<String> Messages
        {
            get { return messages; }
        }

        public Common.RelayCommand<string> PostMassageCommand
        {
            get { return postMassageCommand; }
        }

        public Network.User User
        {
            get { return user; }
        }

        private static void ConnectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as UserViewmodel;
            me.IsConnected = e.NewValue != null;
            var newConnection = e.NewValue as Network.MultiConnection;
            var oldConnection = e.OldValue as Network.MultiConnection;
            if (oldConnection != null)
                oldConnection.Recived -= me.MessageRecived;
            if (newConnection != null)
                newConnection.Recived += me.MessageRecived;
        }

        private static void IsConnectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as UserViewmodel;
            me.initConnectionCommand.RaiseCanExecuteChanged();
            me.postMassageCommand.RaiseCanExecuteChanged();
        }

        private async void AddMessage(string str)
        {
            if (!this.Dispatcher.HasThreadAccess)
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => AddMessage(str));
            else
                this.messages.Add(str);
        }

        private void MessageRecived(byte[] arg)
        {
            var str = UTF8Encoding.UTF8.GetString(arg, 0, arg.Length);
            this.AddMessage(str);
        }

        private async void Server_ConnectionRecived(Network.IUserConnection c)
        {
            if (!this.Dispatcher.HasThreadAccess)
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => Server_ConnectionRecived(c));
            else
            {
                this.Connection = c;

                var engin = new Game.Engine.GameConnection(c, CentralViewmodel.Instance.LogedInUser);

                App.Current.RootFrame.Navigate(typeof(GamePage), engin);
            }
        }
    }
}