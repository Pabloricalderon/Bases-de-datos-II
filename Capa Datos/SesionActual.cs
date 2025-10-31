using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bases_de_datos_II
{
    public class UsuarioSesion
    {
        public int IdUsuario { get; set; }
        public string Username { get; set; }  // puede quedar null en C# 7.3
        public int IdRol { get; set; }
        public string RolNombre { get; set; }
        public int? IdEmpleado { get; set; }
        public string EmpNombres { get; set; }
        public string EmpApellidos { get; set; }
    }

    public static class SessionActual
    {
        public static UsuarioSesion Usuario;

        public static bool Iniciada { get { return Usuario != null; } }

        public static void Iniciar(UsuarioSesion u) { Usuario = u; }
        public static void Cerrar() { Usuario = null; }

        public static bool EsGerente { get { return Usuario != null && Usuario.IdRol == 3; } }
        public static bool EsCajero { get { return Usuario != null && Usuario.IdRol == 2; } }
        public static bool EsDigitador { get { return Usuario != null && Usuario.IdRol == 1; } }
    }
}

