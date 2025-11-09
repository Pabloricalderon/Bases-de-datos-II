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

            // Mostrar Ventas al abrir el menú
            this.Load += FmMenu_Load;

            // Navegación por botones (ajusta los nombres si difieren)
            btnVentas.Click += btnVentas_Click;
            btnClientes.Click += btnClientes_Click;
            btnProductos.Click += btnProductos_Click;
        }

        private void FmMenu_Load(object sender, EventArgs e) => AbrirVentas();

        private void btnVentas_Click(object sender, EventArgs e) => AbrirVentas();
        private void btnClientes_Click(object sender, EventArgs e) => LoadChild(new FmClientes());
        private void btnProductos_Click(object sender, EventArgs e) => LoadChild(new FmProductos());

        private void AbrirVentas() => LoadChild(new FmVentas());

        private void LoadChild(Form child)
        {
            if (_child != null)
            {
                _child.Close();
                _child.Dispose();
                _child = null;
            }

            _child = child;
            child.TopLevel = false;
            child.FormBorderStyle = FormBorderStyle.None;
            child.Dock = DockStyle.Fill;

            panelPrincipal.Controls.Clear();
            panelPrincipal.Controls.Add(child);
            child.Show();
            child.BringToFront();
        }
    }
}
