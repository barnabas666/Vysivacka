using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Vysivacka
{
    /// <summary>
    /// Třída zajišťující vyšívání
    /// </summary>
    class Vysivani : INotifyPropertyChanged
    {
        /// <summary>
        /// Aktuální sloupec a řádek plátna
        /// </summary>
        private int sloupec, radek;
        /// <summary>
        /// Kdy dojde k zastavení DispatcherTimer, nabývá hodnot od 0 (nové plátno) až do pocetStehuCelkem
        /// </summary>
        private int pocetStehuStop;
        /// <summary>
        /// Řídící proměnná pro běh algoritmů Dolu, Nahoru, Doleva a Doprava
        /// </summary>
        private int pocetStehuAlgoritmus;
        /// <summary>
        /// Souřadnice X, souřadnice Y plátna
        /// </summary>
        private int souradniceX, souradniceY;
        /// <summary>
        /// Směr vyšívání u algoritmů - doprava (doleva), případně dolů (nahoru) 
        /// </summary>
        private bool smerDoprava;
        /// <summary>
        /// Směr vyšívání u algoritmu spirály: 1 - doprava, 2 - dolů, 3 - doleva, 4 - nahoru
        /// </summary>
        private int smerSpiraly;
        /// <summary>
        /// Zarážky vymezující odkud - kam se vyšívá u spirály
        /// </summary>
        private int zarazkaHorni, zarazkaDolni, zarazkaLeva, zarazkaPrava;

        /// <summary>
        /// Událost změny vlastnosti
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Instance DispatcherTimer
        /// </summary>
        public DispatcherTimer timer;
        /// <summary>
        /// Pole vzorů (ornamentů) k vyšívání
        /// </summary>
        private bool[,] poleVzoru;
        /// <summary>
        /// Instance plátna Canvas
        /// </summary>
        private Canvas platnoCanvas;
        /// <summary>
        /// Instance objektu Ellipse - zde tzv. LED
        /// </summary>
        private Ellipse ledka;
        /// <summary>
        /// Enum s hodnotami pro různé styly vyšívání
        /// </summary>
        public StylVysivani Styl { get; set; }
        /// <summary>
        /// Enum s hodnotami pro různé vzory vyšívání
        /// </summary>
        public VzorVysivani Vzor { get; set; }
        /// <summary>
        /// Enum s hodnotami pro barvy plátna
        /// </summary>
        public BarvaPlatna BarvaPlatna { get; set; }
        /// <summary>
        /// Vybraná barva stehů
        /// </summary>
        public Brush VybranaBarvaStehu { get; set; }
        /// <summary>
        /// Celkový počet sloupců plátna
        /// </summary>
        public int PocetSloupcu { get; private set; }
        /// <summary>
        /// Celkový počet řádků plátna
        /// </summary>
        public int PocetRadku { get; private set; }
        /// <summary>
        /// Celkový počet stehů plátna
        /// </summary>
        public int PocetStehuCelkem { get; private set; }
        /// <summary>
        /// Velikost stehu
        /// </summary>
        public int VelikostStehu { get; private set; }

        private int pocetStehuOdpocet;
        /// <summary>
        /// Řídící proměnná pro běh DispatcherTimeru
        /// </summary>
        public int PocetStehuOdpocet
        {
            get => pocetStehuOdpocet;
            set
            {
                pocetStehuOdpocet = value;
                // Při maximální nebo nulové hodnotě se nastaví v xaml odpovídající TextBlock a vypne se Ledka
                if (pocetStehuOdpocet == 0 || pocetStehuOdpocet == PocetSloupcu * PocetRadku || pocetStehuOdpocet == PocetSloupcu * PocetRadku + PocetRadku / 2)
                {
                    OnPropertyChanged(nameof(PocetStehuOdpocet));
                    ledka.Fill = new SolidColorBrush(Colors.LightGray);
                }
                // Každých 20 stehů se vypíše do formuláře zbývající počet stehů a změní se hodnota vlastnosti LedkaBool (blikne)
                else if (pocetStehuOdpocet % 20 == 0 && timer.IsEnabled)
                {
                    OnPropertyChanged(nameof(PocetStehuOdpocet));
                    LedkaBool = !LedkaBool;
                }
            }
        }
        /// <summary>
        /// Řídící proměnná pro tzv. blikání LED
        /// </summary>
        public bool LedkaBool
        {
            get
            {
                Color barvaLedky = (ledka.Fill as SolidColorBrush).Color;
                return barvaLedky != Colors.LightGray;
            }
            set
            {
                ledka.Fill = value ? new SolidColorBrush(Colors.LimeGreen) : new SolidColorBrush(Colors.LightGray);
            }
        }

        /// <summary>
        /// Inicializuje instanci
        /// </summary>
        /// <param name="canvas">plátno canvas</param>
        /// <param name="led">LED ellipse</param>
        public Vysivani(Canvas canvas, Ellipse led)
        {
            platnoCanvas = canvas;
            ledka = led;
            PocetSloupcu = 81;
            PocetRadku = 41;
            VybranaBarvaStehu = Brushes.Red;
            BarvaPlatna = BarvaPlatna.Bila;
            NastavPromenne(0, 0, PocetSloupcu * PocetRadku, 0);
            VytvorPoleVzoru();
            VysivejUvodniPlatno();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1) // Nastavení intervalu
            };

            timer.Tick += Timer_Tick;        // Přiřazení metody, která se bude vykonávat v nastaveném intervalu na pozadí  
        }

        /// <summary>
        /// Aktualizuje prvek, na který je daná vlastnost nabindovaná. Vyvolá událost změny vlastnosti.
        /// </summary>
        /// <param name="propertyName">Název vlastnosti</param>
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Metoda, která se bude vykonávat v nastaveném intervalu na pozadí 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            VysivejStehy();
            PosunPodleAlgoritmu();
        }

        /// <summary>
        /// Zavolá metodu pro nastavení proměnných podle zvoleného stylu vyšívání.
        /// </summary>
        /// <param name="stylVysivani"></param>
        public void NastavPromennePodleStylu(int pocetStehuStopka)
        {
            switch (Styl)
            {
                case StylVysivani.Dolu:
                case StylVysivani.Doprava:
                case StylVysivani.CikCakShora:
                    NastavPromenne(0, 0, PocetSloupcu * PocetRadku, pocetStehuStopka);
                    break;
                case StylVysivani.Nahoru:
                    NastavPromenne(PocetRadku - 1, 0, PocetSloupcu * PocetRadku, pocetStehuStopka);
                    break;
                case StylVysivani.Doleva:
                    NastavPromenne(0, PocetSloupcu - 1, PocetSloupcu * PocetRadku, pocetStehuStopka);
                    break;
                case StylVysivani.CikCakZdola:
                    NastavPromenne(PocetRadku - 1, 0, PocetSloupcu * PocetRadku, pocetStehuStopka);
                    break;
                case StylVysivani.Spirala:
                    NastavPromenne(0, 0, PocetSloupcu * PocetRadku + PocetRadku / 2, pocetStehuStopka);
                    break;
            }
        }

        /// <summary>
        /// Nastavení proměnných nezbytných pro vyšívání
        /// </summary>
        /// <param name="nastavRadek">Nastaví počáteční řádek</param>
        /// <param name="nastavSloupec">Nastaví počáteční sloupec</param>
        /// <param name="countDown">Nastaví proměnnou pro odpočet</param>
        private void NastavPromenne(int nastavRadek, int nastavSloupec, int countDown, int pocetStehuStopka)
        {
            PocetSloupcu = 81;
            PocetRadku = 41;
            VelikostStehu = 10;
            sloupec = nastavSloupec;
            radek = nastavRadek;
            pocetStehuAlgoritmus = PocetSloupcu * PocetRadku;
            PocetStehuCelkem = PocetSloupcu * PocetRadku;
            PocetStehuOdpocet = countDown;
            pocetStehuStop = pocetStehuStopka;
            smerDoprava = true;
            smerSpiraly = 1;
            zarazkaHorni = 0;
            zarazkaDolni = PocetRadku - 1;
            zarazkaLeva = 0;
            zarazkaPrava = PocetSloupcu - 1;
        }

        /// <summary>
        /// Vytvoří pole vzorů (ornamentů). Hodnoty false mají barvu plátna, true zvolenou barvu stehů.
        /// </summary>
        public void VytvorPoleVzoru()
        {
            poleVzoru = new bool[PocetRadku, PocetSloupcu];
            for (int i = 0; i < poleVzoru.GetLength(0); i++)
                for (int j = 0; j < poleVzoru.GetLength(1); j++)
                {
                    // "Vykreslení" vnější linky
                    if (((i == 1 || i == PocetRadku - 2) && j > 4 && j < PocetSloupcu - 5) ||
                        ((i == 5 || i == PocetRadku - 6) && (j > 0 && j < 6 || j > PocetSloupcu - 7 && j < PocetSloupcu - 1)) ||
                        ((j == 1 || j == PocetSloupcu - 2) && i > 4 && i < PocetRadku - 5) ||
                        ((j == 5 || j == PocetSloupcu - 6) && (i > 0 && i < 6 || i > PocetRadku - 7 && i < PocetRadku - 1)))
                        poleVzoru[i, j] = true;

                    // "Vykreslení" 4 řad ornamentů
                    else if (i > 2 && i < 6 && j > 6 && j < PocetSloupcu - 7 ||
                        i > (PocetRadku - 7) && i < (PocetRadku - 3) && j > 6 && j < (PocetSloupcu - 7) ||
                        j > 2 && j < 6 && i > 6 && i < (PocetRadku - 7) ||
                        j > (PocetSloupcu - 7) && j < (PocetSloupcu - 3) && i > 6 && i < (PocetRadku - 7))
                        poleVzoru[i, j] = (i + j) % 2 == 0;

                    if (Vzor == VzorVysivani.CSharp)
                        VytvorPoleStehuCSharp(i, j);
                    else
                        VytvorPoleStehuJava(i, j);
                }
        }

        /// <summary>
        /// Doplní v poli stehů vnitřní symboly dle zvoleného stylu CSharp. Hodnoty false mají barvu plátna, true zvolenou barvu stehů.
        /// </summary>
        private void VytvorPoleStehuCSharp(int i, int j)
        {
            if ((i == 10 || i == 18 || i == 27) &&
               (j == 25 || j == 26 || j == 29 || j == 31 || j == 35 || j == 42 || j == 45 || j == 46 || j == 49 || j == 51 || j == 54 || j == 55) ||
               (i == 11 || i == 19 || i == 28) &&
               (j == 24 || (j > 27 && j < 33) || j == 36 || j == 42 || j == 45 || j == 46 || j == 49 || j == 51 || j == 54 || j == 55) ||
               (i == 12 || i == 20 || i == 29) &&
               (j == 24 || j == 29 || j == 31 || j == 37 || j == 42 || j == 45 || j == 46 || j == 49 || j == 51 || j == 54 || j == 55) ||
               (i == 13 || i == 21 || i == 30) &&
               (j == 24 || (j > 27 && j < 33) || j == 36 || j == 40 || j == 42 || j == 44 || j == 47 || j == 50 || j == 53 || j == 56) ||
               (i == 14 || i == 22 || i == 31) &&
               (j == 25 || j == 26 || j == 29 || j == 31 || j == 35 || j == 41 || j == 44 || j == 47 || j == 50 || j == 53 || j == 56))
                poleVzoru[i, j] = true;
        }

        /// <summary>
        /// Doplní v poli stehů vnitřní symboly dle zvoleného stylu Java. Hodnoty false mají barvu plátna, true zvolenou barvu stehů.
        /// </summary>
        private void VytvorPoleStehuJava(int i, int j)
        {
            if ((i == 10 || i == 18 || i == 27) &&
               (j == 22 || j == 23 || j == 26 || j == 28 || j == 32 || j == 35 || j == 38 || j == 45 || j == 48 || j == 49 || j == 52 || j == 54 || j == 57 || j == 58) ||
               (i == 11 || i == 19 || i == 28) &&
               (j == 21 || (j > 24 && j < 30) || j == 33 || j == 36 || j == 39 || j == 45 || j == 48 || j == 49 || j == 52 || j == 54 || j == 57 || j == 58) ||
               (i == 12 || i == 20 || i == 29) &&
               (j == 21 || j == 26 || j == 28 || j == 34 || j == 37 || j == 40 || j == 45 || j == 48 || j == 49 || j == 52 || j == 54 || j == 57 || j == 58) ||
               (i == 13 || i == 21 || i == 30) &&
               (j == 21 || (j > 24 && j < 30) || j == 33 || j == 36 || j == 39 || j == 43 || j == 45 || j == 47 || j == 50 || j == 53 || j == 56 || j == 59) ||
               (i == 14 || i == 22 || i == 31) &&
               (j == 22 || j == 23 || j == 26 || j == 28 || j == 32 || j == 35 || j == 38 || j == 44 || j == 47 || j == 50 || j == 53 || j == 56 || j == 59))
                poleVzoru[i, j] = true;
        }

        /// <summary>
        /// Přesune vyšívačku na pozici pro další steh
        /// </summary>
        private void PosunPodleAlgoritmu()
        {
            switch (Styl)
            {
                case StylVysivani.Dolu:
                    Dolu();
                    break;
                case StylVysivani.Nahoru:
                    Nahoru();
                    break;
                case StylVysivani.Doprava:
                    Doprava();
                    break;
                case StylVysivani.Doleva:
                    Doleva();
                    break;
                case StylVysivani.CikCakShora:
                    CikCakShora();
                    break;
                case StylVysivani.CikCakZdola:
                    CikCakZdola();
                    break;
                case StylVysivani.Spirala:
                    Spirala();
                    break;
            }
        }

        /// <summary>
        /// Vyšije jeden steh na plátno canvas
        /// </summary>
        public void VysivejStehy()
        {
            PocetStehuOdpocet--; // Odpočet od hodnoty pocetPixeluCountDown až k nule, kdy se zastaví DispatcherTimer
            if (PocetStehuOdpocet == 0)
                timer.Stop();               // Zastavení timeru

            souradniceX = sloupec * VelikostStehu;
            souradniceY = radek * VelikostStehu;

            Rectangle steh = new Rectangle();
            if (BarvaPlatna == BarvaPlatna.Bila)
                steh.Fill = poleVzoru[radek, sloupec] ? VybranaBarvaStehu : Brushes.White;
            else
                steh.Fill = poleVzoru[radek, sloupec] ? Brushes.White : Brushes.Blue;

            PridejStehNaPlatno(steh);
        }

        /// <summary>
        /// Vyšije uložené rozpracované plátno
        /// </summary>
        public void VysivejUlozenePlatno()
        {
            do
            {
                PocetStehuOdpocet--; // Odpočet od hodnoty pocetPixeluCountDown až k pocetStehuStop
                souradniceX = sloupec * VelikostStehu;
                souradniceY = radek * VelikostStehu;

                Rectangle steh = new Rectangle();
                if (BarvaPlatna == BarvaPlatna.Bila)
                    steh.Fill = poleVzoru[radek, sloupec] ? VybranaBarvaStehu : Brushes.White;
                else
                    steh.Fill = poleVzoru[radek, sloupec] ? Brushes.White : Brushes.Blue;

                PridejStehNaPlatno(steh);
                PosunPodleAlgoritmu();
            } while (PocetStehuOdpocet > pocetStehuStop);
            OnPropertyChanged(nameof(PocetStehuOdpocet)); // Po vykreslení uloženého plátna obnoví kontrolku s počtem zbývajících stehů.
        }

        /// <summary>
        /// Vyšije úvodní plátno na canvas, které je mřížkované kvůli odlišení při vyšívání
        /// </summary>       
        public void VysivejUvodniPlatno()
        {
            for (int i = 0; i < poleVzoru.GetLength(0); i++)
                for (int j = 0; j < poleVzoru.GetLength(1); j++)
                {
                    souradniceX = j * VelikostStehu;
                    souradniceY = i * VelikostStehu;

                    Rectangle steh = new Rectangle();
                    steh.Fill = BarvaPlatna == BarvaPlatna.Bila ? Brushes.White : Brushes.Blue;
                    steh.StrokeThickness = 0.15;
                    steh.Stroke = Brushes.Black;

                    PridejStehNaPlatno(steh);
                }
        }

        /// <summary>
        /// Nastaví velikost stehu a přidá ho na plátno.
        /// </summary>
        /// <param name="steh">objekt typu Rectangle</param>
        private void PridejStehNaPlatno(Rectangle steh)
        {
            steh.Width = VelikostStehu;
            steh.Height = VelikostStehu;

            platnoCanvas.Children.Add(steh);

            Canvas.SetLeft(steh, souradniceX);
            Canvas.SetTop(steh, souradniceY);
        }

        /// <summary>
        /// Smaže a znovu vyšije úvodní plátno na canvas
        /// </summary>
        public void VycistiPlatno()
        {
            platnoCanvas.Children.Clear();
            VysivejUvodniPlatno();
        }

        #region Algoritmy
        /// <summary>
        /// Algoritmus pro vyšívání stehů odshora dolů
        /// </summary>
        private void Dolu()
        {
            if (PocetStehuOdpocet % PocetSloupcu == 0) // Po vykreslení každého řádku se odřádkuje a změní směr vyšívání
            {
                smerDoprava = !smerDoprava;
                pocetStehuAlgoritmus -= PocetSloupcu; // Hodnota se snižuje od hodnoty PocetStehuAlgoritmus až k nule (zde má každý skok velikost PocetSloupcu)
                radek++; // Hodnota roste od nuly až do pocetRadku
            }
            if (smerDoprava)
            {
                sloupec = pocetStehuAlgoritmus - PocetStehuOdpocet; // Hodnota roste od nuly až po PocetSloupcu 
            }
            else
            {
                sloupec = PocetStehuOdpocet - pocetStehuAlgoritmus - 1 + PocetSloupcu; // Hodnota klesá od PocetSloupcu až do nuly
            }
        }

        /// <summary>
        /// Algoritmus pro vyšívání stehů zdola nahoru
        /// </summary>
        private void Nahoru()
        {
            if (PocetStehuOdpocet % PocetSloupcu == 0) // Po vykreslení každého řádku se odřádkuje a změní směr vyšívání
            {
                smerDoprava = !smerDoprava;
                pocetStehuAlgoritmus -= PocetSloupcu; // Hodnota se snižuje od hodnoty PocetStehuAlgoritmus až k nule (zde má každý skok velikost PocetSloupcu)
                radek--; // Hodnota klesá od pocetRadku až do nuly
            }
            if (smerDoprava)
            {
                sloupec = pocetStehuAlgoritmus - PocetStehuOdpocet; // Hodnota roste od nuly až po PocetSloupcu
            }
            else
            {
                sloupec = PocetStehuOdpocet - pocetStehuAlgoritmus - 1 + PocetSloupcu; // Hodnota klesá od PocetSloupcu až do nuly
            }
        }

        /// <summary>
        /// Algoritmus pro vyšívání stehů zleva doprava
        /// </summary>
        private void Doprava()
        {
            if (PocetStehuOdpocet % PocetRadku == 0) // Po vykreslení každého sloupce se odsloupcuje a změní směr vyšívání
            {
                smerDoprava = !smerDoprava;
                pocetStehuAlgoritmus -= PocetRadku; // Hodnota se snižuje od hodnoty PocetStehuAlgoritmus až k nule (zde má každý skok velikost PocetRadku)
                sloupec++; // Hodnota roste od nuly až po pocetSloupcu
            }
            if (smerDoprava)
            {
                radek = pocetStehuAlgoritmus - PocetStehuOdpocet; // Hodnota roste od nuly až po PocetRadku 
            }
            else
            {
                radek = PocetStehuOdpocet - pocetStehuAlgoritmus - 1 + PocetRadku; // Hodnota klesá od PocetRadku až do nuly
            }
        }

        /// <summary>
        /// Algoritmus pro vyšívání stehů zprava doleva
        /// </summary>
        private void Doleva()
        {
            if (PocetStehuOdpocet % PocetRadku == 0) // Po vykreslení každého sloupce (zde 50 pixelů) se odsloupcuje a změní směr vyšívání
            {
                smerDoprava = !smerDoprava;
                pocetStehuAlgoritmus -= PocetRadku; // Hodnota se snižuje od hodnoty PocetStehuAlgoritmus až k nule (zde má každý skok velikost PocetRadku)
                sloupec--; // Hodnota roste od nuly až po pocetSloupcu
            }
            if (smerDoprava)
            {
                radek = pocetStehuAlgoritmus - PocetStehuOdpocet; // Hodnota roste od nuly až po PocetRadku 
            }
            else
            {
                radek = PocetStehuOdpocet - pocetStehuAlgoritmus - 1 + PocetRadku; // Hodnota klesá od PocetRadku až do nuly
            }
        }

        /// <summary>
        /// Algoritmus pro vyšívání stehů odshora ve směru hlavní diagonály matice, dílčí posuny probíhají po vedlejších diagonálách.
        /// </summary>
        private void CikCakShora()
        {
            if (smerDoprava)
            {
                // Posun po vedlejší diagonále
                if (radek > 0 && sloupec < PocetSloupcu - 1)
                {
                    radek--;
                    sloupec++;
                }
                // Narazí se na poslední sloupec, změna směru
                else if (sloupec == PocetSloupcu - 1)
                {
                    radek++;
                    smerDoprava = !smerDoprava;
                }
                // Narazí se na poslední řádek, změna směru
                else
                {
                    sloupec++;
                    smerDoprava = !smerDoprava;
                }
            }
            else
            {
                if (sloupec > 0 && radek < PocetRadku - 1)
                {
                    radek++;
                    sloupec--;
                }
                else if (radek == PocetRadku - 1)
                {
                    sloupec++;
                    smerDoprava = !smerDoprava;
                }
                else
                {
                    radek++;
                    smerDoprava = !smerDoprava;
                }
            }
        }

        /// <summary>
        /// Algoritmus pro vyšívání stehů zdola ve směru vedlejší diagonály matice, dílčí posuny probíhají po hlavních diagonálách.
        /// </summary>
        private void CikCakZdola()
        {
            if (smerDoprava)
            {
                // Posun po hlavní diagonále
                if (radek < PocetRadku - 1 && sloupec < PocetSloupcu - 1)
                {
                    radek++;
                    sloupec++;
                }
                // Narazí se na poslední sloupec, změna směru
                else if (sloupec == PocetSloupcu - 1)
                {
                    radek--;
                    smerDoprava = !smerDoprava;
                }
                // Narazí se na poslední řádek, změna směru
                else
                {
                    sloupec++;
                    smerDoprava = !smerDoprava;
                }
            }
            else
            {
                if (radek > 0 && sloupec > 0)
                {
                    radek--;
                    sloupec--;
                }
                else if (radek == 0)
                {
                    sloupec++;
                    smerDoprava = !smerDoprava;
                }
                else
                {
                    radek--;
                    smerDoprava = !smerDoprava;
                }
            }
        }

        /// <summary>
        /// Algoritmus pro vyšívání stehů ve směru spirály
        /// </summary>
        private void Spirala()
        {
            // Směr doprava
            if (smerSpiraly == 1)
            {
                if (sloupec < zarazkaPrava)
                {
                    sloupec++;
                }
                else
                {
                    zarazkaHorni++;
                    smerSpiraly = 2;
                }
            }
            // Směr dolů
            if (smerSpiraly == 2)
            {
                if (radek < zarazkaDolni)
                {
                    radek++;
                }
                else
                {
                    zarazkaPrava--;
                    smerSpiraly = 3;
                }
            }
            // Směr doleva
            if (smerSpiraly == 3)
            {
                if (sloupec > zarazkaLeva)
                {
                    sloupec--;
                }
                else
                {
                    zarazkaDolni--;
                    smerSpiraly = 4;
                }
            }
            // Směr nahoru
            if (smerSpiraly == 4)
            {
                if (radek > zarazkaHorni)
                {
                    radek--;
                }
                else
                {
                    zarazkaLeva++;
                    smerSpiraly = 1;
                }
            }
            #endregion
        }
    }
}
