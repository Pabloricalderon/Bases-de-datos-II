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
    public partial class FmEditarCliente : Form
    {
        private readonly ClientesRepo _repo = new ClientesRepo();
        private readonly int _id;

        public FmEditarCliente(int id)
        {
            InitializeComponent();
            _id = id;
            this.Load += FmEditarCliente_Load;
            btnGuardar.Click += btnGuardar_Click;
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
        }

        private void FmEditarCliente_Load(object sender, EventArgs e)
        {
            var c = _repo.ObtenerPorId(_id);
            if (c == null) { MessageBox.Show("Cliente no encontrado."); this.Close(); return; }
            txtNit.Text = c.Nit;
            txtNombre.Text = c.Nombre;
            txtDireccion.Text = c.Direccion;
            txtTelefono.Text = c.Telefono;
            txtCorreo.Text = c.Correo;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                _repo.Actualizar(
                    _id,
                    txtNit.Text.Trim(),
                    txtNombre.Text.Trim(),
                    txtDireccion.Text.Trim(),
                    txtTelefono.Text.Trim(),
                    txtCorreo.Text.Trim()
                );
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar: " + ex.Message);
            }
        }
    }
}
