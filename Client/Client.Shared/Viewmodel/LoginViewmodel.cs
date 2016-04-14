using Client.Common;
using Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Storage.Streams;
using Windows.UI.Xaml;



namespace Client.Viewmodel
{
    public class LoginViewmodel : DependencyObject
    {


        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsLoading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(LoginViewmodel), new PropertyMetadata(false));

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Password.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(LoginViewmodel), new PropertyMetadata(""));



        public object LoginSelectedUser
        {
            get { return GetValue(LoginSelectedUserProperty); }
            set { SetValue(LoginSelectedUserProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LoginSelectedUser.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoginSelectedUserProperty =
            DependencyProperty.Register("LoginSelectedUser", typeof(object), typeof(LoginViewmodel), new PropertyMetadata(null));



        public UserDataViewmodel.UserAccount UserData
        {
            get { return (UserDataViewmodel.UserAccount)GetValue(UserDataProperty); }
            set { SetValue(UserDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UserData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserDataProperty =
            DependencyProperty.Register("UserData", typeof(UserDataViewmodel.UserAccount), typeof(LoginViewmodel), new PropertyMetadata(null));


        public LoginViewmodel()
        {
            if (!DesignMode.Enabled)
                Init();
        }

        private async void Init()
        {
            UserData = await UserDataViewmodel.Instance.ReadUserAccount();
        }

        public RelayCommand LoginCommand
        {
            get
            {
                return new RelayCommand(Login);
            }
        }

        private async void Login()
        {
            Windows.UI.Popups.MessageDialog error = null;

            try
            {
                IsLoading = true;
                await UserDataViewmodel.Instance.LogIn(this.Password);
            }
            catch (InvalidPasswordException)
            {
                error = new Windows.UI.Popups.MessageDialog("Falsches Passwort", "Fehlerhaftes Passwort");
            }
            catch (Exception e)
            {
                String text = e.ToString() + "\n" + e.Message;
                error = new Windows.UI.Popups.MessageDialog("Leider ist ein unbekannter Fehler aufgetreten.\n" + text, "Fehler");
            }
            finally
            {
                IsLoading = false;
            }
            if (error != null)
                await error.ShowAsync();
            else
                App.RootFrame.Navigate(typeof(HubPage));
        }

    }
}