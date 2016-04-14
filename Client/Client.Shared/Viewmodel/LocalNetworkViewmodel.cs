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

namespace Client.Viewmodel
{
    public class LocalNetworkViewmodel : BaseNetworkViewmodel, IDisposable
    {
        private GameServer server;

        protected override GameServer Server
        {
            get
            {
                if (server == null)
                    server = Network.GameServer.GetLocalPeers(UserDataViewmodel.Instance.LoggedInUser);
                return server;
            }
        }

     










    }
}
