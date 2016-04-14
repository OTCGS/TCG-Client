using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Client.Store.Viewmodel.Lobby
{
    public class ServerViewmodel : DependencyObject
    {
        private readonly string name;
        private readonly Network.GameServer server;
        private readonly ObservableCollection<UserViewmodel> users = new ObservableCollection<UserViewmodel>();

        public ObservableCollection<UserViewmodel> Users
        {
            get { return users; }
        }

        public Network.GameServer Server
        {
            get { return server; }
        }

        public string Name
        {
            get { return name; }
        }

        public ServerViewmodel(Network.GameServer server)
        {
            this.server = server;
            this.name = server.Name;

            server.Users.CollectionChanged += Users_CollectionChanged;
            foreach (var item in server.Users)
                this.users.Add(new UserViewmodel(item, this));
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
                            this.users.Add(new UserViewmodel(item, this));
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        var toDelete = e.OldItems.Cast<Network.User>().Select(x => users.First(y => y.User == x)).ToArray();
                        foreach (var item in toDelete)
                            this.users.Remove(item);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    this.users.Clear();
                    break;

                default:
                    break;
            }
        }
    }
}