using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Vysivacka
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Instance třídy Vysivani
        /// </summary>
        private Vysivani vysivani;
        /// <summary>
        /// Instance třídy SpravceOsob
        /// </summary>
        private SpravceOsob spravceOsob;
        /// <summary>
        /// Určuje, podle kterého sloupce se třídí
        /// </summary>
        private GridViewColumnHeader osobyListViewSortSloupec = null;
        /// <summary>
        /// Určuje, kde je umístěn trojúhelník pro třídění
        /// </summary>
        private SortAdorner osobyListViewSortAdorner = null;
        private bool jeStisknutaPauza;
        private bool blokujVse;

        /// <summary>
        /// Inicializuje formulář
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            vysivani = new Vysivani(platnoCanvas, ledka);
            spravceOsob = new SpravceOsob();
            try
            {
                spravceOsob.NactiOsoby();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            barvaPlatnaComboBox.SelectionChanged += BarvaPlatnaComboBox_SelectionChanged;
            DataContext = spravceOsob;
            odpocetTextBlock.DataContext = vysivani;
        }

        /// <summary>
        /// Spustí vyšívání
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VysivejButton_Click(object sender, RoutedEventArgs e)
        {
            if (vysivani.timer.IsEnabled || vysivani.PocetStehuOdpocet <= 0) // Kontrola, jestli už není spuštěná instance DispatcherTimer nebo je hotové plátno
                return;
            if (jeStisknutaPauza) // Vyšívání po stisknutém tlačítku Pauza nebo z uložených pláten
            {
                vysivani.timer.Start();                   // Spuštění timeru
                jeStisknutaPauza = false;
            }
            else
            {
                vysivani.Styl = (StylVysivani)stylComboBox.SelectedIndex;
                vysivani.Vzor = (VzorVysivani)vzorComboBox.SelectedIndex;
                vysivani.VytvorPoleVzoru();
                vysivani.VycistiPlatno();
                vysivani.NastavPromennePodleStylu(0);
                vysivani.timer.Start();                   // Spuštění timeru
            }
            ZablokujVyber(blokujVse = true);
        }

        /// <summary>
        /// Pozastaví vyšívání
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PauzaButton_Click(object sender, RoutedEventArgs e)
        {
            if (!vysivani.timer.IsEnabled) // Kontrola, jestli je spuštěná instance DispatcherTimer
                return;
            vysivani.timer.Stop();
            jeStisknutaPauza = true;
            ledka.Fill = new SolidColorBrush(Colors.LightGray); // "Zhasne" Ledku
        }

        /// <summary>
        /// Zastaví vyšívání a obnoví kontrolky
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (!stylComboBox.IsEnabled) // Kontrola, jestli je aktivní kontrolka pro výběr stylu vyšívání
            {
                vysivani.timer.Stop();
                jeStisknutaPauza = false;
                vysivani.VycistiPlatno();
                vysivani.NastavPromennePodleStylu(0);
                OdblokujVyber();
                spravceOsob.JmenoPoslednihoVysivace = null;
                osobyListView.SelectedItem = null;   // "Odblokuje" opětovné spuštění SelectionChanged eventu při výběru stejné položky.                
            }
        }

        /// <summary>
        /// Výběr barvy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Barva_MouseDown(object sender, MouseButtonEventArgs e)
        {
            vysivani.VybranaBarvaStehu = ((Rectangle)sender).Fill;
        }

        /// <summary>
        /// Výběr plátna
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BarvaPlatnaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vysivani.BarvaPlatna = (BarvaPlatna)barvaPlatnaComboBox.SelectedIndex;
            vysivani.VycistiPlatno();
            vysivani.NastavPromennePodleStylu(0);
            if (vysivani.BarvaPlatna == BarvaPlatna.Bila)
                barvyStackPanel.Visibility = Visibility.Visible;
            else
                barvyStackPanel.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Uloží rozpracované/hotové plátno 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UlozButton_Click(object sender, RoutedEventArgs e)
        {
            // Kontrola, jestli byl vyšit alespoň jeden steh na plátno.
            if (platnoCanvas.Children.Count < vysivani.PocetSloupcu * vysivani.PocetRadku + 1)
            {
                MessageBox.Show("Je třeba vyšít alespoň jeden steh", "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (vysivani.timer.IsEnabled) // Kontrola, jestli je spuštěná instance DispatcherTimer
                PauzaButton_Click(this, null); // Kliknutí na tlačítko Pauza

            new ZadaniJmenaWindow(spravceOsob, vysivani.PocetStehuOdpocet, vysivani.PocetStehuCelkem, vysivani.Styl, vysivani.Vzor, vysivani.BarvaPlatna).ShowDialog();
        }

        /// <summary>
        /// Odebrání plátna ze seznamu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SmazButton_Click(object sender, RoutedEventArgs e)
        {
            if (vysivani.timer.IsEnabled) // Kontrola, jestli je spuštěná instance DispatcherTimer
                PauzaButton_Click(this, null); // Kliknutí na tlačítko Pauza

            osobyListView.Visibility = Visibility.Hidden; // Skryje seznam uložených pláten
            new SmazWindow(spravceOsob).ShowDialog();
            osobyListView.Visibility = Visibility.Visible; // Obnoví seznam uložených pláten
            // Pokud bylo smazáno plátno aktuálního vyšívače
            if (spravceOsob.JmenoPoslednihoVysivace != null)
            {
                // Pokud se jméno posledního (aktuálního) vyšívače z formuláře již nenachází v kolekci Osoby, je třeba ho odstranit i z formuláře.
                if (spravceOsob.Osoby.FirstOrDefault(c => c.Jmeno == spravceOsob.JmenoPoslednihoVysivace) == null)
                {
                    spravceOsob.JmenoPoslednihoVysivace = null;
                    vysivani.VycistiPlatno();
                    vysivani.NastavPromennePodleStylu(0);
                    OdblokujVyber();
                }
            }
        }

        /// <summary>
        /// Vybere osobu vyšívače ze seznamu uložených pláten, nastavení proměnných a obnovení plátna
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OsobyListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (osobyListView.SelectedItem != null)
            {
                Osoba vybranaOsoba = (Osoba)osobyListView.SelectedItem;

                vysivani.Styl = vybranaOsoba.StylVysivaniOsoby;
                stylComboBox.SelectedIndex = (int)vysivani.Styl;
                vysivani.Vzor = vybranaOsoba.VzorVysivaniOsoby;
                vzorComboBox.SelectedIndex = (int)vysivani.Vzor;
                vysivani.BarvaPlatna = vybranaOsoba.BarvaPlatnaOsoby;
                barvaPlatnaComboBox.SelectedIndex = (int)vysivani.BarvaPlatna;
                vysivani.VybranaBarvaStehu = vzorComboBox.SelectedIndex == 0 ? Brushes.Red : Brushes.Blue;

                vysivani.VytvorPoleVzoru();
                vysivani.VycistiPlatno();
                vysivani.NastavPromennePodleStylu(vybranaOsoba.PocetZbyvajicichStehu);
                vysivani.VysivejUlozenePlatno();
                jeStisknutaPauza = true;
                spravceOsob.JmenoPoslednihoVysivace = vybranaOsoba.Jmeno;
                ZablokujVyber(blokujVse = false);
            }
        }

        /// <summary>
        /// Zablokuje použití kontrolek pro výběr stylu, vzoru, barvy a uložených pláten
        /// </summary>
        private void ZablokujVyber(bool all)
        {
            stylComboBox.IsEnabled = false;
            vzorComboBox.IsEnabled = false;
            barvaPlatnaComboBox.IsEnabled = false;
            if (all)
                osobyListView.IsEnabled = false;
        }

        /// <summary>
        /// Odblokuje použití kontrolek pro výběr stylu, vzoru, barvy a uložených pláten
        /// </summary>
        private void OdblokujVyber()
        {
            stylComboBox.IsEnabled = true;
            vzorComboBox.IsEnabled = true;
            barvaPlatnaComboBox.IsEnabled = true;
            osobyListView.IsEnabled = true;
        }

        /// <summary>
        /// Obsluha události kliknutí na záhlaví sloupce daného ListView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OsobyListViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader sloupec = sender as GridViewColumnHeader; // Sloupec, na který uživatel kliknul
            string sortBy = sloupec.Tag.ToString(); // Tag = Property, podle kterého se má třídit
            if (osobyListViewSortSloupec != null) // Kontrola, jestli se podle tohoto sloupce už třídí => odstraní se trojúhelník a vyčistí se SortDescriptions
            {
                AdornerLayer.GetAdornerLayer(osobyListViewSortSloupec).Remove(osobyListViewSortAdorner);
                osobyListView.Items.SortDescriptions.Clear();
            }

            ListSortDirection novySmer = ListSortDirection.Ascending; // Volba směru
            if (osobyListViewSortSloupec == sloupec && osobyListViewSortAdorner.SmerTrideni == novySmer) // Pokud se právě podle daného sloupce třídilo, změní se směr
                novySmer = ListSortDirection.Descending;

            osobyListViewSortSloupec = sloupec;
            osobyListViewSortAdorner = new SortAdorner(osobyListViewSortSloupec, novySmer); // Nová instance SortAdorner s vybraným sloupcem a směrem třídění
            AdornerLayer.GetAdornerLayer(osobyListViewSortSloupec).Add(osobyListViewSortAdorner); // Instance se přidá do záhlaví vybraného sloupce
            osobyListView.Items.SortDescriptions.Add(new SortDescription(sortBy, novySmer)); // Přidá SortDescription do ListView => určí, podle čeho a ve kterém směru se třídí
        }
    }
}