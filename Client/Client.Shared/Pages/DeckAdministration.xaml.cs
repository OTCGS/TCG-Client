using Client.Viewmodel;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Client.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DeckAdministration : Page
    {

        private DeckCollectionViewmodel Model { get { return DataContext as DeckCollectionViewmodel; } }

        /// <summary>
        /// NavigationHelper wird auf jeder Seite zur Unterstützung bei der Navigation verwendet und 
        /// Verwaltung der Prozesslebensdauer
        /// </summary>
        public NavigationHelper NavigationHelper { get; }

        public DeckAdministration()
        {
                        this.InitializeComponent();
            
            // Navigationshilfe einrichten
            this.NavigationHelper = new NavigationHelper(this);
            //this.NavigationHelper.LoadState += navigationHelper_LoadState;
            //this.NavigationHelper.SaveState += navigationHelper_SaveState;
            

            // Die Komponenten der logischen Seitennavigation einrichten, die
            // die Seite, nur einen Bereich gleichzeitig anzuzeigen.
            this.NavigationHelper.GoBackCommand = new Client.Common.RelayCommand(() => NavigationHelper.GoBack(), () => NavigationHelper.CanGoBack());
            this.DeckList.SelectionChanged += ItemListView_SelectionChanged;
            
            Window.Current.SizeChanged += Window_SizeChanged;
            
        }




        private bool internalGridViewChange = false;

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (internalGridViewChange)
                return;
            try
            {
                internalGridViewChange = true;
                if (Model.SelectedDeck == null)
                    return;
                foreach (CardViewmodel item in e.RemovedItems)
                    Model.SelectedDeck.Cards?.Remove(item);
                foreach (CardViewmodel item in e.AddedItems)
                    Model.SelectedDeck.Cards?.Add(item);

            }
            finally
            {
                internalGridViewChange = false;
            }
        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (internalGridViewChange)
                return;
            try
            {
                this.ccViewmodel = (CardCollectionViewmodel)grid.DataContext; // Aus irgend einem grund wird XAML diese Variable nicht gesetzt :(
                await this.ccViewmodel.LoadingWaiter;
                await Task.Delay(10); // Hack: Wir wollen Warten bis die Collection gefüllt ist.
                internalGridViewChange = true;
                var cards = this.Model.SelectedDeck?.Cards;
                this.CardView.SelectedItems.Clear();
                if (this.DeckList.SelectedItem == null || cards == null)
                    return;
                cards.CollectionChanged += Cards_CollectionChanged;
                foreach (var item in cards)
                    this.CardView.SelectedItems.Add(item);

            }
            finally
            {
                internalGridViewChange = false;
            }
        }

        private void Cards_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    this.CardView.SelectedItems.Add(item);
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                    this.CardView.SelectedItems.Remove(item);
        }



        #region Logische Seitennavigation

        // Die geteilte Seite ist so entworfen, dass wenn Fenster nicht genug Raum hat, um sowohl
        // die Liste als auch die Details anzuzeigen, nur jeweils ein Bereich angezeigt wird.
        //
        // All dies wird mit einer einzigen physischen Seite implementiert, die zwei logische Seiten darstellen
        // kann.  Mit dem nachfolgenden Code wird dieses Ziel erreicht, ohne dass der Benutzer aufmerksam gemacht wird auf den
        // Unterschied.

        private const int MinimumWidthForSupportingTwoPanes = 768;

        /// <summary>
        /// Wird aufgerufen, um zu bestimmen, ob die Seite als eine logische Seite oder zwei agieren soll.
        /// </summary>
        /// <returns>True, wenn das Fenster als eine logische Seite fungieren soll, false
        /// in anderen Fällen.</returns>
        private bool UsingLogicalPageNavigation()
        {
            return Window.Current.Bounds.Width < MinimumWidthForSupportingTwoPanes;
        }

        /// <summary>
        /// Mit der Änderung der Fenstergröße aufgerufen
        /// </summary>
        /// <param name="sender">Das aktuelle Fenster</param>
        /// <param name="e">Ereignisdaten, die die neue Größe des Fensters beschreiben</param>
        private void Window_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            this.InvalidateVisualState();
        }

        /// <summary>
        /// Wird aufgerufen, wenn ein Element innerhalb der Liste ausgewählt wird.
        /// </summary>
        /// <param name="sender">Die GridView, die das ausgewählte Element anzeigt.</param>
        /// <param name="e">Ereignisdaten, die beschreiben, wie die Auswahl geändert wurde.</param>
        private void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Den Ansichtszustand ungültig machen, wenn die logische Seitennavigation wirksam ist, da eine Änderung an der
            // Auswahl zu einer entsprechenden Änderung an der aktuellen logischen Seite führen kann.  Wenn
            // ein Element ausgewählt wird, führt dies dazu, dass anstelle der Elementliste
            // die Details des ausgewählten Elements angezeigt werden.  Wenn die Auswahl aufgehoben wird, hat
            // dies den umgekehrten Effekt.
            if (this.UsingLogicalPageNavigation())
                this.InvalidateVisualState();

            if (this.UsingLogicalPageNavigation())
            {
                this.NavigationHelper.GoBackCommand.RaiseCanExecuteChanged();
            }
        }

        private bool CanGoBack()
        {
            if (this.UsingLogicalPageNavigation() && this.DeckList.SelectedItem != null)
            {
                return true;
            }
            else
            {
                return this.NavigationHelper.CanGoBack();
            }
        }
        private void GoBack()
        {
            if (this.UsingLogicalPageNavigation() && this.DeckList.SelectedItem != null)
            {
                // Wenn die logische Seitennavigation wirksam ist und ein ausgewähltes Element vorliegt, werden die
                // Details dieses Elements aktuell angezeigt.  Beim Aufheben der Auswahl wird die
                // Elementliste wieder aufgerufen.  Aus Sicht des Benutzers ist dies eine logische Rückwärtsnavigation
                // Rückwärtsnavigation.
                this.DeckList.SelectedItem = null;
            }
            else
            {
                this.NavigationHelper.GoBack();
            }
        }

        private void InvalidateVisualState()
        {
            var visualState = DetermineVisualState();
            VisualStateManager.GoToState(this, visualState, false);
            this.NavigationHelper.GoBackCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Wird aufgerufen, um den Namen des visuellen Zustands zu bestimmen, der dem Ansichtszustand einer Anwendung
        /// entspricht.
        /// </summary>
        /// <returns>Der Name des gewünschten visuellen Zustands.  Dieser ist identisch mit dem Namen des
        /// Ansichtszustands, außer wenn ein ausgewähltes Element im Hochformat und in der angedockten Ansicht vorliegt, wobei
        /// diese zusätzliche logische Seite durch Hinzufügen des Suffix _Detail dargestellt wird.</returns>
        private string DetermineVisualState()
        {
            if (!UsingLogicalPageNavigation())
                return "PrimaryView";

            // Den Aktivierungszustand der Schaltfläche "Zurück" aktualisieren, wenn der Ansichtszustand geändert wird
            var logicalPageBack = this.UsingLogicalPageNavigation() && this.DeckList.SelectedItem != null;

            return logicalPageBack ? "SinglePane_Detail" : "SinglePane";
        }

        #endregion

        #region NavigationHelper-Registrierung

        /// Die in diesem Abschnitt bereitgestellten Methoden werden einfach verwendet, um
        /// damit NavigationHelper auf die Navigationsmethoden der Seite reagieren kann.
        /// 
        /// Platzieren Sie seitenspezifische Logik in Ereignishandlern für  
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// Der Navigationsparameter ist in der LoadState-Methode verfügbar 
        /// zusätzlich zum Seitenzustand, der während einer früheren Sitzung beibehalten wurde.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            NavigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
