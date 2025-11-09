using Bases_de_datos_II.Capa_Datos;
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
    public partial class FmSeleccionProducto : Form   // <- partial
    {
        private readonly VentasRepo _ventas = new VentasRepo();
        private TextBox txtBuscar;
        private DataGridView dgv;
        private Button btnAceptar;
        private Button btnCancelar;

        public DataRow FilaSeleccionada { get; private set; }

        public FmSeleccionProducto()
        {
            InitializeComponent();                 // <- llama al Designer (aunque esté vacío)
            this.Text = "Seleccionar producto";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(820, 500);

            txtBuscar = new TextBox { Left = 12, Top = 12, Width = 670 };
            var lbl = new Label { Left = 12, Top = 40, Width = 340, Text = "Buscar por nombre o color" };

            dgv = new DataGridView
            {
                Left = 12,
                Top = 60,
                Width = 780,
                Height = 340,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            btnAceptar = new Button { Left = 592, Top = 410, Width = 90, Height = 30, Text = "Aceptar" };
            btnCancelar = new Button { Left = 702, Top = 410, Width = 90, Height = 30, Text = "Cancelar" };

            this.Controls.AddRange(new Control[] { txtBuscar, lbl, dgv, btnAceptar, btnCancelar });

            this.Load += (s, e) => Cargar();
            txtBuscar.TextChanged += (s, e) => Cargar(txtBuscar.Text.Trim());
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) Aceptar(); };
            btnAceptar.Click += (s, e) => Aceptar();
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
        }

        private void Cargar(string filtro = "")
        {
            var dt = _ventas.BuscarProductos(filtro);
            dgv.DataSource = dt;

            if (dgv.Columns.Contains("id_producto"))
                dgv.Columns["id_producto"].HeaderText = "ID";
            if (dgv.Columns.Contains("nombre"))
                dgv.Columns["nombre"].HeaderText = "Producto";
            if (dgv.Columns.Contains("unidad"))
                dgv.Columns["unidad"].HeaderText = "Unidad";
            if (dgv.Columns.Contains("precio"))
            {
                dgv.Columns["precio"].HeaderText = "Precio";
                dgv.Columns["precio"].DefaultCellStyle.Format = "N2";
            }
            if (dgv.Columns.Contains("stock"))
            {
                dgv.Columns["stock"].HeaderText = "Stock";
                dgv.Columns["stock"].DefaultCellStyle.Format = "N2";
            }
            if (dgv.Columns.Contains("descuento_pct"))
            {
                dgv.Columns["descuento_pct"].HeaderText = "Desc. %";
                dgv.Columns["descuento_pct"].DefaultCellStyle.Format = "N2";
            }
            if (dgv.Columns.Contains("color"))
                dgv.Columns["color"].HeaderText = "Color";
        }

        private void Aceptar()
        {
            var dt = dgv.DataSource as DataTable;
            if (dt == null || dgv.CurrentRow == null) return;
            FilaSeleccionada = (dgv.CurrentRow.DataBoundItem as DataRowView).Row;
            this.DialogResult = DialogResult.OK;
        }
    }
}
