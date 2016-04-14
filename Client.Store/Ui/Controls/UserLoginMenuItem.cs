using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// Die Elementvorlage "Steuerelement mit Vorlagen" ist unter http://go.microsoft.com/fwlink/?LinkId=234235 dokumentiert.

namespace Client.Store.Controls
{
    public sealed class UserLoginMenuItem : MenuFlyoutItem
    {
        public UserLoginMenuItem()
        {
            this.DefaultStyleKey = typeof(UserLoginMenuItem);
        }

        public Viewmodel.CentralViewmodel.CentralViewmodelData.UserAccount UserAccount
        {
            get { return (Viewmodel.CentralViewmodel.CentralViewmodelData.UserAccount)GetValue(UserAccountProperty); }
            set { SetValue(UserAccountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UserAccount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserAccountProperty =
            DependencyProperty.Register("UserAccount", typeof(Viewmodel.CentralViewmodel.CentralViewmodelData.UserAccount), typeof(UserLoginMenuItem), new PropertyMetadata(null));
    }
}