using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace Client.Viewmodel
{
    class TradeViewModel : DependencyObject, IDisposable
    {
        /// <summary>
        /// Karten welcher der andere Spieler einem offeriert.
        /// </summary>
        public ObservableCollection<CardViewmodel> OfferedCards { get; } = new ObservableCollection<CardViewmodel>();

        public CardCollectionViewmodel OwnCards { get; } = new CardCollectionViewmodel();
        /// <summary>
        /// Die Karten welche dem anderem Oferiert werden sollen.
        /// </summary>
        public ObservableCollection<CardViewmodel> SelectedCardsToOffer { get; } = new ObservableCollection<CardViewmodel>();

        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();



        public bool IsTrading
        {
            get { return (bool)GetValue(IsTradingProperty); }
            set { SetValue(IsTradingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTrading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTradingProperty =
            DependencyProperty.Register(nameof(IsTrading), typeof(bool), typeof(TradeViewModel), new PropertyMetadata(false));




        public Client.Game.Data.TradeAgreement RecivedTradeOffer
        {
            get { return (Client.Game.Data.TradeAgreement)GetValue(RecivedTradeOfferProperty); }
            set { SetValue(RecivedTradeOfferProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RecivedTradeOffer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RecivedTradeOfferProperty =
            DependencyProperty.Register(nameof(RecivedTradeOffer), typeof(Client.Game.Data.TradeAgreement), typeof(TradeViewModel), new PropertyMetadata(null));

        internal Task QuitConnection()
        {
            return this.TradeConnection.SendExit();
        }

        public event Action SessionEnded;

        public Trade.TradeConnectivity TradeConnection
        {
            get { return (Trade.TradeConnectivity)GetValue(TradeConnectionProperty); }
            set { SetValue(TradeConnectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TradeConnection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TradeConnectionProperty =
            DependencyProperty.Register(nameof(TradeConnection), typeof(Trade.TradeConnectivity), typeof(TradeViewModel), new PropertyMetadata(null, TradeConnectionChanged));




        public Client.Game.Data.TradeAgreement SendedTradeOffer
        {
            get { return (Client.Game.Data.TradeAgreement)GetValue(SendAgreementProperty); }
            set { SetValue(SendAgreementProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SendAgreement.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SendAgreementProperty =
            DependencyProperty.Register(nameof(SendedTradeOffer), typeof(Client.Game.Data.TradeAgreement), typeof(TradeViewModel), new PropertyMetadata(null));




        private static void TradeConnectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as TradeViewModel;
            if (e.OldValue != null)
            {
                var con = e.OldValue as Trade.TradeConnectivity;
                con.AgreementRecived -= me.Con_AgreementRecived;
                con.ExitRecived -= me.Con_ExitRecived;
                con.MessageRecived -= me.Con_MessageRecived;
                con.OfferRecived -= me.Con_OfferRecived;
                con.RejectedRecived -= me.Con_RejectedRecived;
                con.SigningRecived -= me.Con_SigningRecived;
            }
            if (e.NewValue != null)
            {
                var con = e.NewValue as Trade.TradeConnectivity;
                con.AgreementRecived += me.Con_AgreementRecived;
                con.ExitRecived += me.Con_ExitRecived;
                con.MessageRecived += me.Con_MessageRecived;
                con.OfferRecived += me.Con_OfferRecived;
                con.RejectedRecived += me.Con_RejectedRecived;
                con.SigningRecived += me.Con_SigningRecived;
            }


        }

        TaskCompletionSource<byte[]> signBytes;
        System.Threading.DisposingUsingSemaphore signSemaphor = new System.Threading.DisposingUsingSemaphore();

        private async void Con_SigningRecived(Client.Game.Data.TradeSigning obj)
        {
            using (await signSemaphor.Enter())
            {
                if (signBytes == null)
                    signBytes = new TaskCompletionSource<byte[]>();
                else if (signBytes.Task.Status.HasFlag(TaskStatus.Faulted) || signBytes.Task.Status.HasFlag(TaskStatus.RanToCompletion) || signBytes.Task.Status.HasFlag(TaskStatus.Canceled))
                    signBytes = new TaskCompletionSource<byte[]>();
                signBytes.SetResult(obj.Signiture);
            }
        }

        private void Con_RejectedRecived(Client.Game.Data.TradeReject obj)
        {
            if (SendedTradeOffer != null && SendedTradeOffer.CardsGiven.SequenceEqual(obj.CardsTaken) && SendedTradeOffer.CardsTaken.SequenceEqual(obj.CardsGiven))
                this.SendedTradeOffer = null;

            if (RecivedTradeOffer != null && RecivedTradeOffer.CardsGiven.SequenceEqual(obj.CardsGiven) && RecivedTradeOffer.CardsTaken.SequenceEqual(obj.CardsTaken))
                this.RecivedTradeOffer = null;
        }

        private async void Con_OfferRecived(Client.Game.Data.TradeOffer obj)
        {
            var instances = await Task.WhenAll(obj.Cards.Select(x => DDR.GetCardInstances(x)));
            var viewmodels = await Task.WhenAll(instances.Select(async x =>
           {
               var model = new CardViewmodel();
               await model.LoadData(x);
               return model;
           }));
            this.OfferedCards.UpdateCollection(viewmodels);
        }

        private void Con_MessageRecived(string obj)
        {
            this.Messages.Add(new Message() { Text = obj, User = this.TradeConnection.User });
        }

        private void Con_ExitRecived(Client.Game.Data.TradeExit obj)
        {
            if (SessionEnded != null)
                this.SessionEnded();
        }

        private async void Con_AgreementRecived(Client.Game.Data.TradeAgreement obj)
        {

            if (SendedTradeOffer != null && SendedTradeOffer.CardsGiven.SequenceEqual(obj.CardsTaken) && SendedTradeOffer.CardsTaken.SequenceEqual(obj.CardsGiven))
            {
                Logger.Assert(signBytes == null, "Es gibt bereits eine unterschrift.");
                Func<byte[], Task<byte[]>> otherSign = async toSign =>
                {
                    using (await signSemaphor.Enter())
                    {
                        if (signBytes == null)
                            signBytes = new TaskCompletionSource<byte[]>();
                    }
                    return await signBytes.Task;
                };
                IsTrading = true;
                try
                {
                    await global::Game.TransactionMap.Graph.Trade(Viewmodel.UserDataViewmodel.Instance.LoggedInUser.PublicKey, TradeConnection.User.PublicKey, SendedTradeOffer.CardsGiven, SendedTradeOffer.CardsTaken, bytes => SignAndSendSigniture(bytes), otherSign);
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                    var m = new Windows.UI.Popups.MessageDialog("Fehler beim Handel");
                }
                this.SendedTradeOffer = null;
                this.RecivedTradeOffer = null;
                signBytes = null;
                await this.OwnCards.Reload();
                IsTrading = false;
            }
            else if (SendedTradeOffer == null)
            {
                RecivedTradeOffer = obj;
            }
        }

        private async Task<byte[]> SignAndSendSigniture(byte[] bytes)
        {
            var myPrivateKey = Viewmodel.UserDataViewmodel.Instance.LoggedInUser.PublicKey as Security.IPrivateKey;
            var sig = await myPrivateKey.Sign(bytes);

            await TradeConnection.SendSigniture(sig);

            return sig;
        }

        public ICommand SendMessageCommand { get; }
        public ICommand AcceptOfferCommand { get; }
        public ICommand RejectOfferCommand { get; }
        public ICommand SendOfferCommand { get; }

        public TradeViewModel()
        {
            OwnCards.User = UserDataViewmodel.Instance.LoggedInUser;

            this.SendMessageCommand = new DelegateCommand(async str =>
            {
                var message = new Message() { Text = (string)str, User = Viewmodel.UserDataViewmodel.Instance.LoggedInUser };
                Messages.Add(message);
                await TradeConnection.SendMessage((string)str);
            });
            this.SendOfferCommand = new DelegateCommand(async () =>
            {
                if (SendedTradeOffer != null || RecivedTradeOffer != null)
                    return;
                var cardsGiven = this.SelectedCardsToOffer;
                var agreement = new Client.Game.Data.TradeAgreement();
                agreement.CardsTaken.AddRange(this.OfferedCards.Select(x => x.CardInstance));
                agreement.CardsGiven.AddRange(cardsGiven.Select(x => x.CardInstance));
                this.SendedTradeOffer = agreement;
                await TradeConnection.SendAgreement(agreement);
            });
            this.AcceptOfferCommand = new DelegateCommand(async () =>
            {
                if (RecivedTradeOffer == null)
                    return;
                Logger.Assert(signBytes == null, "Es gibt bereits eine unterschrift.");
                var agreement = new Client.Game.Data.TradeAgreement();
                agreement.CardsTaken.AddRange(this.RecivedTradeOffer.CardsGiven);
                agreement.CardsGiven.AddRange(this.RecivedTradeOffer.CardsTaken);
                await TradeConnection.SendAgreement(agreement);

                Func<byte[], Task<byte[]>> otherSign = async toSign =>
                {
                    using (await signSemaphor.Enter())
                    {
                        if (signBytes == null)
                            signBytes = new TaskCompletionSource<byte[]>();
                    }
                    return await signBytes.Task;
                };
                IsTrading = true;
                try
                {
                    await global::Game.TransactionMap.Graph.Trade(TradeConnection.User.PublicKey, Viewmodel.UserDataViewmodel.Instance.LoggedInUser.PublicKey, RecivedTradeOffer.CardsGiven, RecivedTradeOffer.CardsTaken, otherSign, bytes => SignAndSendSigniture(bytes));
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                    var m = new Windows.UI.Popups.MessageDialog("Fehler beim Handel");
                }
                this.SendedTradeOffer = null;
                this.RecivedTradeOffer = null;
                signBytes = null;
                await this.OwnCards.Reload();
                IsTrading = false;
            });
            this.RejectOfferCommand = new DelegateCommand(async () =>
            {
                if (RecivedTradeOffer != null)
                {
                    var agreement = new Client.Game.Data.TradeReject();
                    agreement.CardsTaken.AddRange(this.RecivedTradeOffer.CardsGiven);
                    agreement.CardsGiven.AddRange(this.RecivedTradeOffer.CardsTaken);
                    await TradeConnection.SendRejectAggreement(agreement);
                    this.RecivedTradeOffer = null;
                }
                if (SendedTradeOffer != null)
                {
                    var agreement = new Client.Game.Data.TradeReject();
                    agreement.CardsTaken.AddRange(this.SendedTradeOffer.CardsTaken);
                    agreement.CardsGiven.AddRange(this.SendedTradeOffer.CardsGiven);
                    await TradeConnection.SendRejectAggreement(agreement);
                    this.SendedTradeOffer = null;

                }

            });

            this.SelectedCardsToOffer.CollectionChanged += SelectedCardsToOffer_CollectionChanged;




        }


        private TaskCompletionSource<object> SendOfferWaiter;
        private async void SelectedCardsToOffer_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    var offer = new Client.Game.Data.TradeOffer();
                    offer.Cards.AddRange(this.SelectedCardsToOffer.Select(x => new Client.Game.Data.UuidServer() { Server = x.CardInstance.Creator, Uuid = x.CardInstance.Id }));

                    // Wir wollen Warten falls mehre änderungen kommen, so wollen wir nicht alle einzeln übermitteln sondern alle zusammen.
                    if (SendOfferWaiter != null)
                        SendOfferWaiter.SetCanceled();
                    var waiter = SendOfferWaiter = new TaskCompletionSource<object>(); // Eventuell ist ein TaskCompletionSource mit Tauben auf Kanonen geschossen.
                    await Task.Delay(1500);

                    // Wenn es keine einwände gab senden wir nun.
                    if (!waiter.Task.IsCanceled)
                        await TradeConnection.SendOffer(offer);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break; //Do Nothing :)
                default:
                    throw new NotSupportedException();
            }
        }

        public struct Message
        {
            public string Text { get; set; }
            public Network.User User { get; set; }

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.signSemaphor.Dispose();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this.TradeConnection.SendExit();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this.TradeConnection.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources. 
        // ~TradeViewModel() {
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
