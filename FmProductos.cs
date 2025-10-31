using Bases_de_datos_II.Capa_Datos;
using Bases_de_datos_II.Utils;
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
    public partial class FmProductos : Form
    {
        private readonly ProductosRepo _repo = new ProductosRepo();

        public FmProductos()
        {
            InitializeComponent();
            this.Load += (s, e) => Refrescar();

            txtBusqueda.TextChanged += (s, e) =>
                dgvProductos.DataSource = _repo.Listar(txtBusqueda.Text.Trim());

            btnNuevo.Click += (s, e) => { using (var f = new FmNuevoProducto()) { if (f.ShowDialog(this) == DialogResult.OK) Refrescar(); } };
            btnEditar.Click += (s, e) => EditarSel();
            btnEliminar.Click += (s, e) => EliminarSel();

            dgvProductos.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EditarSel(); };

            // Texto placeholder
            try
            {
                // Tu filtro actual: nombre, categoría o descripción
                txtBusqueda.SetPlaceholder("Buscar por nombre, categoría o descripción…");
            }
            catch
            {
                txtBusqueda.SetPlaceholderManaged("Buscar por nombre, categoría o descripción…");
            }


        }

        private void Refrescar() { dgvProductos.DataSource = _repo.Listar(txtBusqueda.Text.Trim()); }

        private int? IdSel()
        {
            if (dgvProductos.CurrentRow == null) return null;
            var cell = dgvProductos.CurrentRow.Cells["id_producto"];
            if (cell == null) return null;
            return Convert.ToInt32(cell.Value);
        }

        private void EditarSel()
        {
            var id = IdSel();
            if (id == null) { MessageBox.Show("Selecciona un producto."); return; }
            using (var f = new FmEditarProducto(id.Value))
                if (f.ShowDialog(this) == DialogResult.OK) Refrescar();
        }

        private void EliminarSel()
        {
            var id = IdSel();
            if (id == null) { MessageBox.Show("Selecciona un producto."); return; }

            var r = MessageBox.Show(
                "¿Qué deseas hacer?\n\nSí = Desactivar (recomendado)\nNo = Eliminar físicamente",
                "Producto", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            try
            {
                if (r == DialogResult.Yes) _repo.Desactivar(id.Value);
                else if (r == DialogResult.No) _repo.EliminarFisico(id.Value);
                else return;
                Refrescar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FmProductos_Load(object sender, EventArgs e)
        {
            dgvProductos.DataSource = _repo.Listar();
        }
    }
}
