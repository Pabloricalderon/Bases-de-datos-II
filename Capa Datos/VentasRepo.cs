using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bases_de_datos_II.Capa_Datos
{
    public class VentasRepo
    {
        public int SiguienteNumero(string serie)
        {
            using (var cn = Db.GetConn())
            {
                cn.Open();
                using (var cmd = new MySqlCommand(
                    "SELECT IFNULL(MAX(numero)+1, 1) FROM facturas WHERE serie=@s;", cn))
                {
                    cmd.Parameters.AddWithValue("@s", serie);
                    var obj = cmd.ExecuteScalar();
                    return Convert.ToInt32(obj);
                }
            }
        }


        public int CrearFactura(string serie, int numero, int idCliente, int idUsuario, MySqlConnection cn, MySqlTransaction tx)
        {
            using (var cmd = new MySqlCommand(
                @"INSERT INTO facturas (serie, numero, id_cliente, id_usuario)
          VALUES (@s, @n, @c, @u);", cn, tx))
            {
                cmd.Parameters.AddWithValue("@s", serie);
                cmd.Parameters.AddWithValue("@n", numero);
                cmd.Parameters.AddWithValue("@c", idCliente);
                cmd.Parameters.AddWithValue("@u", idUsuario);
                cmd.ExecuteNonQuery();
            }

            using (var idCmd = new MySqlCommand("SELECT LAST_INSERT_ID();", cn, tx))
                return Convert.ToInt32(idCmd.ExecuteScalar());
        }


        public void AgregarDetalle(int idFactura, int idProducto, decimal cantidad, decimal precio, decimal descPct, MySqlConnection cn, MySqlTransaction tx)
        {
            using (var cmd = new MySqlCommand(
                @"INSERT INTO factura_detalle (id_factura, id_producto, cantidad, precio_unitario, descuento_pct)
          VALUES (@f, @p, @c, @pr, @d);", cn, tx))
            {
                cmd.Parameters.AddWithValue("@f", idFactura);
                cmd.Parameters.AddWithValue("@p", idProducto);
                cmd.Parameters.AddWithValue("@c", cantidad);
                cmd.Parameters.AddWithValue("@pr", precio);
                cmd.Parameters.AddWithValue("@d", descPct);
                cmd.ExecuteNonQuery();
            }
        }


        public void RegistrarPago(int idFactura, string metodo, decimal monto, string referencia, MySqlConnection cn, MySqlTransaction tx)
        {
            using (var cmd = new MySqlCommand("CALL sp_registrar_pago(@f,@m,@mon,@ref);", cn, tx))
            {
                cmd.Parameters.AddWithValue("@f", idFactura);
                cmd.Parameters.AddWithValue("@m", metodo);
                cmd.Parameters.AddWithValue("@mon", monto);
                cmd.Parameters.AddWithValue("@ref", string.IsNullOrWhiteSpace(referencia) ? (object)DBNull.Value : referencia);
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable BuscarProductos(string filtro)
        {
            using (var cn = Db.GetConn())
            {
                cn.Open();
                const string sql = @"
SELECT p.id_producto, p.nombre, u.nombre AS unidad, p.precio, p.stock, p.descuento_pct, p.color
FROM productos p
JOIN unidades u ON u.id_unidad = p.id_unidad
WHERE p.activo=1 AND (
      @f = '' OR p.nombre LIKE CONCAT('%',@f,'%') OR p.color LIKE CONCAT('%',@f,'%')
)
ORDER BY p.nombre;";
                using (var da = new MySqlDataAdapter(sql, cn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@f", filtro ?? "");
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public DataRow ProductoPorId(int idProducto)
        {
            using (var cn = Db.GetConn())
            {
                cn.Open();
                using (var da = new MySqlDataAdapter(@"
SELECT p.id_producto, p.nombre, u.nombre AS unidad, p.precio, p.stock, p.descuento_pct
FROM productos p
JOIN unidades u ON u.id_unidad = p.id_unidad
WHERE p.id_producto=@id;", cn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@id", idProducto);
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt.Rows.Count > 0 ? dt.Rows[0] : null;
                }
            }
        }
    }
}
