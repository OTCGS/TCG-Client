using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Client.Game.Data;
using Security;
using Windows.UI.Xaml;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Client
{
    /// <summary>
    /// Dynamic Data Resolver
    /// </summary>
    public static class DDR
    {

        private readonly static HashSet<InformationBrokerConnectivity> info = new HashSet<InformationBrokerConnectivity>();

        private static IEnumerable<InformationBrokerConnectivity> GetInfoBroker()
        {
            lock (info)
               return info.ToArray(); // Copy, wir wollen ja nicht das jamand die collection ändert während wir drüberiterieren
        }




        #region CardInstance


        public async static Task<IEnumerable<CardInstance>> GetCardInstances()
        {
            var ids = await global::Game.TransactionMap.Graph.GetCardsOf(Viewmodel.UserDataViewmodel.Instance.LoggedInUser.PublicKey);
            return await Task.WhenAll(ids.Select(x => GetCardInstances(x)));
        }

        public async static Task<global::Client.Game.Data.CardInstance> GetCardInstances(Game.Data.UuidServer id)
        {
            CardInstance cardInstance = await GetLocalCardInstance(id);
            if (cardInstance != null)
                return cardInstance;

            // Auf Server und Peers nachschauen
            cardInstance = await TaskEx.WhenAny(x => x != null,
                GetCarInstanceFromServer(id),
                GetCardInstanceFromPeer(id)
            ).Unwrap();

            if (cardInstance != null)
            {
                if (!id.Server.EqualsIPublicKeyData(cardInstance.Creator))
                    throw new InvalidOperationException($"Server returned wrong CardInstance. fingerprint should {id.Server.FingerPrint()} was {cardInstance.Creator.FingerPrint()}");
                await global::Game.Database.Database.AddCardInstance(cardInstance);
            }
            return cardInstance;
        }

        private static Task<CardInstance> GetCardInstanceFromPeer(UuidServer id)
        {
            var info = GetInfoBroker();
            if (info.Any())
                return TaskEx.WhenAny(x => x != null, info.Select(x => x.AskCardInstance(id))).Unwrap();
            else
                return Task.FromResult<CardInstance>(null);
        }


        private static async Task<CardInstance> GetCarInstanceFromServer(UuidServer id)
        {
            try
            {
                var serverId = await global::Game.Database.Database.GetServerId(id.Server);
                if (serverId == null)
                    return null;

                var ws = await Viewmodel.UserDataViewmodel.GetServerWebService(serverId.Uri);
                var instanceResponse = await ws.getCardInstanceAsync(new CardServerService.getCardInstanceRequest() { cardInstanceId = id.Uuid.ToString() });

                var getCardInstanceResponse = instanceResponse.getCardInstanceResponse;
                var cardInstance = getCardInstanceResponse.cardInstance;
                return (CardInstance)cardInstance;
            }
            catch (System.ServiceModel.FaultException e)
            {
                var key = new Game.Data.PublicKey() { Exponent = id.Server.Exponent, Modulus = id.Server.Modulus };
                Logger.MissingResource(key.FingerPrint(), id.Uuid, RecourceKind.CardInstance, e.Message);
            }
            catch (Exception e)
            {
                var key = new Game.Data.PublicKey() { Exponent = id.Server.Exponent, Modulus = id.Server.Modulus };
                Logger.LogException(e, $"Server: {key.FingerPrint()},\tUUID: {id.Uuid}");
            }
            return null;
        }

        private static async Task<CardInstance> GetLocalCardInstance(UuidServer id)
        {
            return await global::Game.Database.Database.GetCardInstance(id.Uuid, id.Server);
        }
        #endregion

        #region ImageData

        public static Task<Uri> GetCardDataImageUri(Guid id, PublicKey server)
        {
            return GetCardDataImageUri(new UuidServer() { Uuid = id, Server = server });
        }

        public static async Task<ImageData> GetImageData(UuidServer id)
        {
            ImageData data = await GetLocalImageData(id);

            if (data != null)
                return data;

            // Auf Server und Peers nachschauen
            data = await TaskEx.WhenAny(x => x != null,
                GetImageDataFromServer(id),
                GetImageDataFromPeer(id)
            ).Unwrap();

            {

                if (!id.Server.EqualsIPublicKeyData(data.Creator))
                    throw new InvalidOperationException($"Server returned wrong ImageData. fingerprint should {id.Server.FingerPrint()} was {data.Creator.FingerPrint()}");
                if (data != null)
                    await AddImageData(data);
            }
            return data;

        }

        private static Task<ImageData> GetImageDataFromPeer(UuidServer id)
        {
            var info = GetInfoBroker();
            if (info.Any())
                return TaskEx.WhenAny(x => x != null, info.Select(x => x.AskImageData(id))).Unwrap();
            else
                return Task.FromResult<ImageData>(null);
        }

        private static async Task<ImageData> GetImageDataFromServer(UuidServer id)
        {
            var serverId = await global::Game.Database.Database.GetServerId(id.Server);
            if (serverId == null)
                return null;

            var ws = await Viewmodel.UserDataViewmodel.GetServerWebService(serverId.Uri);
            if (ws == null)
            {
                Logger.MissingResource(id.Server.FingerPrint(), id.Uuid, RecourceKind.ImageData, "Webservice Offline");
                return null;
            }
            try
            {
                var response = await ws.getImageDataAsync(new CardServerService.getImageDataRequest() { imageId = id.Uuid.ToString() });
                return response.getImageDataResponse.imageData.ToGameData();
            }
            catch (System.ServiceModel.FaultException e)
            {
                var key = new Game.Data.PublicKey() { Exponent = id.Server.Exponent, Modulus = id.Server.Modulus };
                Logger.MissingResource(key.FingerPrint(), id.Uuid, RecourceKind.ImageData, e.Message);
            }
            catch (Exception e)
            {
                var key = new Game.Data.PublicKey() { Exponent = id.Server.Exponent, Modulus = id.Server.Modulus };
                Logger.LogException(e, $"Server: {key.FingerPrint()},\tUUID: {id.Uuid}");
            }
            return null;

        }

        private static async Task AddImageData(ImageData data)
        {
            Logger.Assert(await global::Game.Database.Database.AddImageData(data), "Add ImageData schlägt fehl obwohl es eigentlich nicht in der DB sein sollte.");
        }

        private static async Task<ImageData> GetLocalImageData(UuidServer id)
        {
            return await global::Game.Database.Database.GetImageData(id.Uuid, id.Server);
        }

        public static async Task<Uri> GetCardDataImageUri(UuidServer id)
        {
            var data = await GetImageData(id);

            return await GenerateUri(data);

        }

        private static async Task<Uri> GenerateUri(ImageData i)
        {
            if (i == null)
                return null;

            var fileName = i.Id.ToString() + i.Creator.Modulus.Aggregate<byte, int>(0, (x, y) => (x * 31) ^ y);
            var file = await Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.GenerateUniqueName);
            if (file.Name != fileName)
                await file.DeleteAsync();
            else
                await Windows.Storage.FileIO.WriteBytesAsync(file, i.Image);

            return new Uri("ms-appdata:///temp/" + fileName, UriKind.Absolute);
        }

        #endregion

        #region CardData

        internal static Task<CardData> GetCardData(Guid id, PublicKey server)
        {
            return GetCardData(new UuidServer() { Uuid = id, Server = server });

        }
        internal async static Task<CardData> GetCardData(UuidServer id)
        {
            var data = await GetLocalCardData(id);
            if (data != null)
                return data;

            // Auf Server und Peers nachschauen
            data = await TaskEx.WhenAny(x => x != null,
                GetCardDataFromServer(id),
                GetCardDataFromPeer(id)
            ).Unwrap();
            if (data != null)
            {

                if (!id.Server.EqualsIPublicKeyData(data.Creator))
                    throw new InvalidOperationException($"Server returned wrong CardData. fingerprint should {id.Server.FingerPrint()} was {data.Creator.FingerPrint()}");

                await AddCardData(data);
            }

            return data;
        }

        public async static Task<bool> ForceCardDataUpdate(UuidServer id)
        {
            // Auf Server und Peers nachschauen
            var data = await TaskEx.WhenAny(x => x != null,
                            GetCardDataFromServer(id)
                        ).Unwrap();

            if (data != null)
            {

                if (!id.Server.EqualsIPublicKeyData(data.Creator))
                    throw new InvalidOperationException($"Server returned wrong CardDataUpdate. fingerprint should {id.Server.FingerPrint()} was {data.Creator.FingerPrint()}");



                return await AddCardData(data);
            }
            return false;
        }

        private static Task<CardData> GetCardDataFromPeer(UuidServer id)
        {
            var info = GetInfoBroker();
            if (info.Any())
                return TaskEx.WhenAny(x => x != null, info.Select(x => x.AskCardData(id))).Unwrap();
            else
                return Task.FromResult<CardData>(null);
        }

        private static async Task<CardData> GetCardDataFromServer(UuidServer id)
        {
            try
            {
                var serverId = await global::Game.Database.Database.GetServerId(id.Server);
                if (serverId == null)
                    return null;

                var ws = await Viewmodel.UserDataViewmodel.GetServerWebService(serverId.Uri);
                var response = await ws.getCardDataAsync(new CardServerService.getCardDataRequest() { cardDataId = id.Uuid.ToString() });
                return response.getCardDataResponse.cardData.ToGameData();
            }
            catch (System.ServiceModel.FaultException e)
            {
                var key = new Game.Data.PublicKey() { Exponent = id.Server.Exponent, Modulus = id.Server.Modulus };
                Logger.MissingResource(key.FingerPrint(), id.Uuid, RecourceKind.CardData, e.Message);
            }
            catch (Exception e)
            {
                var key = new Game.Data.PublicKey() { Exponent = id.Server.Exponent, Modulus = id.Server.Modulus };
                Logger.LogException(e, $"Server: {key.FingerPrint()},\tUUID: {id.Uuid}");
            }
            return null;
        }

        private static async Task<bool> AddCardData(CardData data)
        {
            var task = await global::Game.Database.Database.AddCardData(data);
            Logger.Assert(task, "Add Kart schlägt fehl obwohl es eigentlich nicht in der DB sein sollte.");
            return task;
        }

        private static async Task<CardData> GetLocalCardData(UuidServer id)
        {
            return await global::Game.Database.Database.GetCardData(id.Uuid, id.Server);
        }

        #endregion


        #region Rules

        internal async static Task<IEnumerable<Ruleset>> GetRulesets()
        {
            var ids = await DDR.GetServerIds();
            var y = ids.Select(x => GetRulesets(x));
            var c = (await Task.WhenAll(y)).SelectMany(x => x);
            return c;
        }

        private static Task<IEnumerable<Ruleset>> GetRulesets(ServerId x)
        {
            return global::Game.Database.Database.GetRulesets(x.Key);
        }

        internal static Task<Ruleset> GetRule(Guid id, PublicKey server)
        {
            return GetRule(new UuidServer() { Uuid = id, Server = server });

        }
        internal async static Task<Ruleset> GetRule(UuidServer id)
        {


            // Auf Server und Peers nachschauen
            var data = await TaskEx.WhenAny(x => x != null,
                            GetLocalRule(id),
                            GetRuleFromServer(id),
                            GetRuleFromPeer(id)
                        ).Unwrap();


            if (data != null)
            {
                if (!id.Server.EqualsIPublicKeyData(data.Creator))
                    throw new InvalidOperationException($"Server returned wrong Rule. fingerprint should {id.Server.FingerPrint()} was {data.Creator.FingerPrint()}");
                await AddRule(data);
            }

            return data;
        }

        private static Task<Ruleset> GetRuleFromPeer(UuidServer id)
        {
            var info = GetInfoBroker();
            if (info.Any())
                return TaskEx.WhenAny(x => x != null, info.Select(x => x.AskRules(id))).Unwrap();
            else
                return Task.FromResult<Ruleset>(null);
        }

        private static async Task<Ruleset> GetRuleFromServer(UuidServer id)
        {
            try
            {
                var serverId = await global::Game.Database.Database.GetServerId(id.Server);
                if (serverId == null)
                    return null;

                var ws = await Viewmodel.UserDataViewmodel.GetServerWebService(serverId.Uri);
                var response = await ws.getRuleSetAsync(new CardServerService.getRuleSetRequest() { id = id.Uuid.ToString() });
                var rule = response.getRuleSetResponse.ruleSet.ToGameData();
                await AddRule(rule);
                return rule;
            }
            catch (System.ServiceModel.FaultException e)
            {
                var key = new Game.Data.PublicKey() { Exponent = id.Server.Exponent, Modulus = id.Server.Modulus };
                Logger.MissingResource(key.FingerPrint(), id.Uuid, RecourceKind.Rule, e.Message);
            }
            catch (Exception e)
            {
                var key = new Game.Data.PublicKey() { Exponent = id.Server.Exponent, Modulus = id.Server.Modulus };
                Logger.LogException(e, $"Server: {key.FingerPrint()},\tUUID: {id.Uuid}");
            }
            return null;
        }

        private static async Task AddRule(Ruleset data)
        {
            var added = await global::Game.Database.Database.AddRuleset(data);
            Logger.Assert(added, "Add Kart schlägt fehl obwohl es eigentlich nicht in der DB sein sollte.");
        }

        private static async Task<Ruleset> GetLocalRule(UuidServer id)
        {
            return await global::Game.Database.Database.GetRuleset(id.Uuid, id.Server);
        }

        #endregion

        #region Server

        private static readonly Dictionary<PublicKey, Trust> trustDic = new Dictionary<PublicKey, Trust>();

        public class Trust : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;



            private bool istrusted;
            private PublicKey serverKey;


            public Trust(PublicKey serverKey, bool trust)
            {
                this.serverKey = serverKey;
                this.istrusted = trust;
            }

            public bool IsTrusted
            {
                get { return istrusted; }
                private set
                {
                    if (istrusted != value)
                    {
                        istrusted = value;
                        FireChanged();
                    }
                }
            }
            [Obsolete("Darf nicht verwendet werden.")]
            internal void SetTrusted(bool trust)
            {
                IsTrusted = trust;
            }

            private void FireChanged([CallerMemberName] string name = null)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        internal async static Task<ServerId> GetServerId(PublicKey serverKey)
        {
            return await global::Game.Database.Database.GetServerId(serverKey);
        }
        internal async static Task<Trust> GetServerTrust(PublicKey serverKey)
        {
            if (trustDic.ContainsKey(serverKey))
                return trustDic[serverKey];
            var trustWorth = await global::Game.Database.Database.IsServerIdTrustworthy(serverKey);
            var trust = new Trust(serverKey, trustWorth);
            trust.PropertyChanged += async (s, e) => await global::Game.Database.Database.SetServerIdTrustworthy(serverKey, trust.IsTrusted);
            return trust;
        }

        internal async static Task SetServerTrustworthy(PublicKey serverKey, bool trust)
        {
            await global::Game.Database.Database.SetServerIdTrustworthy(serverKey, trust);
            if (trustDic.ContainsKey(serverKey))
            {
#pragma warning disable CS0618 // Typ oder Element ist veraltet (Dies ist die einzige Stelle an der dies verwendet werden darf)
                trustDic[serverKey].SetTrusted(trust);
#pragma warning restore CS0618 // Typ oder Element ist veraltet
            }
        }

        internal async static Task<IEnumerable<ServerId>> GetServerIds()
        {
            return await global::Game.Database.Database.GetServerIds();

        }

        #endregion

        public class InformationBrokerConnectivity : Network.AbstractVerifiableConnectivity<Client.Game.Data.Protokoll>, IDisposable
        {
            private readonly Dictionary<UuidServer, TaskCompletionSource<Ruleset>> rulesWaiter = new Dictionary<UuidServer, TaskCompletionSource<Ruleset>>();
            private readonly Dictionary<UuidServer, TaskCompletionSource<ImageData>> imageDataWaiter = new Dictionary<UuidServer, TaskCompletionSource<ImageData>>();
            private readonly Dictionary<UuidServer, TaskCompletionSource<CardData>> cardDataWaiter = new Dictionary<UuidServer, TaskCompletionSource<CardData>>();
            private readonly Dictionary<UuidServer, TaskCompletionSource<CardInstance>> cardInstanceWaiter = new Dictionary<UuidServer, TaskCompletionSource<CardInstance>>();


            public InformationBrokerConnectivity(Network.IUserConnection connection) : base(connection, Viewmodel.UserDataViewmodel.Instance.LoggedInUser, new byte[] { 98, 49, 87 })
            {
                lock (info)
                info.Add(this);
                BackgroundLoop();
            }

            private async void BackgroundLoop()
            {
                while (!disposedValue)
                {
                    var data = await this.Recive();

                    if (data is Game.Data.ImageData)
                    {
                        var imagedata = data as ImageData;
                        var id = new UuidServer() { Server = imagedata.Creator, Uuid = imagedata.Id };
                        SetAndRemove(id, imagedata);
                    }
                    else if (data is Game.Data.CardData)
                    {
                        var carddata = data as CardData;
                        var id = new UuidServer() { Server = carddata.Creator, Uuid = carddata.Id };
                        SetAndRemove(id, carddata);
                    }
                    else if (data is Game.Data.CardInstance)
                    {
                        var cardinstance = data as CardInstance;
                        var id = new UuidServer() { Server = cardinstance.Creator, Uuid = cardinstance.Id };
                        SetAndRemove(id, cardinstance);
                    }
                    else if (data is Game.Data.ResourceNotAvailable)
                    {
                        var nan = data as ResourceNotAvailable;
                        switch (nan.Type)
                        {
                            case ResourceType.ImageData:
                                SetAndRemove<ImageData>(nan.Id, null);
                                break;
                            case ResourceType.CardData:
                                SetAndRemove<CardData>(nan.Id, null);
                                break;
                            case ResourceType.CardInstance:
                                SetAndRemove<CardInstance>(nan.Id, null);
                                break;
                            default:
                                break;
                        }
                    }
                    else if (data is Game.Data.AskResource)
                    {
                        var ask = data as AskResource;
                        Protokoll localData;
                        switch (ask.Type)
                        {
                            case ResourceType.ImageData:
                                localData = await DDR.GetLocalImageData(ask.Id);
                                break;
                            case ResourceType.CardData:
                                localData = await DDR.GetLocalCardData(ask.Id);
                                break;
                            case ResourceType.CardInstance:
                                localData = await DDR.GetLocalCardInstance(ask.Id);
                                break;
                            case ResourceType.Rules:
                                localData = await DDR.GetLocalRule(ask.Id);
                                break;
                            default:
                                localData = null;
                                break;
                        }
                        await SendMessage(localData ?? new ResourceNotAvailable() { Id = ask.Id, Type = ask.Type });
                    }
                    else
                    {
                        if (System.Diagnostics.Debugger.IsAttached)
                            System.Diagnostics.Debugger.Break();
                        // Sollte nicht eintredten, Protokolieren
                    }

                }
            }

            public async Task<Ruleset> AskRules(UuidServer id)
            {
                if (disposedValue)
                    return null;
                var ask = new AskResource() { Id = id, Type = ResourceType.ImageData };
                if (!rulesWaiter.ContainsKey(id))
                {
                    var tcs = new TaskCompletionSource<Ruleset>();
                    rulesWaiter.Add(id, tcs);
                    await SendMessage(ask);
                    return await tcs.Task;
                }
                return await rulesWaiter[id].Task;
            }
            public async Task<ImageData> AskImageData(UuidServer id)
            {
                if (disposedValue)
                    return null;
                var ask = new AskResource() { Id = id, Type = ResourceType.ImageData };
                if (!imageDataWaiter.ContainsKey(id))
                {
                    var tcs = new TaskCompletionSource<ImageData>();
                    imageDataWaiter.Add(id, tcs);
                    await SendMessage(ask);
                    return await tcs.Task;
                }
                return await imageDataWaiter[id].Task;
            }

            public async Task<CardData> AskCardData(UuidServer id)
            {
                if (disposedValue)
                    return null;
                var ask = new AskResource() { Id = id, Type = ResourceType.CardData };
                if (!cardDataWaiter.ContainsKey(id))
                {
                    var tcs = new TaskCompletionSource<CardData>();
                    cardDataWaiter.Add(id, tcs);
                    await SendMessage(ask);
                    return await tcs.Task;
                }
                return await cardDataWaiter[id].Task;
            }

            public async Task<CardInstance> AskCardInstance(UuidServer id)
            {
                if (disposedValue)
                    return null; var ask = new AskResource() { Id = id, Type = ResourceType.CardInstance };
                if (!cardInstanceWaiter.ContainsKey(id))
                {
                    var tcs = new TaskCompletionSource<CardInstance>();
                    cardInstanceWaiter.Add(id, tcs);
                    await SendMessage(ask);
                    return await tcs.Task;
                }
                return await cardInstanceWaiter[id].Task;
            }

            private void SetAndRemove<T>(UuidServer id, T value) where T : Protokoll
            {
                Dictionary<UuidServer, TaskCompletionSource<T>> waiter;
                if (typeof(T) == typeof(ImageData))
                    waiter = imageDataWaiter as Dictionary<UuidServer, TaskCompletionSource<T>>;
                else if (typeof(T) == typeof(CardData))
                    waiter = cardDataWaiter as Dictionary<UuidServer, TaskCompletionSource<T>>;
                else if (typeof(T) == typeof(CardInstance))
                    waiter = cardInstanceWaiter as Dictionary<UuidServer, TaskCompletionSource<T>>;
                else
                {
                    Logger.Failure("DDR.InformationBrokerConnection.SetAndRemove(...) T was " + typeof(T));
                    return;
                }
                if (waiter.ContainsKey(id))
                {
                    waiter[id].SetResult(value);
                    waiter.Remove(id);
                }
            }

            protected override Task<Protokoll> ConvertFromByte(byte[] data)
            {
                var xml = UTF8Encoding.UTF8.GetString(data, 0, data.Length);
                return Task.FromResult((Protokoll)Game.Data.Protokoll.Convert(xml));
            }

            protected override Task<byte[]> ConvertToByte(Protokoll data)
            {
                var str = Game.Data.Protokoll.Convert(data);
                return Task.FromResult(Encoding.UTF8.GetBytes(str));
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // dafür sorgen, das wir keine anfragen mehr bekommen
                        lock (info)
                            info.Remove(this);

                        // Alle noch Wartenden noch benachrichtigen
                        foreach (var item in imageDataWaiter.Values)
                            item.SetResult(null);
                        foreach (var item in cardDataWaiter.Values)
                            item.SetResult(null);
                        foreach (var item in cardInstanceWaiter.Values)
                            item.SetResult(null);

                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    disposedValue = true;
                }
            }

            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources. 
            // ~InformationBrokerConnection() {
            //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            //   Dispose(false);
            // }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                // TODO: uncomment the following line if the finalizer is overridden above.
                // GC.SuppressFinalize(this);
            }
            #endregion
        }

    }
}
