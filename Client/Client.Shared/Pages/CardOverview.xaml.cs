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
using Security;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Client.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CardOverview : Page
    {
        public CardOverview()
        {
            this.InitializeComponent();

            var d2 = grid.DataContext as Viewmodel.CardCollectionViewmodel;
            d2.User = Viewmodel.UserDataViewmodel.Instance.LoggedInUser;
            //     this.DataContext = new Viewmodel.CardCollectionViewmodel() { User = Viewmodel.UserDataViewmodel.Instance.LoggedInUser };

            var hack_container = Windows.Storage.ApplicationData.Current.LocalSettings.CreateContainer("HACK", Windows.Storage.ApplicationDataCreateDisposition.Always);
            var ip = hack_container.Values["ip"] as string;
            if (ip != null)
                this.boosterUrl.Text = ip;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                getBosterButton.IsEnabled = false;

                //var ws = new CardServerService.ServicePortClient();
                // ws = new CardServerService.ServicePortClient(CardServerService.ServicePortClient.EndpointConfiguration.ServicePortSoap11, "");

                //var ws = await Viewmodel.UserDataViewmodel.GetServerWebService("http://143.93.53.57:8080");
                //var ws = await Viewmodel.UserDataViewmodel.GetServerWebService("http://localhost:8080");                


                var hack_container = Windows.Storage.ApplicationData.Current.LocalSettings.CreateContainer("HACK", Windows.Storage.ApplicationDataCreateDisposition.Always);
                hack_container.Values["ip"] = this.boosterUrl.Text;


                var ws = await Viewmodel.UserDataViewmodel.GetServerWebService(this.boosterUrl.Text);

                //var x = await ws.listCardDataAsync(new CardServerService.listCardDataRequest());

                var response = await ws.createBoosterAsync(new CardServerService.createBoosterRequest() { ownerKey = Viewmodel.UserDataViewmodel.Instance.LoggedInUser.PublicKey.ToGameData() });
                var other = response.createBoosterResponse.transactions.First().b;
                var my = Viewmodel.UserDataViewmodel.Instance.LoggedInUser.PublicKey.ToGameData();
                var f1 = Convert.ToBase64String(my.Modulus);
                var f2 = Convert.ToBase64String(other.modulus);
                await global::Game.TransactionMap.Graph.AddTransactions(response.createBoosterResponse.transactions.Cast<global::Game.TransactionMap.ServiceMerger.ITransaction>(), async b =>
                {
                    var privateKey = (Viewmodel.UserDataViewmodel.Instance.LoggedInUser.PublicKey as IPrivateKey);
                    var sig = await privateKey.Sign(b);
                    Logger.Assert(privateKey.Veryfiy(b, sig),"Erstellte signatur ist nicht gültig.");
                    return sig;
                }
                );
                await global::Game.TransactionMap.Graph.Merge(ws);

                var d2 = grid.DataContext as Viewmodel.CardCollectionViewmodel;
                await d2.Reload();


            }
            finally
            {
                getBosterButton.IsEnabled = true;
            }


        }
    }
}
