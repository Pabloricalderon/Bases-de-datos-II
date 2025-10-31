using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bases_de_datos_II.Capa_Datos
{
    public class ClienteDto
    {
        public int IdCliente;
        public string Nit;
        public string Nombre;
        public string Direccion;
        public string Telefono;
        public string Correo;
    }

    public class ClientesRepo
    {
        public DataTable Listar(string filtro = "")
        {
            using (var cn = Db.GetConn())
            {
                cn.Open();
                const string sql = @"
SELECT id_cliente, nit, nombre, direccion, telefono, correo
FROM clientes
WHERE (@f = '' 
   OR nombre LIKE CONCAT('%', @f, '%')
   OR nit    LIKE CONCAT('%', @f, '%'))
ORDER BY id_cliente DESC;";
                using (var da = new MySqlDataAdapter(sql, cn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@f", filtro ?? "");
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public ClienteDto ObtenerPorId(int id)
        {
            using (var cn = Db.GetConn())
            {
                cn.Open();
                using (var cmd = new MySqlCommand(
                    @"SELECT id_cliente, nit, nombre, direccion, telefono, correo
                      FROM clientes WHERE id_cliente=@id", cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return null;
                        var dto = new ClienteDto();
                        dto.IdCliente = rd.GetInt32("id_cliente");
                        dto.Nit = rd.GetString("nit");
                        dto.Nombre = rd.GetString("nombre");
                        dto.Direccion = rd.GetString("direccion");
                        dto.Telefono = rd.IsDBNull(rd.GetOrdinal("telefono")) ? null : rd.GetString("telefono");
                        dto.Correo = rd.IsDBNull(rd.GetOrdinal("correo")) ? null : rd.GetString("correo");
                        return dto;
                    }
                }
            }
        }

        public void Actualizar(int id, string nit, string nombre, string direccion, string telefono, string correo)
        {
            using (var cn = Db.GetConn())
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        // evitar duplicado de NIT en otro cliente
                        using (var chk = new MySqlCommand(
                            @"SELECT COUNT(*) FROM clientes WHERE nit=@nit AND id_cliente<>@id;", cn, tx))
                        {
                            chk.Parameters.AddWithValue("@nit", nit);
                            chk.Parameters.AddWithValue("@id", id);
                            var n = Convert.ToInt32(chk.ExecuteScalar());
                            if (n > 0) throw new Exception("Ya existe otro cliente con ese NIT.");
                        }

                        using (var cmd = new MySqlCommand(
                            @"UPDATE clientes
                              SET nit=@nit, nombre=@nombre, direccion=@dir, telefono=@tel, correo=@cor
                              WHERE id_cliente=@id;", cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@nit", nit);
                            cmd.Parameters.AddWithValue("@nombre", nombre);
                            cmd.Parameters.AddWithValue("@dir", direccion);
                            cmd.Parameters.AddWithValue("@tel", string.IsNullOrWhiteSpace(telefono) ? (object)DBNull.Value : telefono);
                            cmd.Parameters.AddWithValue("@cor", string.IsNullOrWhiteSpace(correo) ? (object)DBNull.Value : correo);
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch (Exception)
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public void Eliminar(int id)
        {
            using (var cn = Db.GetConn())
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new MySqlCommand(
                            "DELETE FROM clientes WHERE id_cliente=@id;", cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }
                        tx.Commit();
                    }
                    catch (MySqlException ex)
                    {
                        tx.Rollback();
                        // 1451 = cannot delete or update a parent row (FK)
                        if (ex.Number == 1451)
                            throw new Exception("No se puede eliminar: el cliente ya tiene facturas relacionadas.");
                        throw;
                    }
                }
            }
        }
        public bool ExisteNit(string nit)
        {
            using (var cn = Db.GetConn())
            {
                cn.Open();
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM clientes WHERE nit=@nit;", cn))
                {
                    cmd.Parameters.AddWithValue("@nit", nit);
                    var n = Convert.ToInt32(cmd.ExecuteScalar());
                    return n > 0;
                }
            }
        }

        public int Insertar(string nit, string nombre, string direccion, string telefono, string correo)
        {
            using (var cn = Db.GetConn())
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        const string sql = @"
INSERT INTO clientes (nit, nombre, direccion, telefono, correo)
VALUES (@nit,@nombre,@direccion,@telefono,@correo);";

                        int id;
                        using (var cmd = new MySqlCommand(sql, cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@nit", nit);
                            cmd.Parameters.AddWithValue("@nombre", nombre);
                            cmd.Parameters.AddWithValue("@direccion", direccion);
                            cmd.Parameters.AddWithValue("@telefono",
                                string.IsNullOrWhiteSpace(telefono) ? (object)DBNull.Value : telefono);
                            cmd.Parameters.AddWithValue("@correo",
                                string.IsNullOrWhiteSpace(correo) ? (object)DBNull.Value : correo);

                            cmd.ExecuteNonQuery();
                            id = Convert.ToInt32(cmd.LastInsertedId); // ← AQUÍ está la clave
                        }

                        tx.Commit();
                        return id;
                    }
                    catch (MySqlException ex)
                    {
                        tx.Rollback();
                        if (ex.Number == 1062) // Duplicate entry
                            throw new Exception("Ya existe un cliente con ese NIT.");
                        throw;
                    }
                }
            }
        }
    }
}
