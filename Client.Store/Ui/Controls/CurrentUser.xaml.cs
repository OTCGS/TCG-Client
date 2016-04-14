using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Benutzersteuerelement" ist unter http://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace Client.Store.Controls
{
    public sealed partial class CurrentUser : UserControl
    {
        public CurrentUser()
        {
            this.InitializeComponent();
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            FlyoutBase.ShowAttachedFlyout(fe);
        }

        private void MenuFlyout_Opening(object sender, object e)
        {
            var menu = sender as MenuFlyout;
            menu.Items.Clear();
            var register = new MenuFlyoutItem();
            register.Text = "Registrieren";
            register.Click += async (s, ev) =>
                {
                    var d = new Dialogs.CreateAccountDialog();
                    await Common.CustomDialog.ShowDialog(d, "Registrieren", new SolidColorBrush(Colors.RoyalBlue), 1, Tuple.Create<string, ICommand>("Registrieren", d.DefaultViewModel.CreateAccountCommand), Tuple.Create<string, ICommand>("Abbruch", new Common.RelayCommand(() => { })));
                };
            menu.Items.Add(register);

            var logIn = new MenuFlyoutItem();
            logIn.Text = "Log In";

            logIn.Click += async (s, ev) =>
            {
                var d = new Dialogs.LogInAccountDialog();
                await Common.CustomDialog.ShowDialog(d, "Login", new SolidColorBrush(Colors.RoyalBlue), 1, Tuple.Create<string, ICommand>("Login", d.DefaultViewModel.LoginAccountCommand), Tuple.Create<string, ICommand>("Abbruch", new Common.RelayCommand(() => { })));
            };

            menu.Items.Add(logIn);

            menu.Items.Add(new MenuFlyoutSeparator());

            foreach (var item in Viewmodel.CentralViewmodel.Instance.PersistedAccounts)
            {
                var menueitem = new UserLoginMenuItem();
                menueitem.UserAccount = item;
                menu.Items.Add(menueitem);
                menueitem.Click += async (s, ev) =>
                    {
                        var d = new Dialogs.LogInPersistentAccountDialog();
                        d.DataContext = new Viewmodel.Account.LogInPersistedAccountViewmodel(item);
                        await Common.CustomDialog.ShowDialog(d, "LogIn", new SolidColorBrush(Colors.RoyalBlue), 1, Tuple.Create<string, ICommand>("Login", d.DefaultViewModel.LoginAccountCommand), Tuple.Create<string, ICommand>("Abbruch", new Common.RelayCommand(() => { })));
                    };
            }
        }

        private void LogOutPressed(object sender, RoutedEventArgs e)
        {
            Viewmodel.CentralViewmodel.Instance.LogedInUser = null;
        }
    }
}