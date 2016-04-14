using Client.Store.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Client.Store.Viewmodel.Account
{
    public class LogInPersistedAccountViewmodel : DependencyObject
    {
        private Common.ProlongedCommand loginAccountCommand;

        public Common.ProlongedCommand LoginAccountCommand
        {
            get { return loginAccountCommand; }
        }

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsLoading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(CreateAccountViewmodel), new PropertyMetadata(false));

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Password.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(LogInAccountViewmodel), new PropertyMetadata(null));

        private CentralViewmodel.CentralViewmodelData.UserAccount userAccount;

        public LogInPersistedAccountViewmodel(CentralViewmodel.CentralViewmodelData.UserAccount item)
        {
            this.userAccount = item;
            this.loginAccountCommand = new Common.ProlongedCommand((obj) => LogIn(), (obj) => true);
        }

        private async Task LogIn()
        {
            Windows.UI.Popups.MessageDialog error = null;

            try
            {
                IsLoading = true;

                var user = new Network.User();
                user.Name = userAccount.UserName;

                try
                {
                    user.Certificate =  CentralViewmodel.Instance.Decrypt(userAccount, this.Password);
                }
                catch (Exception)
                {
                    throw new InvalidPasswordException();
                }
                user.Password = this.Password;
                user.Image = userAccount.Image;
                CentralViewmodel.Instance.LogedInUser = user;
            }
            catch (InvalidPasswordException)
            {
                error = new Windows.UI.Popups.MessageDialog("Falsches Passwort", "Fehlerhaftes Passwort");
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