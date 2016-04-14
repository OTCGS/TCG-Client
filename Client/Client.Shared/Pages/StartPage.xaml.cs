using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Client.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StartPage : Page
    {
        public StartPage()
        {
            this.InitializeComponent();
            this.Loaded += StartPage_Loaded;
        }

        private async void StartPage_Loaded(object sender, RoutedEventArgs e)
        {
            var t = new TaskCompletionSource<object>();

            var storyboard = new Storyboard();

            object value;
            this.Resources.TryGetValue("SystemControlForegroundAccentBrush", out value);
            SolidColorBrush forground = value as SolidColorBrush;

            var iconAnimation = new ColorAnimation();
            iconAnimation.To = forground?.Color ?? Colors.White;
            iconAnimation.Duration = TimeSpan.FromSeconds(.5);
            Storyboard.SetTarget(iconAnimation, path);
            Storyboard.SetTargetProperty(iconAnimation, "(Path.Fill).(SolidColorBrush.Color)");
            storyboard.Children.Add(iconAnimation);


            this.Resources.TryGetValue("ApplicationPageBackgroundThemeBrush", out value);
            SolidColorBrush background = value as SolidColorBrush;

            var bgAnimation = new ColorAnimation();
            bgAnimation.To = background?.Color ?? Colors.Black;
            bgAnimation.Duration = TimeSpan.FromSeconds(.5);
            Storyboard.SetTarget(bgAnimation, grid);
            Storyboard.SetTargetProperty(bgAnimation, "(Control.Background).(SolidColorBrush.Color)");
            storyboard.Children.Add(bgAnimation);


            storyboard.Completed += (s, ev) => t.SetResult(null);
            storyboard.Begin();
            await t.Task;

            if (await Viewmodel.UserDataViewmodel.Instance.UserExists())
                App.RootFrame.Navigate(typeof(Pages.LogIn));
            else
                App.RootFrame.Navigate(typeof(Pages.CreateAccount));

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (Frame.CanGoBack)
            {
                PageStackEntry lastPage = Frame.BackStack[Frame.BackStackDepth - 1];
                if (lastPage.SourcePageType == typeof(StartPage))
                {
                    Frame.BackStack.Remove(lastPage);
                }
            }
        }
    }
}
