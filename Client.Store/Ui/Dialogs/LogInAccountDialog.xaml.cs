using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Benutzersteuerelement" ist unter http://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace Client.Store.Dialogs
{
    public sealed partial class LogInAccountDialog : UserControl
    {
        public Viewmodel.Account.LogInAccountViewmodel DefaultViewModel
        {
            get { return this.DataContext as Viewmodel.Account.LogInAccountViewmodel; }
        }

        public LogInAccountDialog()
        {
            this.InitializeComponent();
        }
    }
}