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
    public partial class FmSeleccionCliente : Form   // <- partial
    {
        private readonly ClientesRepo _repo = new ClientesRepo();
        private TextBox txtBuscar;
        private DataGridView dgv;
        private Button btnAceptar;
        private Button btnCancelar;

        public int IdClienteSeleccionado { get; private set; }
        public string NombreSeleccionado { get; private set; }

        public FmSeleccionCliente()
        {
            InitializeComponent();                 // <- llama al Designer (aunque esté vacío)
            this.Text = "Seleccionar cliente";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(720, 460);

            txtBuscar = new TextBox { Left = 12, Top = 12, Width = 560 };
            var lbl = new Label { Left = 12, Top = 40, Width = 200, Text = "Buscar por nombre o NIT" };

            dgv = new DataGridView
            {
                Left = 12,
                Top = 60,
                Width = 680,
                Height = 300,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            btnAceptar = new Button { Left = 492, Top = 370, Width = 90, Height = 30, Text = "Aceptar" };
            btnCancelar = new Button { Left = 602, Top = 370, Width = 90, Height = 30, Text = "Cancelar" };

            this.Controls.AddRange(new Control[] { txtBuscar, lbl, dgv, btnAceptar, btnCancelar });

            this.Load += (s, e) => Cargar();
            txtBuscar.TextChanged += (s, e) => Cargar(txtBuscar.Text.Trim());
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) Aceptar(); };
            btnAceptar.Click += (s, e) => Aceptar();
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
        }

        private void Cargar(string filtro = "")
        {
            dgv.DataSource = _repo.Listar(filtro);
            if (dgv.Columns.Contains("id_cliente"))
                dgv.Columns["id_cliente"].HeaderText = "ID";
            if (dgv.Columns.Contains("nit"))
                dgv.Columns["nit"].HeaderText = "NIT";
            if (dgv.Columns.Contains("nombre"))
                dgv.Columns["nombre"].HeaderText = "Nombre";
            if (dgv.Columns.Contains("direccion"))
                dgv.Columns["direccion"].HeaderText = "Dirección";
            if (dgv.Columns.Contains("telefono"))
                dgv.Columns["telefono"].HeaderText = "Teléfono";
            if (dgv.Columns.Contains("correo"))
                dgv.Columns["correo"].HeaderText = "Correo";
        }

        private void Aceptar()
        {
            if (dgv.CurrentRow == null) return;
            var row = dgv.CurrentRow;
            IdClienteSeleccionado = Convert.ToInt32(row.Cells["id_cliente"].Value);
            var nombre = Convert.ToString(row.Cells["nombre"].Value);
            var nit = Convert.ToString(row.Cells["nit"].Value);
            NombreSeleccionado = string.IsNullOrWhiteSpace(nit) ? nombre : (nombre + "  (" + nit + ")");
            this.DialogResult = DialogResult.OK;
        }
    }
}
