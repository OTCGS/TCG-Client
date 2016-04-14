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
using Security.Interfaces;

namespace Client.Viewmodel
{
    public class ServerNetworkViewmodel : BaseNetworkViewmodel
    {

        private GameServer server;
        private readonly WebserviceService.LayzyWraper service = WebserviceService.GetService();

        protected override GameServer Server
        {
            get
            {
                if (server == null)
                    server = new Network.ServerServer(UserDataViewmodel.Instance.LoggedInUser, this.service);
                return server;
            }
        }


        public ServerNetworkViewmodel(string url)
        {
            service.SetUrl(url);
        }
        protected override void Init()
        {

            base.Init();
        }

        #region IService


        protected override void Dispose(bool disposing)
        {
            if (disposing && !DisposedValue)
                server.Dispose();
            base.Dispose(disposing);
        }
        private sealed class WebserviceService : Network.ServerServer.IService
        {
            private TaskCompletionSource<ServicePortClient> ws = new TaskCompletionSource<ServicePortClient>();
            private readonly string url;
            private int usageCounter = 1;


            private WebserviceService(string url)
            {
                this.url = url;
                this.Name = url;
                PollLoop();

            }



            public static LayzyWraper GetService()
            {
                return new LayzyWraper();
            }


            public class LayzyWraper : ServerServer.IService
            {
                private static Dictionary<string, WebserviceService> lookup = new Dictionary<string, WebserviceService>();
                private TaskCompletionSource<ServerServer.IService> service = new TaskCompletionSource<ServerServer.IService>();



                public void SetUrl(string url)
                {
                    if (lookup.ContainsKey(url))
                    {
                        var layzyWraper = lookup[url];
                        InitWraper(layzyWraper);
                    }
                    else
                    {
                        var layzyWraper = new WebserviceService(url);
                        lookup[url] = layzyWraper;
                        InitWraper(layzyWraper);
                    }
                }

                private void InitWraper(ServerServer.IService layzyWraper)
                {
                    Name = layzyWraper.Name;
                    service.SetResult(layzyWraper);
                    layzyWraper.UsersOnServer.CollectionChanged += UsersOnServer_CollectionChanged;
                    foreach (var item in layzyWraper.UsersOnServer)
                        this.UsersOnServer.Add(item);
                }

                private void UsersOnServer_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            if (e.NewItems != null)
                                foreach (IPublicKeyData item in e.NewItems)
                                    this.UsersOnServer.Add(item);
                            break;
                        case NotifyCollectionChangedAction.Move:
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            if (e.OldItems != null)
                                foreach (IPublicKeyData item in e.OldItems)
                                    this.UsersOnServer.Remove(item);
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            if (e.NewItems != null)
                                foreach (IPublicKeyData item in e.NewItems)
                                    this.UsersOnServer.Add(item);
                            if (e.OldItems != null)
                                foreach (IPublicKeyData item in e.OldItems)
                                    this.UsersOnServer.Remove(item);
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            UsersOnServer.Clear();
                            break;
                        default:
                            break;
                    }


                }

                public string Name { get; private set; }

                public ObservableCollection<IPublicKeyData> UsersOnServer { get; } = new ObservableCollection<IPublicKeyData>();

                public event Action<ServerServer.Message<byte[]>> DataMessageRecived
                {
                    add
                    {
                        AddEvent(value);
                    }
                    remove
                    {
                        RemoveEvent(value);
                    }
                }

                private async void AddEvent(Action<ServerServer.Message<byte[]>> value)
                {
                    (await service.Task).DataMessageRecived += value;
                }
                private async void RemoveEvent(Action<ServerServer.Message<byte[]>> value)
                {
                    (await service.Task).DataMessageRecived -= value;
                }

                public async void Dispose()
                {
                    (await service.Task).Dispose();
                }

                public async void IncreaseUse()
                {
                    (await service.Task).IncreaseUse();
                }

                public async Task SendDataBroadcastMessage(byte[] msg)
                {
                    await (await service.Task).SendDataBroadcastMessage(msg);
                }

                public async Task SendDataMessage(byte[] msg, IPublicKeyData To)
                {
                    await (await service.Task).SendDataMessage(msg, To);
                }
            }



            private event Action<ServerServer.Message<byte[]>> DataMessageRecived;
            event Action<ServerServer.Message<byte[]>> ServerServer.IService.DataMessageRecived
            {
                add
                {
                    DataMessageRecived += value;
                }

                remove
                {
                    DataMessageRecived -= value;
                }
            }

            private async void PollLoop()
            {
                this.ws.SetResult(await UserDataViewmodel.GetServerWebService(url));
                var ws = await this.ws.Task;
                var registerResponse = await ws.registerAsync(new registerRequest() { user = UserDataViewmodel.Instance.LoggedInUser.PublicKey.ToGameData() });

                var keys = registerResponse.registerResponse1;
                foreach (var item in keys)
                {
                    this.usersFromWebservice.Add((PublicKey)item);
                }


                while (true)
                {
                    await Stoped.Barrier;
                    try
                    {
                        var publicKey = UserDataViewmodel.Instance.LoggedInUser.PublicKey.ToGameData();
                        var response = await Task.Run(() => ws.getRelayMessageAsync(new getRelayMessageRequest() { @for = publicKey }));
                        foreach (var msg in response.getRelayMessageResponse1)
                        {
                            if (msg.from.EqualsIPublicKeyData(publicKey))
                                continue;
                            if (this.DataMessageRecived != null)
                                this.DataMessageRecived(new ServerServer.Message<byte[]>() { From = msg.from, Data = msg.data });
                        }
                        await Task.Delay(200);
                    }
                    catch (Exception e)
                    {
                        Logger.LogException(e);
                    }
                }
            }



            async Task ServerServer.IService.SendDataMessage(byte[] msg, Security.Interfaces.IPublicKeyData To)
            {
                TaskCompletionSource<PublicKey> key = new TaskCompletionSource<PublicKey>();

                try
                {
                    var dispatcher = App.Dispatcher;
                    await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        var pk = UserDataViewmodel.Instance.LoggedInUser.PublicKey.ToGameData();
                        key.SetResult(pk);
                    });
                }
                catch (Exception)
                {

                    throw;
                }
                var publicKey = await key.Task;
                await (await (ws.Task)).relayMessageAsync(new relayMessageRequest() { relayMessage = new relayMessage() { from = publicKey, to = To.ToGameData(), data = msg } });
            }

            async Task ServerServer.IService.SendDataBroadcastMessage(byte[] msg)
            {
                await (await ws.Task).broadcastMessageAsync(new broadcastMessageRequest() { relayMessage = new relayMessage() { data = msg, from = UserDataViewmodel.Instance.LoggedInUser.PublicKey.ToGameData() } });
            }

            public void IncreaseUse()
            {
                System.Threading.Interlocked.Increment(ref usageCounter);
                this.Stoped.IsEnabled = true;
            }

            private readonly ObservableCollection<Security.Interfaces.IPublicKeyData> usersFromWebservice = new ObservableCollection<Security.Interfaces.IPublicKeyData>();

            ObservableCollection<Security.Interfaces.IPublicKeyData> ServerServer.IService.UsersOnServer => usersFromWebservice;
            public string Name { get; private set; }
            public Misc.Portable.AwaitBarrier Stoped { get; } = new Misc.Portable.AwaitBarrier(true);

            #region IDisposable Support

            public void Dispose()
            {
                var newValue = System.Threading.Interlocked.Decrement(ref usageCounter);

                if (newValue == 0)
                {
                    this.Stoped.IsEnabled = false;
                }


            }

            #endregion
        }
        #endregion


    }
}
