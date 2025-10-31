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
    public partial class FmEditarProducto : Form
    {
        private readonly ProductosRepo _repo = new ProductosRepo();
        private readonly int _id;

        public FmEditarProducto(int id)
        {
            InitializeComponent();
            _id = id;
            this.Load += FmEditarProducto_Load;
            btnGuardar.Click += btnGuardar_Click;
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            cboCategoria.SelectedIndexChanged += (s, e) => TogglePB();
        }

        private void FmEditarProducto_Load(object sender, EventArgs e)
        {
            // combos
            var cat = _repo.ListarCategorias();
            cboCategoria.DisplayMember = "nombre";
            cboCategoria.ValueMember = "id_categoria";
            cboCategoria.DataSource = cat;

            var uni = _repo.ListarUnidades();
            cboUnidad.DisplayMember = "nombre";
            cboUnidad.ValueMember = "id_unidad";
            cboUnidad.DataSource = uni;

            // datos
            var p = _repo.ObtenerPorId(_id);
            if (p == null) { MessageBox.Show("Producto no encontrado."); this.Close(); return; }

            cboCategoria.SelectedValue = p.IdCategoria;
            cboUnidad.SelectedValue = p.IdUnidad;
            txtNombre.Text = p.Nombre;
            txtDescripcion.Text = p.Descripcion;
            nudPrecio.Value = p.Precio;
            nudDescuento.Value = p.DescuentoPct;
            nudStock.Value = p.Stock;
            nudDuracion.Value = p.DuracionAnios.HasValue ? p.DuracionAnios.Value : 0;
            nudCobertura.Value = p.CoberturaM2.HasValue ? p.CoberturaM2.Value : 0;
            txtColor.Text = p.Color;
            chkActivo.Checked = p.Activo;

            TogglePB();
        }

        private bool EsPB()
        {
            var drv = cboCategoria.SelectedItem as DataRowView;
            var nom = drv == null ? "" : Convert.ToString(drv["nombre"]).ToLowerInvariant();
            return nom == "pintura" || nom == "barniz";
        }

        private void TogglePB()
        {
            bool req = EsPB();
            lblDuracion.Enabled = nudDuracion.Enabled = req;
            lblCobertura.Enabled = nudCobertura.Enabled = req;
            lblColor.Enabled = txtColor.Enabled = req;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                var p = new ProductoDto();
                p.IdProducto = _id;
                p.IdCategoria = Convert.ToInt32(cboCategoria.SelectedValue);
                p.IdUnidad = Convert.ToInt32(cboUnidad.SelectedValue);
                p.Nombre = txtNombre.Text.Trim();
                p.Descripcion = txtDescripcion.Text.Trim();
                p.Precio = nudPrecio.Value;
                p.DescuentoPct = nudDescuento.Value;
                p.Stock = nudStock.Value;
                p.Activo = chkActivo.Checked;

                if (EsPB())
                {
                    p.DuracionAnios = Convert.ToInt32(nudDuracion.Value);
                    p.CoberturaM2 = Convert.ToInt32(nudCobertura.Value);
                    p.Color = txtColor.Text.Trim();
                }
                else
                {
                    p.DuracionAnios = null;
                    p.CoberturaM2 = null;
                    p.Color = null;
                }

                _repo.Actualizar(p);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar: " + ex.Message);
            }
        }
    }
}
