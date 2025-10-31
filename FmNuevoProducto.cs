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
    public partial class FmNuevoProducto : Form
    {
        private readonly ProductosRepo _repo = new ProductosRepo();

        public FmNuevoProducto()
        {
            InitializeComponent();
            this.Load += FmNuevoProducto_Load;
            btnGuardar.Click += btnGuardar_Click;
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            cboCategoria.SelectedIndexChanged += (s, e) => ToggleCamposPinturaBarniz();
        }

        private void FmNuevoProducto_Load(object sender, EventArgs e)
        {
            // Categorías
            var dtC = _repo.ListarCategorias();
            cboCategoria.DisplayMember = "nombre";
            cboCategoria.ValueMember = "id_categoria";
            cboCategoria.DataSource = dtC;

            // Unidades (muestra todas; si quieres filtrar por categoría, aquí lo haces)
            var dtU = _repo.ListarUnidades();
            cboUnidad.DisplayMember = "nombre";
            cboUnidad.ValueMember = "id_unidad";
            cboUnidad.DataSource = dtU;

            ToggleCamposPinturaBarniz();
        }

        private bool EsPinturaOBarniz()
        {
            if (cboCategoria.SelectedItem == null) return false;
            var row = (cboCategoria.SelectedItem as DataRowView);
            var nombre = row != null ? (row["nombre"] as string) : null;
            if (nombre == null) return false;
            nombre = nombre.Trim().ToLowerInvariant();
            return nombre == "pintura" || nombre == "barniz";
        }

        private void ToggleCamposPinturaBarniz()
        {
            bool req = EsPinturaOBarniz();
            lblDuracion.Enabled = nudDuracion.Enabled = req;
            lblCobertura.Enabled = nudCobertura.Enabled = req;
            lblColor.Enabled = txtColor.Enabled = req;
        }

        private bool Validar(out string msg)
        {
            msg = "";
            if (cboCategoria.SelectedValue == null) { msg = "Seleccione una categoría."; return false; }
            if (cboUnidad.SelectedValue == null) { msg = "Seleccione una unidad."; return false; }
            if (string.IsNullOrWhiteSpace(txtNombre.Text)) { msg = "Nombre es obligatorio."; return false; }
            if (string.IsNullOrWhiteSpace(txtDescripcion.Text)) { msg = "Descripción es obligatoria."; return false; }
            if (nudPrecio.Value < 0) { msg = "Precio no puede ser negativo."; return false; }
            if (nudDescuento.Value < 0 || nudDescuento.Value > 100) { msg = "Descuento inválido."; return false; }
            if (nudStock.Value < 0) { msg = "Stock no puede ser negativo."; return false; }

            if (EsPinturaOBarniz())
            {
                if (nudDuracion.Value <= 0) { msg = "Duración (años) es obligatoria para Pintura/Barniz."; return false; }
                if (nudCobertura.Value <= 0) { msg = "Cobertura (m²) es obligatoria para Pintura/Barniz."; return false; }
                if (string.IsNullOrWhiteSpace(txtColor.Text)) { msg = "Color es obligatorio para Pintura/Barniz."; return false; }
            }
            return true;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                string m;
                if (!Validar(out m)) { MessageBox.Show(m); return; }

                int idCategoria = Convert.ToInt32(cboCategoria.SelectedValue);
                int idUnidad = Convert.ToInt32(cboUnidad.SelectedValue);
                string nombre = txtNombre.Text.Trim();
                string desc = txtDescripcion.Text.Trim();
                decimal precio = nudPrecio.Value;
                decimal descPct = nudDescuento.Value;
                decimal stock = nudStock.Value;

                int? dur = EsPinturaOBarniz() ? (int?)Convert.ToInt32(nudDuracion.Value) : null;
                int? cob = EsPinturaOBarniz() ? (int?)Convert.ToInt32(nudCobertura.Value) : null;
                string color = EsPinturaOBarniz() ? txtColor.Text.Trim() : null;

                int id = _repo.Insertar(idCategoria, idUnidad, nombre, desc, precio, descPct, stock, dur, cob, color);

                MessageBox.Show("Producto creado. ID: " + id);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo crear el producto: " + ex.Message);
            }
        }

        private void FmNuevoProducto_Load_1(object sender, EventArgs e)
        {

        }
    }
}
