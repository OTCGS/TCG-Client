using MentalCardGame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.ApplicationInsights;
using Windows.UI.ViewManagement;

namespace Client
{
    /// <summary>
    /// Stellt das anwendungsspezifische Verhalten bereit, um die Standardanwendungsklasse zu ergänzen.
    /// </summary>
    sealed partial class App : Application
    {

        partial void AppCode()
        {
            var view = ApplicationView.GetForCurrentView();
            view.TitleBar.BackgroundColor = (Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush)?.Color;
            view.TitleBar.ButtonBackgroundColor = (Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush)?.Color;

        }


    }
}
