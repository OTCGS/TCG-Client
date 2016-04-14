using Network;
using Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public abstract class AbstractVerifiableConnectivity<T>
    {
        protected readonly Network.IUserConnection Connection;
        private readonly byte[] MAGIC;
        private readonly System.Collections.Concurrent.ConcurrentChannel<T> messeageQue = new System.Collections.Concurrent.ConcurrentChannel<T>();
        private readonly User MeUser;

        protected Task<T> Peek()
        {
            return messeageQue.Peek();
        }

        protected Task<T> Recive()
        {
            return messeageQue.Recive();
        }

        public AbstractVerifiableConnectivity(Network.IUserConnection connection, User me, byte[] magic)
        {
            this.Connection = connection;
            MeUser = me;
            MAGIC = magic;

            Connection.Recived += MessageRecived;
        }

        private async void MessageRecived(byte[] erg)
        {
            if (!erg.Take(MAGIC.Length).SequenceEqual(MAGIC))
                return;
            erg = erg.Skip(MAGIC.Length).ToArray();
            var length = BitConverter.ToInt32(erg, 0);
            Logger.TransactionInfo("Länge: " + length);
            var toValidate = erg.Skip(4).Take(length).ToArray();
            var toReturn = await ConvertFromByte(toValidate);
            Logger.Information($"Message of Type {toReturn?.GetType()} recived. ({toReturn?.ToString()})");
            var sig = erg.Skip(4 + length).ToArray();
            var isValid = Connection.User.PublicKey.Veryfiy(toValidate, sig);

            Logger.TransactionInfo("Anderer Schlüssel: " + Connection.User.PublicKey.FingerPrint());


            Logger.TransactionInfo("Data Recived: " + Convert.ToBase64String(toValidate));
            Logger.TransactionInfo("sig  Recived: " + Convert.ToBase64String(sig));

            if (!isValid)
                throw new Exception("Wrong Signiture");
            await messeageQue.Send(toReturn);
        }

        protected abstract Task<T> ConvertFromByte(byte[] data);

        protected abstract Task<byte[]> ConvertToByte(T data);

        protected async Task SendMessage(T t)
        {
            var data = await ConvertToByte(t);
            var signiture = await (MeUser.PublicKey as Security.IPrivateKey).Sign(data);
            var bLength = BitConverter.GetBytes(data.Length);

            Logger.TransactionInfo("Eingene Unterschrift gülltig: " + MeUser.PublicKey.Veryfiy(data, signiture));
            Logger.TransactionInfo("Eigener Schlüssel: " + MeUser.PublicKey.FingerPrint());
            Logger.TransactionInfo("Anderer Schlüssel: " + Connection.User.PublicKey.FingerPrint());
            Logger.TransactionInfo("Anderer Valid: " + Connection.User.PublicKey.ValidParameter);
            Logger.TransactionInfo("Meins   Valid: " + MeUser.PublicKey.ValidParameter);
            Logger.TransactionInfo("Data Send: " + Convert.ToBase64String(data));
            Logger.TransactionInfo("sig  Send: " + Convert.ToBase64String(signiture));
            Logger.TransactionInfo("Länge: " + data.Length);

            Logger.Information($"Sending type {t?.GetType()}");
            await Connection.Send(MAGIC.Concat(bLength).Concat(data).Concat(signiture).ToArray());
            Logger.Information($"Sended type {t?.GetType()}");
        }
    }
    public abstract class AbstractConnectivity<T>
    {
        protected readonly Network.IConnection Connection;
        private readonly byte[] MAGIC;
        private readonly System.Collections.Concurrent.ConcurrentChannel<T> messeageQue = new System.Collections.Concurrent.ConcurrentChannel<T>();

        public Task<T> Peek()
        {
            return messeageQue.Peek();
        }

        public Task<T> Recive()
        {
            return messeageQue.Recive();
        }

        public AbstractConnectivity(Network.IConnection connection, byte[] magic)
        {
            this.Connection = connection;
            MAGIC = magic;

            Connection.Recived += MessageRecived;
        }

        private async void MessageRecived(byte[] erg)
        {
            if (!erg.Take(MAGIC.Length).SequenceEqual(MAGIC))
                return;
            erg = erg.Skip(MAGIC.Length).ToArray();
            var toReturn = await ConvertFromByte(erg);

            await messeageQue.Send(toReturn);
        }

        protected abstract Task<T> ConvertFromByte(byte[] data);

        protected abstract Task<byte[]> ConvertToByte(T data);

        public async Task SendMessage(T t)
        {
            var data = await ConvertToByte(t);
            await Connection.Send(MAGIC.Concat(data).ToArray());
        }
    }
}