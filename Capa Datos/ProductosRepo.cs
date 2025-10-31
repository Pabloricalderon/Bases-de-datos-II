using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bases_de_datos_II.Capa_Datos
{
    public class ProductoDto
    {
        public int IdProducto;
        public int IdCategoria;
        public int IdUnidad;
        public string Nombre;
        public string Descripcion;
        public decimal Precio;
        public decimal DescuentoPct;
        public decimal Stock;
        public int? DuracionAnios;
        public int? CoberturaM2;
        public string Color;
        public bool Activo;
    }
    public class ProductosRepo
    {
        public DataTable Listar(string filtro = "")
        {
            using (var cn = Db.GetConn())
            {
                cn.Open();
                string sql = @"
SELECT p.id_producto, c.nombre AS categoria, p.nombre, p.descripcion,
       p.precio, p.descuento_pct, p.stock, u.nombre AS unidad, p.color
FROM productos p
JOIN categorias c ON c.id_categoria = p.id_categoria
JOIN unidades   u ON u.id_unidad    = p.id_unidad
WHERE (@f = '' 
   OR p.nombre      LIKE CONCAT('%', @f, '%')
   OR c.nombre      LIKE CONCAT('%', @f, '%')
   OR p.descripcion LIKE CONCAT('%', @f, '%'))
ORDER BY p.id_producto DESC;";
                using (var da = new MySqlDataAdapter(sql, cn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@f", filtro ?? "");
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public DataTable ListarCategorias()
        {
            using (var cn = Db.GetConn())
            {
                cn.Open();
                using (var da = new MySqlDataAdapter("SELECT id_categoria, nombre FROM categorias ORDER BY nombre;", cn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public DataTable ListarUnidades()
        {
            using (var cn = Db.GetConn())
            {
                cn.Open();
                using (var da = new MySqlDataAdapter("SELECT id_unidad, nombre FROM unidades ORDER BY id_unidad;", cn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public int Insertar(
           int idCategoria, int idUnidad, string nombre, string descripcion,
           decimal precio, decimal descuentoPct, decimal stock,
           int? duracionAnios, int? coberturaM2, string color)
        {
            using (var cn = Db.GetConn())
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        const string sql = @"
INSERT INTO productos
(id_categoria, id_unidad, nombre, descripcion, precio, descuento_pct, stock, duracion_anios, cobertura_m2, color, activo)
VALUES
(@id_categoria,@id_unidad,@nombre,@descripcion,@precio,@descuento,@stock,@duracion,@cobertura,@color,1);";

                        int id;
                        using (var cmd = new MySqlCommand(sql, cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id_categoria", idCategoria);
                            cmd.Parameters.AddWithValue("@id_unidad", idUnidad);
                            cmd.Parameters.AddWithValue("@nombre", nombre);
                            cmd.Parameters.AddWithValue("@descripcion", descripcion);
                            cmd.Parameters.AddWithValue("@precio", precio);
                            cmd.Parameters.AddWithValue("@descuento", descuentoPct);
                            cmd.Parameters.AddWithValue("@stock", stock);
                            cmd.Parameters.AddWithValue("@duracion", (object)duracionAnios ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@cobertura", (object)coberturaM2 ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@color", string.IsNullOrWhiteSpace(color) ? (object)DBNull.Value : color);

                            cmd.ExecuteNonQuery();
                            id = Convert.ToInt32(cmd.LastInsertedId); // ← AQUÍ está la clave
                        }

                        tx.Commit();
                        return id;
                    }
                    catch (MySqlException ex)
                    {
                        tx.Rollback();

                        // Captura el SIGNAL '45000' de tus triggers (pinturas/barnices)
                        if (ex.Number == 1644 || string.Equals(ex.SqlState, "45000", StringComparison.Ordinal))
                            throw new Exception(ex.Message);

                        throw;
                    }
                }
            }
        }

        public ProductoDto ObtenerPorId(int id)
        {
            using (var cn = Db.GetConn())
            {
                cn.Open();
                using (var cmd = new MySqlCommand(
                    @"SELECT id_producto, id_categoria, id_unidad, nombre, descripcion, precio,
                             descuento_pct, stock, duracion_anios, cobertura_m2, color, activo
                      FROM productos WHERE id_producto=@id;", cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return null;
                        var dto = new ProductoDto();
                        dto.IdProducto = rd.GetInt32("id_producto");
                        dto.IdCategoria = rd.GetInt32("id_categoria");
                        dto.IdUnidad = rd.GetInt32("id_unidad");
                        dto.Nombre = rd.GetString("nombre");
                        dto.Descripcion = rd.GetString("descripcion");
                        dto.Precio = rd.GetDecimal("precio");
                        dto.DescuentoPct = rd.GetDecimal("descuento_pct");
                        dto.Stock = rd.GetDecimal("stock");
                        dto.DuracionAnios = rd.IsDBNull(rd.GetOrdinal("duracion_anios")) ? (int?)null : rd.GetInt32("duracion_anios");
                        dto.CoberturaM2 = rd.IsDBNull(rd.GetOrdinal("cobertura_m2")) ? (int?)null : rd.GetInt32("cobertura_m2");
                        dto.Color = rd.IsDBNull(rd.GetOrdinal("color")) ? null : rd.GetString("color");
                        dto.Activo = rd.GetBoolean("activo");
                        return dto;
                    }
                }
            }
        }

        public void Actualizar(ProductoDto p)
        {
            using (var cn = Db.GetConn())
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new MySqlCommand(@"
UPDATE productos
SET id_categoria=@cat, id_unidad=@uni, nombre=@nom, descripcion=@des,
    precio=@pre, descuento_pct=@desc, stock=@stk,
    duracion_anios=@dur, cobertura_m2=@cob, color=@col, activo=@act
WHERE id_producto=@id;", cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@cat", p.IdCategoria);
                            cmd.Parameters.AddWithValue("@uni", p.IdUnidad);
                            cmd.Parameters.AddWithValue("@nom", p.Nombre);
                            cmd.Parameters.AddWithValue("@des", p.Descripcion);
                            cmd.Parameters.AddWithValue("@pre", p.Precio);
                            cmd.Parameters.AddWithValue("@desc", p.DescuentoPct);
                            cmd.Parameters.AddWithValue("@stk", p.Stock);
                            cmd.Parameters.AddWithValue("@dur", (object)p.DuracionAnios ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@cob", (object)p.CoberturaM2 ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@col", string.IsNullOrWhiteSpace(p.Color) ? (object)DBNull.Value : p.Color);
                            cmd.Parameters.AddWithValue("@act", p.Activo ? 1 : 0);
                            cmd.Parameters.AddWithValue("@id", p.IdProducto);
                            cmd.ExecuteNonQuery();
                        }
                        tx.Commit();
                    }
                    catch (MySqlException ex)
                    {
                        tx.Rollback();
                        // 1644 o SqlState 45000: mensaje del trigger de validación
                        if (ex.Number == 1644 || string.Equals(ex.SqlState, "45000", StringComparison.Ordinal))
                            throw new Exception(ex.Message);
                        throw;
                    }
                }
            }
        }

        public void Desactivar(int id)  // recomendable cuando ya hay facturas
        {
            using (var cn = Db.GetConn())
            {
                cn.Open();
                using (var cmd = new MySqlCommand("UPDATE productos SET activo=0 WHERE id_producto=@id;", cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void EliminarFisico(int id) // si NO está referenciado
        {
            using (var cn = Db.GetConn())
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new MySqlCommand("DELETE FROM productos WHERE id_producto=@id;", cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }
                        tx.Commit();
                    }
                    catch (MySqlException ex)
                    {
                        tx.Rollback();
                        if (ex.Number == 1451)
                            throw new Exception("No se puede eliminar: el producto está relacionado a facturas. Usa 'Desactivar'.");
                        throw;
                    }
                }
            }
        }
    }
}
