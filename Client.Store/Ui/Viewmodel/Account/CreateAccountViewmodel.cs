
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace Client.Store.Viewmodel.Account
{
    public class CreateAccountViewmodel : DependencyObject
    {
        public CardServerService.CardServiceClient Server
        {
            get { return (CardServerService.CardServiceClient)GetValue(ServerProperty); }
            set { SetValue(ServerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Server.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ServerProperty =
            DependencyProperty.Register("Server", typeof(CardServerService.CardServiceClient), typeof(CreateAccountViewmodel), new PropertyMetadata(null, ServerChangedd));

        private static void ServerChangedd(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as CreateAccountViewmodel;
            var server = e.NewValue as CardServerService.CardServiceClient;
            var serverOld = e.OldValue as CardServerService.CardServiceClient;
            if (serverOld != null && server != null && server.Endpoint.Equals(serverOld.Endpoint))
                return; 
            if (server != null)
                me.ServerAddress = server.Endpoint.Name;
            else
                me.ServerAddress = "";
        }

        public String ServerAddress
        {
            get { return (String)GetValue(ServerAddressProperty); }
            set { SetValue(ServerAddressProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ServerAddress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ServerAddressProperty =
            DependencyProperty.Register("ServerAddress", typeof(String), typeof(CreateAccountViewmodel), new PropertyMetadata(null, ServerAddressChanged));

        private static void ServerAddressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue)
                return;
            var me = d as CreateAccountViewmodel;
            var serverAdress = e.NewValue as string;
            if (String.IsNullOrWhiteSpace(serverAdress))
                me.Server = null;
            else
                me.Server = new CardServerService.CardServiceClient(CardServerService.CardServiceClient.EndpointConfiguration.CardServicePort, serverAdress);
        }

        public String UserName
        {
            get { return (String)GetValue(UserNameProperty); }
            set { SetValue(UserNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UserName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserNameProperty =
            DependencyProperty.Register("UserName", typeof(String), typeof(CreateAccountViewmodel), new PropertyMetadata(null, usernameChanged));

        private static void usernameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as CreateAccountViewmodel;
            me.IsUsernameValid = !String.IsNullOrWhiteSpace(e.NewValue as string);
        }

        public bool IsUsernameValid
        {
            get { return (bool)GetValue(IsUsernameValidProperty); }
            set { SetValue(IsUsernameValidProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsUsernameValid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsUsernameValidProperty =
            DependencyProperty.Register("IsUsernameValid", typeof(bool), typeof(CreateAccountViewmodel), new PropertyMetadata(false, validChanged));

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Password.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(CreateAccountViewmodel), new PropertyMetadata(null, passwordChanged));

        public string PasswordRetype
        {
            get { return (string)GetValue(PasswordRetypeProperty); }
            set { SetValue(PasswordRetypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PasswordRetype.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PasswordRetypeProperty =
            DependencyProperty.Register("PasswordRetype", typeof(string), typeof(CreateAccountViewmodel), new PropertyMetadata(null, passwordChanged));

        private static void passwordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as CreateAccountViewmodel;
            me.IsPasswordValid = me.Password == me.PasswordRetype;
        }

        public string Email
        {
            get { return (string)GetValue(EmailProperty); }
            set { SetValue(EmailProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Email.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EmailProperty =
            DependencyProperty.Register("Email", typeof(string), typeof(CreateAccountViewmodel), new PropertyMetadata(null, EmailChanged));

        private static void EmailChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as CreateAccountViewmodel;
            var mail = e.NewValue as string;
            if (mail != null)
            {
                string patternlenient = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
                var relenient = new Regex(patternlenient);

                bool ismatch = relenient.IsMatch(mail);
                me.IsEmailValid = ismatch;
            }
        }

        public bool IsPasswordValid
        {
            get { return (bool)GetValue(IsPasswordValidProperty); }
            set { SetValue(IsPasswordValidProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPasswordValid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPasswordValidProperty =
            DependencyProperty.Register("IsPasswordValid", typeof(bool), typeof(CreateAccountViewmodel), new PropertyMetadata(false, validChanged));

        public bool IsEmailValid
        {
            get { return (bool)GetValue(IsEmailValidProperty); }
            set { SetValue(IsEmailValidProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsEmailValid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEmailValidProperty =
            DependencyProperty.Register("IsEmailValid", typeof(bool), typeof(CreateAccountViewmodel), new PropertyMetadata(false, validChanged));

        private static void validChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as CreateAccountViewmodel;
            me.createAccountcommand.RaiseCanExecuteChanged();
        }

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsLoading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(CreateAccountViewmodel), new PropertyMetadata(false));

        public byte[] Image
        {
            get { return (byte[])GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(byte[]), typeof(CreateAccountViewmodel), new PropertyMetadata(new byte[0]));

        private readonly Common.ProlongedCommand createAccountcommand;

        public Common.ProlongedCommand CreateAccountCommand
        {
            get { return createAccountcommand; }
        }

        public CreateAccountViewmodel()
        {
            this.createAccountcommand = new Common.ProlongedCommand((obj) => CreateAccount(), (obj) => this.IsEmailValid && IsPasswordValid && this.IsUsernameValid);
        }

        private async Task CreateAccount()
        {
            Windows.UI.Popups.MessageDialog error = null;
            try
            {
                IsLoading = true;

                var id = await Server.CreateAccountAsync(this.Email, this.Password);
                
                var user = new Network.User();
                user.Name = UserName;
                user.Image = Image;
                user.Certificate = await Securety.PrivateCertificate.CreateCertificate(Guid.Parse(id.@return));
                user.Password = this.Password;
                //TODO: Certifikat noch Signieren
                //var sig = await Server.GetUserCertificateAsync(new CardService.loginCredentials() { Email = this.Email, passwordHash = this.Password }  );

                //user.Certificate.Signatures.Add(sig);

                //CentralViewmodel.Instance.CardServer = Server;
                CentralViewmodel.Instance.LogedInUser = user;
                CentralViewmodel.Instance.AddOrUpdateAccount(user);
            }
            catch (WebException e)
            {
                String text = e.ToString() + "\n" + e.Message;

                error = new Windows.UI.Popups.MessageDialog("Leider ist ein Fehler mit der Netzwerkverbindung aufgetreten.\n" + text, "Fehler im Netzwerk");
            }
            finally
            {
                IsLoading = false;
            }
            if (error != null)
                await error.ShowAsync();
        }
    }
}