using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Client.Game.Data;
using Network;

namespace Client.Trade
{
    class TradeConnectivity : Network.AbstractVerifiableConnectivity<Client.Game.Data.Protokoll>, IDisposable
    {
        private readonly IUserConnection connection;
                
        public TradeConnectivity(Network.IUserConnection connection) : base(connection, Viewmodel.UserDataViewmodel.Instance.LoggedInUser, new byte[] { 98, 49, 43 })
        {
            if (connection.ConnectionReason != Network.ConnectionReason.Trade)
                throw new ArgumentException("Der ConnectionReasen der Connection muss Trade sein, war " + connection.ConnectionReason);
            this.connection = connection;
            User = connection.User;

            this.InformationBroker = new DDR.InformationBrokerConnectivity(connection);

            BackgroundLoop();
        }

        private async void BackgroundLoop()
        {
            while (!disposedValue)
            {
                try
                {
                    var data = await this.Recive();

                    if (data is Game.Data.TradeAgreement)
                    {
                        if (AgreementRecived != null)
                            AgreementRecived(data as TradeAgreement);
                        else
                            Logger.Warning($"Listener nicht angemeldet {nameof(TradeAgreement)}");
                    }
                    else if (data is Game.Data.TradeMessage)
                    {
                        if (MessageRecived != null)
                            MessageRecived((data as TradeMessage).Text);
                        else
                            Logger.Warning($"Listener nicht angemeldet {nameof(TradeMessage)}");
                    }
                    else if (data is Game.Data.TradeOffer)
                    {
                        if (OfferRecived != null)
                            OfferRecived(data as TradeOffer);
                        else
                            Logger.Warning($"Listener nicht angemeldet {nameof(TradeOffer)}");
                    }
                    else if (data is Game.Data.TradeReject)
                    {
                        if (RejectedRecived != null)
                            RejectedRecived(data as TradeReject);
                        else
                            Logger.Warning($"Listener nicht angemeldet {nameof(TradeReject)}");
                    }
                    else if (data is Game.Data.TradeSigning)
                    {
                        if (SigningRecived != null)
                            SigningRecived(data as TradeSigning);
                        else
                            Logger.Warning($"Listener nicht angemeldet {nameof(TradeSigning)}");
                    }
                    else if (data is Game.Data.TradeExit)
                    {
                        if (ExitRecived != null)
                            ExitRecived(data as TradeExit);
                        else
                            Logger.Warning($"Listener nicht angemeldet {nameof(TradeExit)}");
                    }
                    else
                    {
                        throw new NotSupportedException($"Der Typ {data.GetType()} ist nicht Bekannt.");
                    }

                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            }
        }

        public event Action<String> MessageRecived;
        public event Action<TradeOffer> OfferRecived;
        public event Action<TradeReject> RejectedRecived;
        public event Action<TradeAgreement> AgreementRecived;
        public event Action<TradeSigning> SigningRecived;
        public event Action<TradeExit> ExitRecived;


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

        public User User { get; }
        public DDR.InformationBrokerConnectivity InformationBroker { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    InformationBroker.Dispose();
                    connection.Dispose();
                }

    
                disposedValue = true;
            }
        }

    
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        internal async Task SendSigniture(byte[] sig)
        {
            await base.SendMessage(new TradeSigning() { Signiture = sig });
        }

        internal async Task SendMessage(string str)
        {
            await base.SendMessage(new TradeMessage() { Text = str });
        }

        internal async Task SendAgreement(TradeAgreement agreement)
        {
            await base.SendMessage(agreement);
        }

        internal async Task SendRejectAggreement(TradeReject agreement)
        {
            await base.SendMessage(agreement);
        }

        internal async Task SendOffer(TradeOffer offer)
        {
            await base.SendMessage(offer);
        }

        internal async Task SendExit()
        {
            await base.SendMessage(new TradeExit());
        }
        #endregion
    }
}
