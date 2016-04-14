using Client.Exceptions;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using System.Collections.ObjectModel;
using Security;
using System.Net;
using Windows.Security.Cryptography.Certificates;

namespace Client.Viewmodel
{
    public class UserDataViewmodelAcces
    {
        public UserDataViewmodel Instance { get; } = UserDataViewmodel.Instance;
    }
    public class UserDataViewmodel : DependencyObject
    {
        private static UserDataViewmodel instance;
        public static UserDataViewmodel Instance
        {
            get
            {
                try
                {
                    if (!App.Current.Resources.ContainsKey("UserDataViewmodel"))
                        App.Current.Resources.Add("UserDataViewmodel", new UserDataViewmodel());
                    return App.Current.Resources["UserDataViewmodel"] as UserDataViewmodel;

                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                    throw;      
                }
            }
        }

        public Network.User LoggedInUser
        {
            get { return (Network.User)GetValue(LoggedInUserProperty); }
            set { SetValue(LoggedInUserProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LoggedInUser.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoggedInUserProperty =
            DependencyProperty.Register("LoggedInUser", typeof(Network.User), typeof(UserDataViewmodel), new PropertyMetadata(null));
        private TaskCompletionSource<UserAccount> userAccount;

        public async Task<bool> UserExists()
        {
            return null != await this.ReadUserAccount();
        }

        private UserDataViewmodel()
        {
            if (instance != null && !DesignMode.Enabled)
                throw new InvalidOperationException($"Es darf nur eine Instanz von {nameof(UserDataViewmodel)} erstellt werden.");
            instance = this;
        }



        public async Task CreateUserAccount(string name, string password, byte[] image)
        {
            var user = new Network.User();
            user.PublicKey = SecurityFactory.CreatePrivateKey();
            user.Name = name;
            user.Password = password;
            user.Image = image;

            await this.PersistUserAccount(user);
            this.LoggedInUser = user;
        }


        internal static async Task<CardServerService.ServicePortClient> GetServerWebService(string remoteAddress)
        {
            try
            {

                var remoteWithEndingSlash = remoteAddress;
                if (!remoteWithEndingSlash.EndsWith("/", StringComparison.Ordinal))
                    remoteWithEndingSlash += "/";

                var config = remoteAddress.StartsWith("https") ? CardServerService.ServicePortClient.EndpointConfiguration.https : CardServerService.ServicePortClient.EndpointConfiguration.http;
                var ws = new CardServerService.ServicePortClient(config, remoteWithEndingSlash + "ws/");
                var idResponse = await ws.ServerIdentityAsync(new CardServerService.ServerIdentityRequest());
                //Windows.Security.Cryptography.Certificates.CertificateStore x;x.Add()

                var serverId = (Client.Game.Data.ServerId)idResponse.ServerIdentityResponse.identity;

                if (serverId.Uri != remoteAddress)
                    return await GetServerWebService(serverId.Uri);

                await global::Game.Database.Database.AddServerId(serverId);

                return ws;
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



        #region PersistedAccounts




        private async Task PersistUserAccount(Network.User user)
        {
            var pCert = user.PublicKey as Security.IPrivateKey;

            if (pCert == null)
                throw new ArgumentException("User muss Privaten Key besitzen");

            var encryptedData = Encrypt(pCert, user.Password);
            var pUser = new UserAccount() { UserID = user.PublicKey, UserName = user.Name, Image = user.Image, EncryptedData = encryptedData };

            var ser = new Misc.Serialization.XmlSerilizer<UserAccount>();
            var xml = ser.Serialize(pUser);

            var folder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("PersistedAccounts", Windows.Storage.CreationCollisionOption.OpenIfExists);
            using (var stream = await (await folder.CreateFileAsync("user.data", Windows.Storage.CreationCollisionOption.ReplaceExisting)).OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
            {
                using (var writer = new StreamWriter(stream.AsStream()))
                    await writer.WriteAsync(xml);
            }
        }

        public async Task<UserAccount> ReadUserAccount()
        {
            if (userAccount != null)
                return await userAccount.Task;
            userAccount = new TaskCompletionSource<UserAccount>();
            var folder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("PersistedAccounts", Windows.Storage.CreationCollisionOption.OpenIfExists);
            string xml;

            var f = await folder.CreateFileAsync("user.data", Windows.Storage.CreationCollisionOption.GenerateUniqueName);
            if (f.Name != "user.data")
            {
                await f.DeleteAsync();
                using (var stream = await (await folder.GetFileAsync("user.data")).OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    using (var reader = new StreamReader(stream.AsStream()))
                        xml = await reader.ReadToEndAsync();
                }
                var ser = new Misc.Serialization.XmlSerilizer<UserAccount>();
                ser.AddFactoryMethod<IPublicKey>(() => SecurityFactory.CreatePrivateKey());
                userAccount.SetResult(ser.Deserilize(xml));
                return await userAccount.Task;
            }
            else
            {
                return null;
            }
        }

        private byte[] Encrypt(IPrivateKey user, string password)
        {
            var xml = user.ToPrivateXml();

            CryptographicKey key;
            IBuffer iv;
            GenerateKey(password, user, out key, out iv);
            var data = Windows.Security.Cryptography.Core.CryptographicEngine.Encrypt(key, CryptographicBuffer.ConvertStringToBinary(xml, BinaryStringEncoding.Utf8), iv);
            return data.ToArray();
        }

        private static void GenerateKey(string password, IPublicKey pk, out CryptographicKey key, out IBuffer iv)
        {
            IBuffer saltBuffer = pk.Modulus.AsBuffer();
            var kdfParameters = KeyDerivationParameters.BuildForPbkdf2(saltBuffer, 100000);

            var kdf = KeyDerivationAlgorithmProvider.OpenAlgorithm(KeyDerivationAlgorithmNames.Pbkdf2Sha256);
            var passwordBuffer = CryptographicBuffer.ConvertStringToBinary(password, BinaryStringEncoding.Utf8);
            var passwordSourceKey = kdf.CreateKey(passwordBuffer);

            int keySize = 256 / 8;
            int ivSize = 128 / 8;
            uint totalDataNeeded = (uint)(keySize + ivSize);
            var keyAndIv = CryptographicEngine.DeriveKeyMaterial(passwordSourceKey, kdfParameters, totalDataNeeded);

            var keyMaterialBytes = keyAndIv.ToArray();
            var keyMaterial = WindowsRuntimeBuffer.Create(keyMaterialBytes, 0, keySize, keySize);
            iv = WindowsRuntimeBuffer.Create(keyMaterialBytes, keySize, ivSize, ivSize);
            var algo = Windows.Security.Cryptography.Core.SymmetricKeyAlgorithmProvider.OpenAlgorithm(Windows.Security.Cryptography.Core.SymmetricAlgorithmNames.AesCbcPkcs7);
            key = algo.CreateSymmetricKey(keyMaterial);
        }

        public IPrivateKey Decrypt(UserAccount account, string password)
        {
            CryptographicKey key;
            IBuffer iv;
            GenerateKey(password, account.UserID, out key, out iv);

            var data = Windows.Security.Cryptography.Core.CryptographicEngine.Decrypt(key, account.EncryptedData.AsBuffer(), iv);
            var xml = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, data);
            var cert = Security.SecurityFactory.CreatePrivateKey();
            cert.LoadPrivateXml(xml);
            return cert;
        }




        public async Task LogIn(string pswd)
        {
            var account = await this.ReadUserAccount();
            var user = new Network.User();
            user.Name = account.UserName;

            try
            {
                user.PublicKey = Decrypt(account, pswd);
            }
            catch (Exception)
            {
                throw new InvalidPasswordException();
            }
            user.Password = pswd;
            user.Image = account.Image;

            this.LoggedInUser = user;

        }

        public class UserAccount
        {
            public Security.IPublicKey UserID { get; set; }

            public byte[] EncryptedData { get; set; }

            public byte[] Image { get; set; }

            public string UserName { get; set; }

            // override object.Equals
            public override bool Equals(object obj2)
            {
                var obj = obj2 as UserAccount;
                if (obj == null)
                {
                    return false;
                }

                return UserID.Equals(obj.UserID);
            }

            // override object.GetHashCode
            public override int GetHashCode()
            {
                return UserID.GetHashCode();
            }
        }

        #endregion PersistedAccounts

    }
}