using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bases_de_datos_II
{
    public partial class FmMenu : Form
    {
        private Form _child;

        public FmMenu()
        {
            InitializeComponent();

            // Ajusta estos nombres a tus botones reales
            btnClientes.Click += (s, e) => LoadChild(new FmClientes());
            btnProductos.Click += (s, e) => LoadChild(new FmProductos());
        }

        private void LoadChild(Form child)
        {
            if (_child != null)
            {
                _child.Close();
                _child.Dispose();
            }

            _child = child;
            child.TopLevel = false;
            child.FormBorderStyle = FormBorderStyle.None;
            child.Dock = DockStyle.Fill;

            panelPrincipal.Controls.Clear();   // asegúrate que el panel se llame así
            panelPrincipal.Controls.Add(child);
            child.Show();
        }
    }
}
