using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Client.Store.Viewmodel.Account
{
    public class LogInAccountViewmodel : DependencyObject
    {
        private Common.ProlongedCommand loginAccountCommand = null;

        public Common.ProlongedCommand LoginAccountCommand
        {
            get { return loginAccountCommand; }
        }

        //public bool IsLoading
        //{
        //    get { return (bool)GetValue(IsLoadingProperty); }
        //    set { SetValue(IsLoadingProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for IsLoading.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty IsLoadingProperty =
        //    DependencyProperty.Register("IsLoading", typeof(bool), typeof(LogInAccountViewmodel), new PropertyMetadata(false));

        //public Network.Server.CardServer.CarServer Server
        //{
        //    get { return (Network.Server.CardServer.CarServer)GetValue(ServerProperty); }
        //    set { SetValue(ServerProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Server.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ServerProperty =
        //    DependencyProperty.Register("Server", typeof(Network.Server.CardServer.CarServer), typeof(LogInAccountViewmodel), new PropertyMetadata(null, ServerChangedd));

        //private static void ServerChangedd(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var me = d as LogInAccountViewmodel;
        //    var server = e.NewValue as Network.Server.CardServer.CarServer;
        //    var serverOld = e.OldValue as Network.Server.CardServer.CarServer;
        //    if (serverOld != null && server != null && server.Host == serverOld.Host)
        //        return;
        //    if (server != null)
        //        me.ServerAddress = server.Host;
        //    else
        //        me.ServerAddress = "";
        //}

        //public String ServerAddress
        //{
        //    get { return (String)GetValue(ServerAddressProperty); }
        //    set { SetValue(ServerAddressProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for ServerAddress.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ServerAddressProperty =
        //    DependencyProperty.Register("ServerAddress", typeof(String), typeof(LogInAccountViewmodel), new PropertyMetadata(null, ServerAddressChanged));

        //public string Password
        //{
        //    get { return (string)GetValue(PasswordProperty); }
        //    set { SetValue(PasswordProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Password.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty PasswordProperty =
        //    DependencyProperty.Register("Password", typeof(string), typeof(LogInAccountViewmodel), new PropertyMetadata(null));

        //public string Email
        //{
        //    get { return (string)GetValue(EmailProperty); }
        //    set { SetValue(EmailProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Email.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty EmailProperty =
        //    DependencyProperty.Register("Email", typeof(string), typeof(LogInAccountViewmodel), new PropertyMetadata(null, EmailChanged));

        //public bool IsEmailValid
        //{
        //    get { return (bool)GetValue(IsEmailValidProperty); }
        //    set { SetValue(IsEmailValidProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for IsEmailValid.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty IsEmailValidProperty =
        //    DependencyProperty.Register("IsEmailValid", typeof(bool), typeof(LogInAccountViewmodel), new PropertyMetadata(false, validChanged));

        //private static void validChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var me = d as LogInAccountViewmodel;
        //    me.loginAccountCommand.RaiseCanExecuteChanged();
        //}

        //private static void EmailChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var me = d as LogInAccountViewmodel;
        //    var mail = e.NewValue as string;
        //    if (mail != null)
        //    {
        //        string patternlenient = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
        //        var relenient = new Regex(patternlenient);

        //        bool ismatch = relenient.IsMatch(mail);
        //        me.IsEmailValid = ismatch;
        //    }
        //}

        //private static void ServerAddressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (e.OldValue == e.NewValue)
        //        return;
        //    var me = d as LogInAccountViewmodel;
        //    var serverAdress = e.NewValue as string;
        //    if (String.IsNullOrWhiteSpace(serverAdress))
        //        me.Server = null;
        //    else
        //        me.Server = new Network.Server.CardServer.CarServer(serverAdress);
        //}

        //public LogInAccountViewmodel()
        //{
        //    this.loginAccountCommand = new Common.ProlongedCommand((obj) => LogIn(), (obj) => this.IsEmailValid);
        //}

        //private async Task LogIn()
        //{
        //    Windows.UI.Popups.MessageDialog error = null;

        //    try
        //    {
        //        Server.SetCredentials(this.Email, this.Password);

        //        var id = await Server.GetId();
        //        var userName = await Server.getName();

        //        var user = new Network.User();
        //        user.Name = userName;

        //        user.Certificate = await Securety.PrivateCertificate.CreateCertificate(id);
        //        user.Password = this.Password;
        //        //TODO: Certifikat noch Signieren
        //        //var sig = await Server.SingnSecreet(user.Certificate.PublicKey);
        //        //user.Certificate.Signatures.Add(sig);

        //        CentralViewmodel.Instance.CardServer = Server;
        //        CentralViewmodel.Instance.LogedInUser = user;
        //        CentralViewmodel.Instance.AddOrUpdateAccount(user);
        //    }
        //    catch (WebException e)
        //    {
        //        String text = e.ToString() + "\n" + e.Message;

        //        error = new Windows.UI.Popups.MessageDialog("Leider ist ein Fehler mit der Netzwerkverbindung aufgetreten.\n" + text, "Fehler im Netzwerk");
        //    }
        //    finally
        //    {
        //    }

        //    if (error != null)
        //        await error.ShowAsync();
        //}
    }
}