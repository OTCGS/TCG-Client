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
using Client.Common;
using System.Threading.Tasks;

// Die Projektvorlage "Universelle Hub-Anwendung" ist unter http://go.microsoft.com/fwlink/?LinkID=391955 dokumentiert.

namespace Client
{
    /// <summary>
    /// Eine Seite, auf der eine gruppierte Auflistung von Elementen angezeigt wird.
    /// </summary>
    public sealed partial class HubPage : Page
    {

#if DEBUG
        public Visibility DebugVisibility { get; } = Visibility.Visible;
#else
        public Visibility DebugVisibility { get; } = Visibility.Collapsed;
#endif
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// Ruft den NavigationHelper ab, der zur Unterstützung bei der Navigation und Verwaltung der Prozesslebensdauer verwendet wird.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Ruft das DefaultViewModel ab. Dies kann in ein stark typisiertes Anzeigemodell geändert werden.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public HubPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
        }

        /// <summary>
        /// Füllt die Seite mit Inhalt auf, der bei der Navigation übergeben wird.  Gespeicherte Zustände werden ebenfalls
        /// bereitgestellt, wenn eine Seite aus einer vorherigen Sitzung neu erstellt wird.
        /// </summary>
        /// <param name="sender">
        /// Die Quelle des Ereignisses, normalerweise <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Ereignisdaten, die die Navigationsparameter bereitstellen, die an
        /// <see cref="Frame.Navigate(Type, object)"/> als diese Seite ursprünglich angefordert wurde und
        /// ein Wörterbuch des Zustands, der von dieser Seite während einer früheren
        /// beibehalten wurde.  Der Zustand ist beim ersten Aufrufen einer Seite NULL.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Ein geeignetes Datenmodell für die problematische Domäne erstellen, um die Beispieldaten auszutauschen
        }

        /// <summary>
        /// Wird aufgerufen, wenn auf ein Element innerhalb eines Abschnitts geklickt wird.
        /// </summary>
        /// <param name="sender">Die GridView oder ListView
        /// die das angeklickte Element anzeigt.</param>
        /// <param name="e">Ereignisdaten, die das angeklickte Element beschreiben.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Zur entsprechenden Zielseite navigieren und die neue Seite konfigurieren,
            // indem die erforderlichen Informationen als Navigationsparameter übergeben werden
            ((MenueItemModel)e.ClickedItem).FierClick();
        }

        #region NavigationHelper-Registrierung

        /// <summary>
        /// Die in diesem Abschnitt bereitgestellten Methoden werden einfach verwendet, um
        /// damit NavigationHelper auf die Navigationsmethoden der Seite reagieren kann.
        /// Platzieren Sie seitenspezifische Logik in Ereignishandlern für
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// Der Navigationsparameter ist in der LoadState-Methode verfügbar
        /// zusätzlich zum Seitenzustand, der während einer früheren Sitzung beibehalten wurde.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        private void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            Logger.LogException(new Exception($"Navigation Faild: {e.Exception}"));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion NavigationHelper-Registrierung



        private void LocalPlayClicked(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Pages.NetworkLobby));
        }


        private void DeckClicked(object sender, RoutedEventArgs e)
        {

            this.Frame.Navigate(typeof(Pages.DeckAdministration));

        }

        private void ServerOverviewClicked(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Pages.ServerOverview));
        }

        private void LogConsoleLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            Logger.LogConsolLevel = (Logger.LogLevel)cb.SelectedValue;
        }

        private void LogFileLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            Logger.LogFileLevel = (Logger.LogLevel)cb.SelectedValue;

        }

        private void LogFileLevel_Loaded(object sender, RoutedEventArgs e)
        {
            var cb = sender as ComboBox;
            foreach (var lvl in Enum.GetValues(typeof(Logger.LogLevel)))
                cb.Items.Add(lvl);
            cb.SelectedIndex = (int)Logger.LogFileLevel;
        }

        private void LogConsoleLevel_Loaded(object sender, RoutedEventArgs e)
        {
            var cb = sender as ComboBox;
            foreach (var lvl in Enum.GetValues(typeof(Logger.LogLevel)))
                cb.Items.Add(lvl);
            cb.SelectedIndex = (int)Logger.LogConsolLevel;
        }

        private void Mesure_LayoutUpdated(object sender, object e)
        {
            //Grid.ItemWidth = Mesure.ActualWidth;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(feedback.Text))
            {
                await new Windows.UI.Popups.MessageDialog("Bitte geben Sie ein Feedback an").ShowAsync();
                return;
            }
            var eventTelemetry = new Microsoft.ApplicationInsights.DataContracts.EventTelemetry("Feedback");
            eventTelemetry.Properties["Feedback"] = feedback.Text;
            eventTelemetry.Properties["Contact"] = mailTextbox.Text ?? "";

            App.Telemetry.TrackEvent(eventTelemetry);
            feedback.Text = "";
            await new Windows.UI.Popups.MessageDialog("Vielen Dank für ihr Feedback").ShowAsync();

        }
    }
}