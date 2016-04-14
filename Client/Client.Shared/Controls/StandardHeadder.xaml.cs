using Client.Common;
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

namespace Client.Controls
{
    public sealed partial class StandardHeadder : UserControl
    {



        public NavigationHelper NavigationHelper
        {
            get { return (NavigationHelper)GetValue(NavigationHelperProperty); }
            set { SetValue(NavigationHelperProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NavigationHelper.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NavigationHelperProperty =
            DependencyProperty.Register("NavigationHelper", typeof(NavigationHelper), typeof(StandardHeadder), new PropertyMetadata(null));


        public String Title
        {
            get { return (String)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(String), typeof(StandardHeadder), new PropertyMetadata(null));



        public StandardHeadder()
        {
            
            this.InitializeComponent();
        }
    }
}
