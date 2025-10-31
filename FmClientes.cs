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
    public partial class FmClientes : Form
    {
        private readonly ClientesRepo _repo = new ClientesRepo();

        public FmClientes()
        {
            InitializeComponent();
            this.Load += FmClientes_Load;

            txtBusqueda.TextChanged += (s, e) =>
                dgvClientes.DataSource = _repo.Listar(txtBusqueda.Text.Trim());

            btnNuevo.Click += (s, e) => { using (var f = new FmNuevoCliente()) { if (f.ShowDialog(this) == DialogResult.OK) Refrescar(); } };
            btnEditar.Click += (s, e) => EditarSel();
            btnEliminar.Click += (s, e) => EliminarSel();

            dgvClientes.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EditarSel(); };

            // Texto de placeholder para busqueda
            try
            {
                // Tu filtro actual busca por nombre y NIT
                txtBusqueda.SetPlaceholder("Buscar por nombre o NIT…");
            }
            catch
            {
                // Si no se muestra (multilinea, etc.)
                txtBusqueda.SetPlaceholderManaged("Buscar por nombre o NIT…");
            }

        }

        private void FmClientes_Load(object sender, EventArgs e) { Refrescar(); }
        private void Refrescar() { dgvClientes.DataSource = _repo.Listar(txtBusqueda.Text.Trim()); }

        private int? IdSel()
        {
            if (dgvClientes.CurrentRow == null) return null;
            var cell = dgvClientes.CurrentRow.Cells["id_cliente"];
            if (cell == null) return null;
            return Convert.ToInt32(cell.Value);
        }

        private void EditarSel()
        {
            var id = IdSel();
            if (id == null) { MessageBox.Show("Selecciona un cliente."); return; }
            using (var f = new FmEditarCliente(id.Value))
                if (f.ShowDialog(this) == DialogResult.OK) Refrescar();
        }

        private void EliminarSel()
        {
            var id = IdSel();
            if (id == null) { MessageBox.Show("Selecciona un cliente."); return; }
            if (MessageBox.Show("¿Eliminar cliente seleccionado?", "Confirmación",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try { _repo.Eliminar(id.Value); Refrescar(); }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

       
    }
}
