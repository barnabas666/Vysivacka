using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Vysivacka
{
    /// <summary>
    /// Interaction logic for SmazWindow.xaml
    /// </summary>
    public partial class SmazWindow : Window
    {
        /// <summary>
        /// Instance správce osob
        /// </summary>
        private SpravceOsob spravceOsob;
        /// <summary>
        /// Určuje, podle kterého sloupce se třídí
        /// </summary>
        private GridViewColumnHeader smazListViewSortSloupec = null;
        /// <summary>
        /// Určuje, kde je umístěn trojúhelník pro třídění
        /// </summary>
        private SortAdorner smazListViewSortAdorner = null;

        /// <summary>
        /// Inicializuje formulář, získá správce osob
        /// </summary>
        /// <param name="spravceOsob">instance SpravceOsob</param>
        public SmazWindow(SpravceOsob spravceOsob)
        {
            InitializeComponent();
            this.spravceOsob = spravceOsob;
            DataContext = spravceOsob;
        }

        /// <summary>
        /// Zavře formulář
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZavriButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Vybere osobu vyšívače ze seznamu uložených pláten a nabídne možnost dané plátno smazat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SmazListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (smazListView.SelectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show($"Opravdu chceš smazat plátno vyšívače s jménem { ((Osoba)smazListView.SelectedItem).Jmeno }?", "Smaž", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        try
                        {
                            spravceOsob.Odeber((Osoba)smazListView.SelectedItem);
                            spravceOsob.UlozOsoby();
                            smazListView.SelectedItem = null;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        break;
                    case MessageBoxResult.No:
                        smazListView.SelectedItem = null;
                        break;
                }
            }
        }

        /// <summary>
        /// Obsluha události kliknutí na záhlaví sloupce daného ListView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SmazListViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader sloupec = sender as GridViewColumnHeader; // Sloupec, na který uživatel kliknul
            string sortBy = sloupec.Tag.ToString(); // Tag = Property, podle kterého se má třídit
            if (smazListViewSortSloupec != null) // Kontrola, jestli se podle tohoto sloupce už třídí => odstraní se trojúhelník a vyčistí se SortDescriptions
            {
                AdornerLayer.GetAdornerLayer(smazListViewSortSloupec).Remove(smazListViewSortAdorner);
                smazListView.Items.SortDescriptions.Clear();
            }

            ListSortDirection novySmer = ListSortDirection.Ascending; // Volba směru
            if (smazListViewSortSloupec == sloupec && smazListViewSortAdorner.SmerTrideni == novySmer) // Pokud se právě podle daného sloupce třídilo, změní se směr
                novySmer = ListSortDirection.Descending;

            smazListViewSortSloupec = sloupec;
            smazListViewSortAdorner = new SortAdorner(smazListViewSortSloupec, novySmer); // Nová instance SortAdorner s vybraným sloupcem a směrem třídění
            AdornerLayer.GetAdornerLayer(smazListViewSortSloupec).Add(smazListViewSortAdorner); // Instance se přidá do záhlaví vybraného sloupce
            smazListView.Items.SortDescriptions.Add(new SortDescription(sortBy, novySmer)); // Přidá SortDescription do ListView => určí, podle čeho a ve kterém směru se třídí
        }
    }
}
