using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace Client.Store.Viewmodel.Lobby
{
    public class LobbyViewModel : DependencyObject
    {
        private readonly ObservableCollection<ServerViewmodel> servers = new ObservableCollection<ServerViewmodel>();

        public ObservableCollection<ServerViewmodel> Servers
        {
            get { return servers; }
        }

        private readonly ICommand addServerCommand;

        public ICommand AddServerCommand
        {
            get { return addServerCommand; }
        }

        public LobbyViewModel()
        {
            addServerCommand = new Common.RelayCommand<string>(AddServer);
            AddLocalPeers();
        }

        private async void AddLocalPeers()
        {
            var server =  Network.GameServer.GetLocalPeers(Viewmodel.CentralViewmodel.Instance.LogedInUser);
            this.servers.Add(new ServerViewmodel(server));
        }

        private async void AddServer(string address)
        {
            var server = await Network.GameServer.GetServer(address, Viewmodel.CentralViewmodel.Instance.LogedInUser);
            this.servers.Add(new ServerViewmodel(server));
        }
    }
}