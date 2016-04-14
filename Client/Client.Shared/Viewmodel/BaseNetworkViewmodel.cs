using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Network;
using Windows.UI.Xaml;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Client.Common;
using System.Linq;
using System.Collections.Specialized;
using Client.CardServerService;
using Security;
using Client.Game.Data;
using Security;

namespace Client.Viewmodel
{

    public class MessageViewmodel
    {
        public User User { get; set; }
        public String Text { get; set; }
    }


    public abstract class BaseNetworkViewmodel : DependencyObject, IDisposable
    {
        protected abstract GameServer Server { get; }

        public ObservableCollection<MessageViewmodel> Messages { get; } = new ObservableCollection<MessageViewmodel>();
        public ObservableCollection<Client.Game.Data.Ruleset> Rulesets { get; } = new ObservableCollection<Client.Game.Data.Ruleset>();

        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();




        public Client.Game.Data.Ruleset SelectedRuleset
        {
            get { return (Client.Game.Data.Ruleset)GetValue(SelectedRulesetProperty); }
            set { SetValue(SelectedRulesetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedRuleset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedRulesetProperty =
            DependencyProperty.Register(nameof(SelectedRuleset), typeof(Client.Game.Data.Ruleset), typeof(BaseNetworkViewmodel), new PropertyMetadata(null, RulesetChanged));

        private static void RulesetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as BaseNetworkViewmodel;
            me.PlayCommand.RaiseCanExecuteChanged();
        }

        public User SelectedUser
        {
            get { return (User)GetValue(SelectedUserProperty); }
            set { SetValue(SelectedUserProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedUser.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedUserProperty =
            DependencyProperty.Register(nameof(SelectedUser), typeof(User), typeof(BaseNetworkViewmodel), new PropertyMetadata(null, SelecteduserChanged));

        private static void SelecteduserChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as BaseNetworkViewmodel;
            me.PlayCommand.RaiseCanExecuteChanged();
            me.TradeCommand.RaiseCanExecuteChanged();
        }

        public event Action<IUserConnection> AcceptedGameConnection;
        public event Action<IUserConnection> AcceptedTradeConnection;

        public BaseNetworkViewmodel()
        {

            (this.Server.Users as INotifyCollectionChanged).CollectionChanged += Users_CollectionChanged;
            this.Server.TextMessageRecived += Server_TextMessageRecived;
            Server.ConnectionRecived += Server_ConnectionRecived;

            TradeCommand = new RelayCommand(async () =>
            {
                var connection = await this.Server.GetConnection(SelectedUser, ConnectionReason.Trade, Guid.Empty, null);
                if (connection == null)
                {
                    await new Windows.UI.Popups.MessageDialog("Verbindung verweigert.").ShowAsync();
                }
                else if (AcceptedTradeConnection != null)
                {
                    AcceptedTradeConnection(connection);
                }
            },
             () => SelectedUser != null
            );

            PlayCommand = new RelayCommand(async () =>
            {
                var dataKey = Security.SecurityFactory.CreatePublicKey();
                if (SelectedRuleset != null)
                {
                    dataKey.SetKey(SelectedRuleset.Creator.Modulus, SelectedRuleset.Creator.Exponent);
                }
                else
                    dataKey = null;

                var connection = await this.Server.GetConnection(SelectedUser, ConnectionReason.Play, SelectedRuleset?.Id ?? Guid.Empty, dataKey);
                if (connection == null)
                {
                    await new Windows.UI.Popups.MessageDialog("Verbindung verweigert.").ShowAsync();
                }
                else if (AcceptedGameConnection != null)
                {
                    AcceptedGameConnection(connection);
                }
            },
             () => SelectedUser != null && SelectedRuleset != null
             );



            Init();

        }



        protected async virtual void Init()
        {
            var rulesets = await DDR.GetRulesets();
            foreach (var r in rulesets)
                this.Rulesets.Add(r);
        }

        private async Task<bool> Server_ConnectionRecived(IUserConnection arg)
        {
            if (this.Dispatcher.HasThreadAccess)
            {
                var title = string.Format("Verbindungs versuch ({1}) von {0}", arg.User.Name, arg.ConnectionReason);
                var content = string.Format("{0} will eine Verbindung mit ihnen aufbauen. Möchten Sie diese annehmen?", arg.User.Name);
                var dialog = new Windows.UI.Popups.MessageDialog(content, title);
                var yes = new Windows.UI.Popups.UICommand("Annehmen");
                var no = new Windows.UI.Popups.UICommand("Verweigern");
                dialog.Commands.Add(yes);
                dialog.Commands.Add(no);
                dialog.CancelCommandIndex = 1;
                dialog.DefaultCommandIndex = 1;
                dialog.Options = Windows.UI.Popups.MessageDialogOptions.AcceptUserInputAfterDelay;
                var answer = await dialog.ShowAsync();
                var accept = answer == yes;
                switch (arg.ConnectionReason)
                {
                    case ConnectionReason.Play:
                        if (accept && AcceptedGameConnection != null)
                            this.AcceptedGameConnection(arg);
                        break;
                    case ConnectionReason.Trade:
                        if (accept && AcceptedTradeConnection != null)
                            this.AcceptedTradeConnection(arg);
                        break;
                    default:
                        break;
                }

                return accept;
            }
            else
            {
                var waiter = new TaskCompletionSource<Task<bool>>();
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var erg = Server_ConnectionRecived(arg);

                    waiter.SetResult(erg);
                });
                return await await waiter.Task;
            }
        }

        private async void Server_TextMessageRecived(User from, string msg)
        {

            if (!this.Dispatcher.HasThreadAccess)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => Server_TextMessageRecived(from, msg));
                return;
            }

            this.Messages.Insert(0, new MessageViewmodel() { User = from, Text = msg });
        }

        private async void Users_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!this.Dispatcher.HasThreadAccess)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => Users_CollectionChanged(sender, e));
                return;
            }
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                        foreach (Network.User item in e.NewItems)
                            this.Users.Add(item);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {

                        foreach (Network.User item in e.OldItems)
                            this.Users.Remove(item);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    this.Users.Clear();
                    break;

                default:
                    break;
            }
        }




        public String MessageToSend
        {
            get { return (String)GetValue(MessageToSendProperty); }
            set { SetValue(MessageToSendProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MessageToSend.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageToSendProperty =
            DependencyProperty.Register("MessageToSend", typeof(String), typeof(BaseNetworkViewmodel), new PropertyMetadata(""));

        public ICommand SendMessageCommand
        {
            get
            {
                return new RelayCommand<String>((msg) =>
                {
                    this.Messages.Insert(0, new MessageViewmodel() { User = UserDataViewmodel.Instance.LoggedInUser, Text = msg });
                    Server.SendTextMessage(msg);
                    MessageToSend = "";
                });
            }
        }

        public RelayCommand PlayCommand { get; }
        public RelayCommand TradeCommand { get; }

        protected bool DisposedValue
        {
            get { return disposedValue; }

        }

        #region IDisposable Support
        private bool disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.

        protected virtual void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                if (disposing)
                {
                    this.Server.Dispose();
                }

                disposedValue = true;
            }
        }


        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(true);
        }
        #endregion



    }
}