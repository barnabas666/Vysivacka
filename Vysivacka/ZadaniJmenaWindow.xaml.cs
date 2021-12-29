using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Vysivacka
{
    /// <summary>
    /// Interaction logic for ZadaniJmenaWindow.xaml
    /// </summary>
    public partial class ZadaniJmenaWindow : Window
    {
        /// <summary>
        /// Instance správce osob
        /// </summary>
        private SpravceOsob spravceOsob;
        private int pocetStehuOdpocet;
        private int pocetStehuCelkem;
        private StylVysivani styl;
        private VzorVysivani vzor;
        private BarvaPlatna barva;

        /// <summary>
        /// Inicializuje formulář, získá správce osob a další proměnné potřebné k vytvoření nové osoby - vyšívače
        /// </summary>
        /// <param name="spravceOsob"></param>
        /// <param name="pocetStehuOdpocet"></param>
        /// <param name="pocetStehuCelkem"></param>
        /// <param name="styl"></param>
        /// <param name="vzor"></param>
        /// <param name="barva"></param>
        public ZadaniJmenaWindow(SpravceOsob spravceOsob, int pocetStehuOdpocet, int pocetStehuCelkem, StylVysivani styl, VzorVysivani vzor, BarvaPlatna barva)
        {
            InitializeComponent();
            this.spravceOsob = spravceOsob;
            this.pocetStehuOdpocet = pocetStehuOdpocet;
            this.pocetStehuCelkem = pocetStehuCelkem;
            this.styl = styl;
            this.vzor = vzor;
            this.barva = barva;
            jmenoTextBox.Text = spravceOsob.JmenoPoslednihoVysivace;
            FocusManager.SetFocusedElement(this, jmenoTextBox); // Umístí kurzor do TextBoxu pro zadávání jména            
        }

        /// <summary>
        /// Potvrzení formuláře
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                spravceOsob.PridejOsobu(jmenoTextBox.Text, pocetStehuOdpocet, pocetStehuCelkem, styl, vzor, barva);
                spravceOsob.UlozOsoby();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}
