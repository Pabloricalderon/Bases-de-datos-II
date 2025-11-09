using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bases_de_datos_II.Utils
{
    public class LineaVenta : INotifyPropertyChanged
    {
        public int IdProducto { get; set; }
        public string Producto { get; set; }
        public string Unidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Stock { get; set; }

        private decimal _cantidad;
        public decimal Cantidad { get { return _cantidad; } set { _cantidad = value; OnChanged("Cantidad"); OnChanged("Subtotal"); } }

        private decimal _descPct;
        public decimal DescPct { get { return _descPct; } set { _descPct = value; OnChanged("DescPct"); OnChanged("Subtotal"); } }

        public decimal Subtotal { get { return Math.Round(Cantidad * Precio * (1 - (DescPct / 100m)), 2); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnChanged(string n) { var h = PropertyChanged; if (h != null) h(this, new PropertyChangedEventArgs(n)); }
    }

    public class PagoLinea
    {
        public string Metodo { get; set; }   // EFECTIVO/TARJETA/TRANSFERENCIA
        public decimal Monto { get; set; }
        public string Referencia { get; set; }
    }
}
