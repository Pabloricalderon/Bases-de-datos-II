using Bases_de_datos_II.Capa_Datos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bases_de_datos_II
{
    public partial class FmNuevoCliente : Form
    {
        private readonly ClientesRepo _repo = new ClientesRepo();

        public FmNuevoCliente()
        {
            InitializeComponent();
            btnGuardar.Click += btnGuardar_Click;
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
        }

        private bool Validar(out string msg)
        {
            msg = "";
            if (string.IsNullOrWhiteSpace(txtNit.Text)) { msg = "NIT es obligatorio."; return false; }
            if (string.IsNullOrWhiteSpace(txtNombre.Text)) { msg = "Nombre es obligatorio."; return false; }
            if (string.IsNullOrWhiteSpace(txtDireccion.Text)) { msg = "Dirección es obligatoria."; return false; }
            if (!string.IsNullOrWhiteSpace(txtCorreo.Text))
            {
                var ok = Regex.IsMatch(txtCorreo.Text.Trim(), @"^\S+@\S+\.\S+$");
                if (!ok) { msg = "Correo no válido."; return false; }
            }
            return true;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                string m;
                if (!Validar(out m)) { MessageBox.Show(m); return; }

                var id = _repo.Insertar(
                    txtNit.Text.Trim(),
                    txtNombre.Text.Trim(),
                    txtDireccion.Text.Trim(),
                    txtTelefono.Text.Trim(),
                    txtCorreo.Text.Trim());

                MessageBox.Show("Cliente creado. ID: " + id);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo crear el cliente: " + ex.Message);
            }
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
