using Client.Store.Viewmodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
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
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var r = new Random();
            var cert = await Securety.PrivateCertificate.CreatePrivateCertificate();
            Viewmodel.CentralViewmodel.Instance.LogedInUser = new Network.User() { Name = "Paul" + r.Next(99), Certificate = cert };
        }

        private void Lobby_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Lobby));
        }

        private void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(CreateAccount));
        }

        private async void OfflineTest_Click(object sender, RoutedEventArgs e)

        {
            var currentIP = CurrentIPAddress();

            var t = await Network.OfflineTestServer.GetConnection(CentralViewmodel.Instance.LogedInUser, currentIP);
            Network.MultiConnection c1 = t.Item1;
            Network.MultiConnection c2 = t.Item2;
            var connection = new Game.Engine.GameConnection(c2, CentralViewmodel.Instance.LogedInUser);
            var testConnection = new Game.Engine.GameConnection(c1, c2.User);

            App.Current.RootFrame.Navigate(typeof(GamePage), connection);

            testConnection.Engin.WaitForPlayerMove += x => { return Task.FromResult<Game.Engine.PlayerMove.AbstractAction>(x.First()); };

            try
            {
                await testConnection.Init();
            }
            catch (Exception)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
                throw;
            }
        }

        private string CurrentIPAddress()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp != null && icp.NetworkAdapter != null)
            {
                var hostname =
                    NetworkInformation.GetHostNames()
                        .SingleOrDefault(
                            hn =>
                            hn.IPInformation != null && hn.IPInformation.NetworkAdapter != null
                            && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                            == icp.NetworkAdapter.NetworkAdapterId);

                if (hostname != null)
                {
                    // the ip address
                    return hostname.CanonicalName;
                }
            }

            return string.Empty;
        }
    }
}