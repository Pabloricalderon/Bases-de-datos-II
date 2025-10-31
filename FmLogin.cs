using Bases_de_datos_II.Capa_Datos;
using MySqlConnector;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Bases_de_datos_II
{
    public partial class FmLogin : Form
    {
        public FmLogin()
        {
            InitializeComponent();
            btnIngresar.Click += btnIngresar_Click;
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            try
            {
                string user = txtUsuario.Text.Trim();
                string pass = txtPassword.Text;

                if (string.IsNullOrWhiteSpace(user) || string.IsNullOrEmpty(pass))
                {
                    MessageBox.Show("Ingrese usuario y contraseña.");
                    return;
                }

                using (var cn = Db.GetConn())
                {
                    cn.Open();
                    using (var cmd = new MySqlCommand(@"
SELECT id_usuario, username, salt, password_hash, activo,
       id_rol, rol, id_empleado, emp_nombres, emp_apellidos
FROM vw_login_usuarios
WHERE username=@u
LIMIT 1;", cn))
                    {
                        cmd.Parameters.AddWithValue("@u", user);
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (!rd.Read()) { MessageBox.Show("Usuario o contraseña inválidos."); return; }
                            if (rd.GetInt32(rd.GetOrdinal("activo")) == 0) { MessageBox.Show("Usuario inactivo."); return; }

                            string salt = rd.GetString(rd.GetOrdinal("salt"));
                            // 1) calculo del hash local
                            byte[] input = Encoding.UTF8.GetBytes(salt + pass);
                            byte[] hashLocal;
                            using (var sha = SHA256.Create())
                                hashLocal = sha.ComputeHash(input);

                            // 2) lectura genérica desde BD (puede venir como byte[] o como string)
                            object raw = rd["password_hash"];
                            bool ok = false;

                            if (raw is byte[])
                            {
                                var buf = (byte[])raw;
                                if (buf.Length == 32)
                                {
                                    // A) BD guarda BINARIO puro (UNHEX(SHA2(...)))
                                    ok = hashLocal.SequenceEqual(buf);
                                }
                                else if (buf.Length == 64)
                                {
                                    // B) BD guarda TEXTO HEX en un tipo binario (pasa a string)
                                    string hexDb = Encoding.ASCII.GetString(buf).TrimEnd('\0');
                                    string hexLocal = BitConverter.ToString(hashLocal).Replace("-", "").ToLowerInvariant();
                                    ok = string.Equals(hexDb, hexLocal, StringComparison.OrdinalIgnoreCase);
                                }
                            }
                            else
                            {
                                // B) BD guarda TEXTO HEX (CHAR/VARCHAR)
                                string hexDb = Convert.ToString(raw);
                                string hexLocal = BitConverter.ToString(hashLocal).Replace("-", "").ToLowerInvariant();
                                ok = string.Equals(hexDb, hexLocal, StringComparison.OrdinalIgnoreCase);
                            }

                            if (!ok)
                            {
                                MessageBox.Show("Usuario o contraseña inválidos.");
                                return;
                            }


                            var ses = new UsuarioSesion();
                            ses.IdUsuario = rd.GetInt32(rd.GetOrdinal("id_usuario"));
                            ses.Username = rd.GetString(rd.GetOrdinal("username"));
                            ses.IdRol = rd.GetInt32(rd.GetOrdinal("id_rol"));
                            ses.RolNombre = rd.GetString(rd.GetOrdinal("rol"));

                            int ordEmp = rd.GetOrdinal("id_empleado");
                            ses.IdEmpleado = rd.IsDBNull(ordEmp) ? (int?)null : rd.GetInt32(ordEmp);

                            int ordN = rd.GetOrdinal("emp_nombres");
                            ses.EmpNombres = rd.IsDBNull(ordN) ? null : rd.GetString(ordN);
                            int ordA = rd.GetOrdinal("emp_apellidos");
                            ses.EmpApellidos = rd.IsDBNull(ordA) ? null : rd.GetString(ordA);

                            SessionActual.Iniciar(ses);
                        }
                    }
                }

                var menu = new FmMenu();
                menu.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en login: " + ex.Message);
            }
        }
    }
}
