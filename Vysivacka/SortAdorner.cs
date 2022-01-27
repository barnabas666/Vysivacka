using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

// https://wpf-tutorial.com/listview-control/listview-how-to-column-sorting/

namespace Vysivacka
{
    /// <summary>
    /// Třída sloužící k třídění dat v kolekci a vykreslení symbolu trojúhelníku
    /// </summary>
    class SortAdorner : Adorner
    {
        // Vykreslí trojúhelník se šipkou nahoru - vzestupně
        private static Geometry ascGeometry = Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");

        private static Geometry descGeometry = Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");
        /// <summary>
        /// Směr třídění
        /// </summary>
        public ListSortDirection SmerTrideni { get; private set; }

        public SortAdorner(UIElement element, ListSortDirection smer)
            : base(element)
        {
            this.SmerTrideni = smer;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (AdornedElement.RenderSize.Width < 20)
                return;

            TranslateTransform transform = new TranslateTransform
                (
                    AdornedElement.RenderSize.Width - 10,
                    (AdornedElement.RenderSize.Height - 5) / 2
                );
            drawingContext.PushTransform(transform);

            Geometry geometry = ascGeometry;
            if (this.SmerTrideni == ListSortDirection.Descending)
                geometry = descGeometry;
            drawingContext.DrawGeometry(Brushes.Black, null, geometry);

            drawingContext.Pop();
        }
    }
}
