using System.Runtime.InteropServices.WindowsRuntime;
using Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Client.Common;
using Client.CardServerService;
using Windows.Security.Cryptography.Certificates;

namespace Client.Viewmodel
{
    public class ServerOverviewViewmodel : DependencyObject
    {

        public ObservableCollection<ServerEntry> Servers { get; } = new ObservableCollection<ServerEntry>();
        private readonly Common.RelayCommand addServerCommand;
        private readonly Common.RelayCommand removeServerCommand;

        public ICommand AddServerCommand { get { return addServerCommand; } }
        public ICommand RemoveServerCommand { get { return removeServerCommand; } }



        public ServerEntry SelectedServer
        {
            get { return (ServerEntry)GetValue(SelectedServerProperty); }
            set { SetValue(SelectedServerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedServer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedServerProperty =
            DependencyProperty.Register("SelectedServer", typeof(ServerEntry), typeof(ServerOverviewViewmodel), new PropertyMetadata(null, SelectedServerChanged));

        private static void SelectedServerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as ServerOverviewViewmodel;
            me.removeServerCommand.RaiseCanExecuteChanged();
        }

        public string NewServerString
        {
            get { return (string)GetValue(NewServerStringProperty); }
            set { SetValue(NewServerStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NewServerString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewServerStringProperty =
            DependencyProperty.Register("NewServerString", typeof(string), typeof(ServerOverviewViewmodel), new PropertyMetadata("", NewServerStringChanged));

        private static void NewServerStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as ServerOverviewViewmodel;
            me.addServerCommand.RaiseCanExecuteChanged();
        }

        public ServerOverviewViewmodel()
        {
            addServerCommand = new Common.RelayCommand(async () =>
            {
                if (this.NewServerString == null)
                    return;
                var entry = new ServerEntry();
                entry.IncreaseLoading();
                this.Servers.Add(entry);
                var id = await GetServerId(NewServerString);
                if (id != null)
                {
                    entry.IsOnline = true;
                    entry.ServerId = id;
                }
                else
                    this.Servers.Remove(entry);
                entry.DecreaseLoading();
            },
            () => Uri.IsWellFormedUriString(NewServerString, UriKind.Absolute));
            removeServerCommand = new Common.RelayCommand(() => Servers.Remove(SelectedServer), () => SelectedServer != null);

            Init();
        }

        private async void Init()
        {
            var ids = await DDR.GetServerIds();
            await Task.WhenAll(ids.Select(async i =>
            {
                var entry = new ServerEntry() { ServerId = i };
                entry.IncreaseLoading();
                Servers.Add(entry);
                var result = await GetServerId(entry.ServerUri);
                if (result != null)
                {
                    if (!i.Key.EqualsIPublicKeyData(result.Key))
                    {
                        entry.IsOnline = false;
                        Logger.Assert(false, $"Key des Servers hat sich geändert.  Fingerprint Remote: {result.Key.FingerPrint()} Fingerprint Local: {i.Key.FingerPrint()}");
                        var m = new Windows.UI.Popups.MessageDialog($"Der Key des Servers {i.Name} hat sich geändert. Sie müssen den Server neu hinzufügen falls Sie ihn in der liste weiter nutzen wollen.", "Public Key hat sich geändert");

                    }
                    else
                    {
                        Logger.Assert(result.Revision >= i.Revision, $"Der Server Liefert eine kleine Revison kleiner als die Lokal gespeicherte.  Fingerprint: {i.Key.FingerPrint()} Revision Local: {i.Revision} remote:{result.Revision}");

                        if (result.Revision != i.Revision)
                            entry.ServerId = result;
                        entry.IsOnline = true;
                    }
                }
                else
                    entry.IsOnline = false;
                entry.DecreaseLoading();
            }));
        }

        private static async Task<Client.Game.Data.ServerId> GetServerId(string remoteAddress)
        {
            try
            {
                var remoteWithEndingSlash = remoteAddress;
                if (!remoteWithEndingSlash.EndsWith("/", StringComparison.Ordinal))
                    remoteWithEndingSlash += "/";


                await GetServerSSLCertificate(remoteAddress);

                var config = remoteAddress.StartsWith("https") ? CardServerService.ServicePortClient.EndpointConfiguration.https : CardServerService.ServicePortClient.EndpointConfiguration.http;
                var ws = new CardServerService.ServicePortClient(config, remoteWithEndingSlash + "ws/");

                bla(ws);
                var idResponse = await ws.ServerIdentityAsync(new CardServerService.ServerIdentityRequest());
                var serverId = (Client.Game.Data.ServerId)idResponse.ServerIdentityResponse.identity;


                if (serverId.Uri != remoteAddress)
                    return await GetServerId(serverId.Uri);

                await global::Game.Database.Database.AddServerId(serverId);


                return serverId;
            }
            catch (System.ServiceModel.EndpointNotFoundException)
            {
                Logger.Information($"WS:Endpoint Not Found remoteAdress = { remoteAddress}");
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"remoteAdress= {remoteAddress}");
                return null;
            }
        }

        private static async Task GetServerSSLCertificate(string remoteAddress)
        {
            return;
            //Windows.Security.Cryptography.Certificates.Certificate cert = null;

            remoteAddress = remoteAddress.Replace("http://", "").Trim('/');
            remoteAddress = remoteAddress.Replace("https://", "").Trim('/');

            Windows.Networking.Sockets.StreamSocket s = new Windows.Networking.Sockets.StreamSocket();
            s.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
            //s.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);
            var hostName = new Windows.Networking.HostName(remoteAddress);
            var t = s.ConnectAsync(hostName, "443", Windows.Networking.Sockets.SocketProtectionLevel.Tls12);
            await t;
            if (s.Information.ServerCertificateErrors.Any(x => x == ChainValidationResult.Untrusted))
            {
                var cert = s.Information.ServerCertificate;
                bla(cert);
            }
        }

        private static void bla(Certificate cert)
        {
            var store = Windows.Security.Cryptography.Certificates.CertificateStores.GetStoreByName("TCG");
            store.Add(cert);
            Windows.Security.Cryptography.Certificates.CertificateStores.IntermediateCertificationAuthorities.Add(cert);
            Windows.Security.Cryptography.Certificates.CertificateStores.TrustedRootCertificationAuthorities.Add(cert);

        }

        private static void bla(ServicePortClient ws)
        {

        }

        public class ServerEntry : DependencyObject
        {
            public ServerEntry()
            {
                getBoosterCommand = new Common.RelayCommand(async () =>
                {
                    try
                    {
                        IncreaseLoading();
                        this.NewCards = null;
                        var result = await GetBoosterAsync();
                        this.NewCards = result as CardViewmodel[] ?? result.ToArray();
                    }
                    catch (Exception e)
                    {
                        Logger.LogException(e);
                    }
                    finally
                    {
                        DecreaseLoading();
                    }
                }, () => this.IsOnline.HasValue && this.IsOnline.Value && !IsLoading);

                updateRules = new RelayCommand(async () =>
                {

                    try
                    {
                        IncreaseLoading();
                        var ws = GetWebservice();
                        var rulesetsID = await ws.listRuleSetAsync(new CardServerService.listRuleSetRequest());
                        foreach (var rs in rulesetsID.listRuleSetResponse.uuids.Select(x => Guid.Parse(x)))
                        {
                            await DDR.GetRule(rs, this.ServerId.Key);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogException(e);
                    }
                    finally
                    {
                        DecreaseLoading();
                    }
                }, () => this.IsOnline.HasValue && this.IsOnline.Value && !IsLoading);

                updateCards = new RelayCommand(async () =>
                {

                    try
                    {
                        IncreaseLoading();
                        var ws = GetWebservice();
                        var instance = await DDR.GetCardInstances();

                        var data = instance.Where(x=>x!=null).Where(x => x.Creator.EqualsIPublicKeyData(this.ServerId.Key)).Select(x => x.CardDataId).Select(x =>
                        {
                            return DDR.ForceCardDataUpdate(new Client.Game.Data.UuidServer() { Uuid = x, Server = this.ServerId.Key });
                        });
                        await Task.WhenAll(data);

                    }
                    catch (Exception e)
                    {
                        Logger.LogException(e);
                    }
                    finally
                    {
                        DecreaseLoading();
                    }
                }, () => this.IsOnline.HasValue && this.IsOnline.Value && !IsLoading);

                this.tradeAndPlay = new RelayCommand(() => App.RootFrame.Navigate(typeof(Pages.NetworkLobby), this.ServerUri)
                , () => this.IsOnline.HasValue && this.IsOnline.Value && !IsLoading);

                this.resetBoosterCardsCommand = new RelayCommand(() => this.NewCards = null);
            }

            public bool IsLoading
            {
                get { return (bool)GetValue(IsLoadingProperty); }
                private set { SetValue(IsLoadingProperty, value); }
            }

            // Using a DependencyProperty as the backing store for IsLoading.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty IsLoadingProperty =
                DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(ServerEntry), new PropertyMetadata(false, LoadingChanged));

            private static void LoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                var me = d as ServerEntry;
                me.RaiseButtonChanged();
            }

            private int loadingCounter;
            public void IncreaseLoading()
            {
                loadingCounter++;
                IsLoading = true;
            }
            public void DecreaseLoading()
            {
                loadingCounter--;
                Logger.Assert(loadingCounter >= 0, $"Loading Count war {loadingCounter}");
                if (loadingCounter <= 0)
                    IsLoading = false;
            }

            public bool? IsOnline
            {
                get { return (bool?)GetValue(IsOnlineProperty); }
                set { SetValue(IsOnlineProperty, value); }
            }

            // Using a DependencyProperty as the backing store for IsOnline.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty IsOnlineProperty =
                DependencyProperty.Register(nameof(IsOnline), typeof(bool?), typeof(ServerEntry), new PropertyMetadata(null, IsOnlineChanged));

            private static void IsOnlineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                var me = d as ServerEntry;
                me.RaiseButtonChanged();
            }

            private void RaiseButtonChanged()
            {
                getBoosterCommand.RaiseCanExecuteChanged();
                updateRules.RaiseCanExecuteChanged();
                updateCards.RaiseCanExecuteChanged();
                tradeAndPlay.RaiseCanExecuteChanged();
            }

            public string FingerPrint
            {
                get { return (string)GetValue(FingerPrintProperty); }
                set { SetValue(FingerPrintProperty, value); }
            }

            // Using a DependencyProperty as the backing store for FingerPrint.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty FingerPrintProperty =
                DependencyProperty.Register("FingerPrint", typeof(string), typeof(ServerEntry), new PropertyMetadata(""));

            public byte[] Image
            {
                get { return (byte[])GetValue(ImageProperty); }
                set { SetValue(ImageProperty, value); }
            }

            // Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty ImageProperty =
                DependencyProperty.Register("Image", typeof(byte[]), typeof(ServerEntry), new PropertyMetadata(null));


            public Client.Game.Data.ServerId ServerId
            {
                get { return (Client.Game.Data.ServerId)GetValue(ServerIdProperty); }
                set { SetValue(ServerIdProperty, value); }
            }

            // Using a DependencyProperty as the backing store for ServerId.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty ServerIdProperty =
                DependencyProperty.Register("ServerId", typeof(Client.Game.Data.ServerId), typeof(ServerEntry), new PropertyMetadata(null, ServeridChanged));

            private static async void ServeridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                var me = d as ServerEntry;
                var newValue = e.NewValue as Client.Game.Data.ServerId;
                var oldValue = e.NewValue as Client.Game.Data.ServerId;
                if (newValue != null)
                {
                    me.FingerPrint = newValue.Key.FingerPrint();
                    me.ServerUri = newValue.Uri;
                    try
                    {
                        me.IncreaseLoading();
                        var imageData = await DDR.GetImageData(new Client.Game.Data.UuidServer() { Uuid = newValue.Icon, Server = newValue.Key });
                        me.Image = imageData?.Image ?? me.Image;
                        var trust = await DDR.GetServerTrust(newValue.Key);
                        me.IsTrustWorthy = trust.IsTrusted;
                        trust.PropertyChanged += me.Trust_PropertyChanged;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex);
                    }
                    finally
                    {
                        me.DecreaseLoading();
                    }

                }
                else
                {
                    me.FingerPrint = null;
                    me.Image = null;
                    me.ServerUri = null;
                    me.IsTrustWorthy = false;
                }
                if (oldValue != null)
                {
                    var trust = await DDR.GetServerTrust(oldValue.Key);
                    trust.PropertyChanged -= me.Trust_PropertyChanged;
                }
            }

            private void Trust_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                var trust = sender as DDR.Trust;
                IsTrustWorthy = trust.IsTrusted;
            }

            public String ServerUri
            {
                get { return (String)GetValue(ServerUriProperty); }
                private set { SetValue(ServerUriProperty, value); }
            }

            // Using a DependencyProperty as the backing store for ServerUri.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty ServerUriProperty =
                DependencyProperty.Register("ServerUri", typeof(String), typeof(ServerEntry), new PropertyMetadata(null));



            #region CardAcireing



            public ICollection<CardViewmodel> NewCards
            {
                get { return (ICollection<CardViewmodel>)GetValue(NewCardsProperty); }
                private set { SetValue(NewCardsProperty, value); }
            }

            // Using a DependencyProperty as the backing store for NewCards.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty NewCardsProperty =
                DependencyProperty.Register(nameof(NewCards), typeof(ICollection<CardViewmodel>), typeof(ServerEntry), new PropertyMetadata(null));
            private readonly RelayCommand getBoosterCommand;
            private readonly RelayCommand updateRules;
            private readonly RelayCommand updateCards;
            private readonly RelayCommand tradeAndPlay;
            private readonly RelayCommand resetBoosterCardsCommand;

            public ICommand GetBoosterCommand => getBoosterCommand;
            public ICommand UpdateRuleCommand => updateRules;
            public ICommand UpdateCardsCommand => updateCards;
            public ICommand TradeAndPlayCommand => tradeAndPlay;
            public ICommand ResetBoosterCardsCommand => resetBoosterCardsCommand;




            public bool IsTrustWorthy
            {
                get { return (bool)GetValue(IsTrustWorthyProperty); }
                set { SetValue(IsTrustWorthyProperty, value); }
            }

            // Using a DependencyProperty as the backing store for IsTrustWorthy.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty IsTrustWorthyProperty =
                DependencyProperty.Register("IsTrustWorthy", typeof(bool), typeof(ServerEntry), new PropertyMetadata(false, IsTrustWorthyChanged));

            private static async void IsTrustWorthyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                var me = d as ServerEntry;
                try
                {
                    me.IncreaseLoading();
                    await DDR.SetServerTrustworthy(me.ServerId.Key, (bool)e.NewValue);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
                finally
                {
                    me.DecreaseLoading();
                }
            }




            /// <summary>
            /// Vordert einen Booster an und fügt sie der Lokalen Datenbank hinzu.
            /// </summary>
            /// <returns>Gibt die neu erhaltenen Karten zurück</returns>
            public async Task<IEnumerable<CardViewmodel>> GetBoosterAsync()
            {
                try
                {
                    IncreaseLoading();

                    var ws = this.GetWebservice();


                    //var x = await ws.listCardDataAsync(new CardServerService.listCardDataRequest());

                    var response = await ws.createBoosterAsync(new CardServerService.createBoosterRequest() { ownerKey = Viewmodel.UserDataViewmodel.Instance.LoggedInUser.PublicKey.ToGameData() });
                    var my = Viewmodel.UserDataViewmodel.Instance.LoggedInUser.PublicKey.ToGameData();
                    Client.Game.Data.PublicKey aKey = response.createBoosterResponse.transactions.First().a;
                    Client.Game.Data.PublicKey bKey = response.createBoosterResponse.transactions.First().b;
                    var other = aKey.Equals(my) ? bKey : aKey;
                    var f1 = Convert.ToBase64String(my.Modulus);
                    Logger.TransactionInfo($"   My Modulus:{f1}");
                    var f2 = Convert.ToBase64String(other.Modulus);
                    Logger.TransactionInfo($"Other Modulus:{f2}");
                    await global::Game.TransactionMap.Graph.AddTransactions(response.createBoosterResponse.transactions.Cast<global::Game.TransactionMap.ServiceMerger.ITransaction>(), async b =>
                    {
                        var privateKey = (Viewmodel.UserDataViewmodel.Instance.LoggedInUser.PublicKey as IPrivateKey);
                        var sig = await privateKey.Sign(b);
                        Logger.Assert(privateKey.Veryfiy(b, sig), "Signatur ist leider nicht Gültig :(");
                        return sig;
                    }
                    );
                    await global::Game.TransactionMap.Graph.Merge(ws);

                    var result = response.createBoosterResponse.transactions.SelectMany(x => x.transfers.Select(y => new { y.cardId, y.creator }));
                    var cardViewmodels = await Task.WhenAll(result.Select(async x =>
                    {
                        var vm = new CardViewmodel();
                        await vm.LoadData(new Client.Game.Data.UuidServer() { Server = x.creator, Uuid = Guid.Parse(x.cardId) });
                        return vm;
                    }));

                    return cardViewmodels;

                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, $"remoteadress ={ServerUri}");
                    return null;
                }
                finally
                {
                    DecreaseLoading();
                }
            }

            #endregion

            public Client.CardServerService.ServicePortClient GetWebservice()
            {
                var remoteWithEndingSlash = this.ServerUri;
                if (!remoteWithEndingSlash.EndsWith("/", StringComparison.Ordinal))
                    remoteWithEndingSlash += "/";

                var config = remoteWithEndingSlash.StartsWith("https") ? CardServerService.ServicePortClient.EndpointConfiguration.https : CardServerService.ServicePortClient.EndpointConfiguration.http;
                var ws = new CardServerService.ServicePortClient(config, remoteWithEndingSlash + "ws/");
                return ws;
            }


        }
    }
}
