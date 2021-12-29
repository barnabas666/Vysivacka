using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Vysivacka
{
    /// <summary>
    /// Třída reprezentující osobu vyšívače
    /// </summary>
    public class Osoba
    {
        /// <summary>
        /// Jméno vyšívače
        /// </summary>
        public string Jmeno { get; set; }
        /// <summary>
        /// Počet stehů, které zbývá vyšít na plátně
        /// </summary>
        public int PocetZbyvajicichStehu { get; set; }
        /// <summary>
        /// Procento dokončení plátna. 100% = hotovo.
        /// </summary>
        public int StavDokonceni { get; set; }
        /// <summary>
        /// Enum s hodnotami pro různé styly vyšívání
        /// </summary>
        public StylVysivani StylVysivaniOsoby { get; set; }
        /// <summary>
        /// Enum s hodnotami pro různé vzory vyšívání
        /// </summary>
        public VzorVysivani VzorVysivaniOsoby { get; set; }
        /// <summary>
        /// Enum s hodnotami pro barvy plátna
        /// </summary>
        public BarvaPlatna BarvaPlatnaOsoby { get; set; }
        /// <summary>
        /// Vrátí obrázek podle zvoleného stylu vyšívání
        /// </summary>
        public string StylVysivaniImagePath
        {
            get
            {
                switch (StylVysivaniOsoby)
                {
                    case StylVysivani.Dolu:
                        return @"/Images/down_arrow_icon.png";
                    case StylVysivani.Nahoru:
                        return @"/Images/up_arrow_icon.png";
                    case StylVysivani.Doprava:
                        return @"/Images/right_arrow_icon.png";
                    case StylVysivani.Doleva:
                        return @"/Images/arrow_left_icon.png";
                    case StylVysivani.CikCakShora:
                        return @"/Images/arrow_down_right_icon.png";
                    case StylVysivani.CikCakZdola:
                        return @"/Images/arrow_right_up_icon.png";
                    case StylVysivani.Spirala:
                        return @"/Images/spiral_icon.png";
                    default:
                        return @"/Images/down_arrow_icon.png";
                }
            }
        }
        /// <summary>
        /// Vrátí obrázek podle zvoleného vzoru vyšívání
        /// </summary>
        public string VzorVysivaniImagePath
        {
            get
            {
                switch (VzorVysivaniOsoby)
                {
                    case VzorVysivani.CSharp:
                        return @"/Images/CSharp.png";
                    case VzorVysivani.Java:
                        return @"/Images/Java.png";
                    default:
                        return @"/Images/CSharp.png";
                }
            }
        }
        /// <summary>
        /// Vrátí barvu štětce podle zvolené barvy plátna
        /// </summary>
        public Brush BarvaPlatnaImagePath
        {
            get
            {
                switch (BarvaPlatnaOsoby)
                {
                    case BarvaPlatna.Bila:
                        return Brushes.White;
                    case BarvaPlatna.Modra:
                        return Brushes.Blue;
                    default:
                        return Brushes.White;
                }
            }
        }

        /// <summary>
        /// Zobrazení osoby pro případ, kdy není použita v xaml šablona.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Jmeno + ", " + StylVysivaniOsoby + ", " + VzorVysivaniOsoby + ", " + BarvaPlatnaOsoby + ", " + PocetZbyvajicichStehu + ", " + StavDokonceni;
        }
    }
}
