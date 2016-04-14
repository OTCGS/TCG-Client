using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace Client.Store.Viewmodel
{
    public class CentralViewmodel
    {
        private static readonly CentralViewmodelData instance = new CentralViewmodelData();

        public static CentralViewmodelData Instance
        {
            get { return CentralViewmodel.instance; }
        }

        public class CentralViewmodelData : DependencyObject, INotifyPropertyChanged
        {
            public CentralViewmodelData()
            {
                addServerCommand = new Common.RelayCommand<string>(AddServer);
                removeServerCommand = new Common.RelayCommand<string>(RemoveServer);
                UpdatePersistedAccounts();
                Windows.Storage.ApplicationData.Current.DataChanged += (data, obj) => UpdatePersistedAccounts();

                LoadGame();
                InitTrustedCertificates();
            }

            private async void LoadGame()
            {
                var gameUri = new Uri("ms-appx:///MauMau.gRule", UriKind.Absolute);
                var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(gameUri);

                var txt = await Windows.Storage.FileIO.ReadTextAsync(file);

                this.GameData = Game.Data.GameData.Deserelize(txt);
                Game.Data.CardDataStorage.InitStandardGameCards(this.GameData);
            }

            public Network.User LogedInUser
            {
                get { return (Network.User)GetValue(LogedInUserProperty); }
                set { SetValue(LogedInUserProperty, value); }
            }

            // Using a DependencyProperty as the backing store for LogedInUser.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty LogedInUserProperty =
                DependencyProperty.Register("LogedInUser", typeof(Network.User), typeof(CentralViewmodelData), new PropertyMetadata(null, LogedInUserChanged));

            private static void LogedInUserChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                var me = d as CentralViewmodelData;
                me.FirePropertyChanged("LogedInUser");
            }

            //public Network.Server.CardServer.CarServer CardServer
            //{
            //    get { return (Network.Server.CardServer.CarServer)GetValue(CardServerProperty); }
            //    set { SetValue(CardServerProperty, value); }
            //}

            //// Using a DependencyProperty as the backing store for CardServer.  This enables animation, styling, binding, etc...
            //public static readonly DependencyProperty CardServerProperty =
            //    DependencyProperty.Register("CardServer", typeof(Network.Server.CardServer.CarServer), typeof(CentralViewmodelData), new PropertyMetadata(null));

            #region PersistedAccounts

            private IEnumerable<UserAccount> persistedaccounts;

            public IEnumerable<UserAccount> PersistedAccounts
            {
                get
                {
                    return persistedaccounts;
                }
            }

            public ICommand ClearLocalAccountsCommand
            {
                get
                {
                    return new Common.RelayCommand(ClearLocalUserAccounts);
                }
            }

            private void UpdatePersistedAccounts()
            {
                var ser = new Misc.Serialization.XmlSerilizer<UserAccount>();
                var servers = Windows.Storage.ApplicationData.Current.RoamingSettings.CreateContainer("Accounts", Windows.Storage.ApplicationDataCreateDisposition.Always);
                persistedaccounts = servers.Values.Values.Cast<string>().Select(x =>
                {
                    var ua = ser.Deserilize(x);

                    Task.Run(async () =>
                    {
                        try
                        {
                            var imageFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("Avatar", Windows.Storage.CreationCollisionOption.OpenIfExists);
                            var file = await imageFolder.GetFileAsync(ua.UserID.ToString());

                            using (var stream = await file.OpenReadAsync())
                            {
                                var b = new byte[stream.Size];
                                var buffer = b.AsBuffer();
                                await stream.ReadAsync(buffer, (uint)b.Length, InputStreamOptions.None);
                                ua.Image = b;
                            }
                        }
                        catch (Exception)
                        { }
                    });

                    return ua;
                }).ToArray();

                FirePropertyChanged("PersistedAccounts");
            }

            public async void AddOrUpdateAccount(Network.User user)
            {
                var pCert = user.Certificate as Securety.PrivateCertificate;

                if (pCert == null)
                    throw new ArgumentException("User muss Privates Certifikat besitzen");

                var encryptedData = Encrypt(pCert, user.Password);
                var pUser = new UserAccount() { UserID = user.Certificate.UserId, UserName = user.Name, Image = user.Image, EncryptedData = encryptedData };

                var ser = new Misc.Serialization.XmlSerilizer<UserAccount>();
                var xml = ser.Serialize(pUser);

                var accountsSettings = Windows.Storage.ApplicationData.Current.RoamingSettings.CreateContainer("Accounts", Windows.Storage.ApplicationDataCreateDisposition.Always);
                var imageFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("Avatar", Windows.Storage.CreationCollisionOption.OpenIfExists);
                using (var stream = await (await imageFolder.CreateFileAsync(pUser.UserID.ToString(), Windows.Storage.CreationCollisionOption.ReplaceExisting)).OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                {
                    await stream.WriteAsync(pUser.Image.AsBuffer());
                }

                accountsSettings.Values[user.Certificate.UserId.ToString()] = new Misc.Serialization.XmlSerilizer<UserAccount>().Serialize(pUser);
                UpdatePersistedAccounts();
            }

            public void ClearLocalUserAccounts()
            {
                var accountsSettings = Windows.Storage.ApplicationData.Current.RoamingSettings.CreateContainer("Accounts", Windows.Storage.ApplicationDataCreateDisposition.Always);
                accountsSettings.Values.Clear();
                UpdatePersistedAccounts();
            }

            private byte[] Encrypt(Securety.PrivateCertificate user, string password)
            {
                var xml = user.ToPrivateXml();

                CryptographicKey key;
                IBuffer iv;
                GenerateKey(password, user.UserId, out key, out iv);
                var data = Windows.Security.Cryptography.Core.CryptographicEngine.Encrypt(key, CryptographicBuffer.ConvertStringToBinary(xml, BinaryStringEncoding.Utf8), iv);
                return data.ToArray();
            }

            private static void GenerateKey(string password, Guid guid, out CryptographicKey key, out IBuffer iv)
            {
                IBuffer saltBuffer = guid.ToByteArray().AsBuffer();
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

            public Securety.PrivateCertificate Decrypt(UserAccount account, string password)
            {
                CryptographicKey key;
                IBuffer iv;
                GenerateKey(password, account.UserID, out key, out iv);

                var data = Windows.Security.Cryptography.Core.CryptographicEngine.Decrypt(key, account.EncryptedData.AsBuffer(), iv);
                var xml = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, data);
                return Securety.PrivateCertificate.ParsePrivateXml(xml);
            }

            public class UserAccount
            {
                public Guid UserID { get; set; }

                public byte[] EncryptedData { get; set; }

                [Misc.Serialization.XmlIgnore]
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

            #region ServerAdresses

            public IEnumerable<String> Serveraddresses
            {
                get
                {
                    var servers = Windows.Storage.ApplicationData.Current.RoamingSettings.CreateContainer("Servers", Windows.Storage.ApplicationDataCreateDisposition.Always);
                    return servers.Values.Select(x => x.Value).OfType<string>();
                }
            }

            private readonly ICommand addServerCommand;

            public ICommand AddServerCommand
            {
                get { return addServerCommand; }
            }

            public void AddServer(string address)
            {
                if (string.IsNullOrWhiteSpace(address))
                    return;
                var servers = Windows.Storage.ApplicationData.Current.RoamingSettings.CreateContainer("Servers", Windows.Storage.ApplicationDataCreateDisposition.Always);
                servers.Values.Add(address, address);
                FirePropertyChanged("Serveraddresses");
            }

            public void RemoveServer(string address)
            {
                if (string.IsNullOrWhiteSpace(address))
                    return;
                var servers = Windows.Storage.ApplicationData.Current.RoamingSettings.CreateContainer("Servers", Windows.Storage.ApplicationDataCreateDisposition.Always);
                servers.Values.Remove(address);
                FirePropertyChanged("Serveraddresses");
            }

            private readonly Common.RelayCommand<string> removeServerCommand;

            public Common.RelayCommand<string> RemoveServerCommand
            {
                get { return removeServerCommand; }
            }

            #endregion ServerAdresses

            #region INotifyPropertyChanged

            protected void FirePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string caller = null)
            {
                if (caller == null)
                    return;

                System.Diagnostics.Debug.Assert(this.GetType().GetRuntimeProperty(caller) != null);
                if (PropertyChanged == null)
                    return;
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }

            public event PropertyChangedEventHandler PropertyChanged;

            #endregion INotifyPropertyChanged

            #region TrustedCertificates

            private ICommand addTrustedCertificateCommand;
            private ICommand removeTrustedCertificateCommand;

            private void InitTrustedCertificates()
            {
                addTrustedCertificateCommand = new Common.RelayCommand<String>(AddTrustedCertificate);
                removeTrustedCertificateCommand = new Common.RelayCommand<Securety.Certificate>(RemoveTrustedCertificate);
                trustedCertificates.CollectionChanged += trustedCertificates_CollectionChanged;
            }

            private void trustedCertificates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        foreach (var c in e.NewItems.OfType<Securety.Certificate>())
                        {
                            Securety.ServerCertificateStore.AddTrustedCertificate(c);
                        }
                        break;

                    case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                        break;

                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        foreach (var c in e.OldItems.OfType<Securety.Certificate>())
                        {
                            Securety.ServerCertificateStore.RemoveTrustedCertificate(c);
                        }
                        break;

                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                        break;

                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        break;

                    default:
                        break;
                }
            }

            private void AddTrustedCertificate(String parameter)
            {
                //var server = new Network.Server.CardServer.CarServer(parameter);
                //var cert = await server.GetServerCertificate();
                //trustedCertificates.Add(cert);
            }

            private void RemoveTrustedCertificate(Securety.Certificate parameter)
            {
                trustedCertificates.Remove(parameter);
            }

            private readonly ObservableCollection<Securety.Certificate> trustedCertificates = new ObservableCollection<Securety.Certificate>();

            public ObservableCollection<Securety.Certificate> TrustedCertificates
            {
                get { return trustedCertificates; }
            }

            public ICommand AddTrustedCertificateCommand { get { return addTrustedCertificateCommand; } }

            public ICommand RemoveTrustedCertificateCommand { get { return addTrustedCertificateCommand; } }

            #endregion TrustedCertificates

            public Game.Data.GameData GameData { get; set; }
        }
    }
}