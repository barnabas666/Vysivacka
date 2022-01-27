using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Vysivacka
{
    /// <summary>
    /// Třída s obslužnými metodami pro kolekci osob
    /// </summary>
    public class SpravceOsob : INotifyPropertyChanged
    {
        /// <summary>
        /// Cesta k souboru s daty
        /// </summary>
        private string cesta = "seznamOsob.xml";
        /// <summary>
        /// Jméno posledního (aktuálního) vyšívače.
        /// </summary>
        private string jmenoPoslednihoVysivace;
        public string JmenoPoslednihoVysivace
        {
            get => jmenoPoslednihoVysivace;
            set
            {
                jmenoPoslednihoVysivace = value;
                OnPropertyChanged(nameof(JmenoPoslednihoVysivace));
            }
        }
        private ObservableCollection<Osoba> osoby;
        /// <summary>
        /// Kolekce osob - vyšívačů
        /// </summary>
        public ObservableCollection<Osoba> Osoby
        {
            get => osoby;
            set
            {
                osoby = value;
                OnPropertyChanged(nameof(Osoby));
            }
        }
        /// <summary>
        /// Událost změny vlastnosti
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Aktualizuje prvek, na který je daná vlastnost nabindovaná. Vyvolá událost změny vlastnosti.
        /// </summary>
        /// <param name="propertyName">Název vlastnosti</param>
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Přidá / edituje novou osobu - vyšívače.
        /// </summary>
        /// <param name="pocetStehu"></param>
        public void PridejOsobu(string jmeno, int pocetStehu, int pocetStehuCelkem, StylVysivani styl, VzorVysivani vzor, BarvaPlatna barva)
        {
            // Edituje existující osobu vyšívače ze seznamu.            
            if (!string.IsNullOrEmpty(JmenoPoslednihoVysivace))
            {
                if (JmenoPoslednihoVysivace != jmeno)
                    throw new ArgumentException($"Tohle plátno patří vyšívači jménem { JmenoPoslednihoVysivace }. Tohle je vyšívačka, ne kopírka.");
                else
                {
                    var nalezenyVysivac = Osoby.FirstOrDefault(c => c.Jmeno == jmeno);
                    nalezenyVysivac.PocetZbyvajicichStehu = pocetStehu;
                    nalezenyVysivac.StavDokonceni = (int)Math.Round(((double)pocetStehuCelkem - pocetStehu) * 100 / pocetStehuCelkem);
                }
            }
            // Pokud je zadané jméno mimo povolený rozsah.
            else if (jmeno.Length < 3 || jmeno.Length > 20)
                throw new ArgumentException("Jméno musí být v rozsahu 3 - 20 znaků");
            // Přidá novou osobu vyšívače do seznamu.
            else
            {
                // Kontrola, jestli už není v seznamu vyšívač stejného jména.
                if (Osoby.FirstOrDefault(c => c.Jmeno == jmeno) != null)
                    throw new ArgumentException("Je třeba zvolit jiné jméno, tohle už je obsazené.");
                else
                {
                    JmenoPoslednihoVysivace = jmeno;
                    Osoby.Add(new Osoba
                    {
                        Jmeno = jmeno,
                        PocetZbyvajicichStehu = pocetStehu,
                        StavDokonceni = (int)Math.Round(((double)pocetStehuCelkem - pocetStehu) * 100 / pocetStehuCelkem),
                        StylVysivaniOsoby = styl,
                        VzorVysivaniOsoby = vzor,
                        BarvaPlatnaOsoby = barva
                    });
                }
            }
            Osoby = new ObservableCollection<Osoba>(Osoby.OrderBy(h => h.PocetZbyvajicichStehu));
        }

        /// <summary>
        /// Odebere osobu - vyšívače z kolekce
        /// </summary>
        /// <param name="osoba">Osoba k odebrání</param>
        public void Odeber(Osoba osoba)
        {
            Osoby.Remove(osoba);
        }

        /// <summary>
        /// Vrátí kolekci osob - vyšívačů ze souboru.
        /// </summary>
        public void NactiOsoby()
        {
            if (File.Exists(cesta))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Osoba>));

                using (StreamReader sr = new StreamReader(cesta))
                {
                    Osoby = (ObservableCollection<Osoba>)serializer.Deserialize(sr);
                }
            }
            else
                Osoby = new ObservableCollection<Osoba>();
        }

        /// <summary>
        /// Uloží kolekci osob - vyšívačů do souboru.
        /// </summary>
        public void UlozOsoby()
        {
            XmlSerializer serializer = new XmlSerializer(Osoby.GetType());

            using (StreamWriter sw = new StreamWriter(cesta))
            {
                serializer.Serialize(sw, Osoby);
            }
        }
    }
}
