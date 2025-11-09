using Bases_de_datos_II.Capa_Datos;
using Bases_de_datos_II.Utils;
using MySqlConnector;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Bases_de_datos_II
{
    public partial class FmVentas : Form
    {
        private const decimal IVA_PCT = 0.12m; // 12%

        private readonly VentasRepo _ventas = new VentasRepo();
        private readonly ClientesRepo _clientes = new ClientesRepo();
        private BindingList<LineaVenta> _carrito = new BindingList<LineaVenta>();
        private BindingList<PagoLinea> _pagos = new BindingList<PagoLinea>();
        private int? _idCliente;

        public FmVentas()
        {
            InitializeComponent();

            chkIva.CheckedChanged += (s, e) => RecalcularTotales();


            this.Load += FmVentas_Load;

            dgvLineas.AutoGenerateColumns = false;
            dgvLineas.DataSource = _carrito;
            _carrito.ListChanged += (s, e) => RecalcularTotales();
            dgvLineas.CellEndEdit += (s, e) => RecalcularTotales();

            dgvPagos.AutoGenerateColumns = false;
            dgvPagos.DataSource = _pagos;
            _pagos.ListChanged += (s, e) => RecalcularTotales();

            btnAgregarProducto.Click += (s, e) => BuscarYAgregarProducto();
            btnQuitarLinea.Click += (s, e) => QuitarLineaSel();
            btnVaciar.Click += (s, e) => { _carrito.Clear(); RecalcularTotales(); };

            btnAgregarPago.Click += (s, e) => AgregarPago();
            btnQuitarPago.Click += (s, e) => QuitarPagoSel();

            btnBuscarCliente.Click += (s, e) => BuscarCliente();
            btnRefrescarNumero.Click += (s, e) => CalcularNumero();

            btnGuardarFactura.Click += (s, e) => GuardarFactura();
        }

        private void FmVentas_Load(object sender, EventArgs e)
        {
            cboSerie.Items.Clear(); cboSerie.Items.Add("A"); cboSerie.Items.Add("B"); cboSerie.Items.Add("C"); cboSerie.SelectedIndex = 0;
            lblUsuario.Text = SessionActual.Usuario != null ? SessionActual.Usuario.Username : "";

            // --- GRID LÍNEAS ---
            dgvLineas.AutoGenerateColumns = false;
            dgvLineas.DataSource = _carrito;
            dgvLineas.Columns.Clear();
            dgvLineas.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "IdProducto", Name = "IdProducto", Visible = false });
            dgvLineas.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Producto", Name = "Producto", ReadOnly = true, Width = 230 });
            dgvLineas.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Unidad", Name = "Unidad", ReadOnly = true, Width = 70 });
            dgvLineas.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Precio", Name = "Precio", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvLineas.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Stock", Name = "Stock", ReadOnly = true, Width = 70, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvLineas.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Cantidad", Name = "Cantidad", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvLineas.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DescPct", Name = "DescPct", Width = 70, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvLineas.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Subtotal", Name = "Subtotal", ReadOnly = true, Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });

            // --- GRID PAGOS ---
            dgvPagos.AutoGenerateColumns = false;
            dgvPagos.DataSource = _pagos;
            dgvPagos.Columns.Clear();
            var colMetodo = new DataGridViewComboBoxColumn { Name = "Metodo", DataPropertyName = "Metodo" };
            colMetodo.Items.AddRange(new object[] { "EFECTIVO", "TARJETA", "TRANSFERENCIA" });
            dgvPagos.Columns.Add(colMetodo);
            dgvPagos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Monto", DataPropertyName = "Monto", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvPagos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Referencia", DataPropertyName = "Referencia", Width = 160 });

            // eventos (si aún no los tenías)
            _carrito.ListChanged += (s, ev) => RecalcularTotales();
            dgvLineas.CellEndEdit += (s, ev) => RecalcularTotales();
            _pagos.ListChanged += (s, ev) => RecalcularTotales();

            CalcularNumero();
            RecalcularTotales();
        }


        private void CalcularNumero()
        {
            try
            {
                var serie = Convert.ToString(cboSerie.SelectedItem);
                var n = _ventas.SiguienteNumero(serie);
                txtNumero.Text = n.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo calcular el número: " + ex.Message);
            }
        }

        private void BuscarCliente()
        {
            using (var dlg = new FmSeleccionCliente())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _idCliente = dlg.IdClienteSeleccionado;
                    txtCliente.Text = dlg.NombreSeleccionado;
                }
            }
        }

        private void BuscarYAgregarProducto()
        {
            using (var dlg = new FmSeleccionProducto())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    var r = dlg.FilaSeleccionada;
                    var linea = new LineaVenta
                    {
                        IdProducto = Convert.ToInt32(r["id_producto"]),
                        Producto = Convert.ToString(r["nombre"]),
                        Unidad = Convert.ToString(r["unidad"]),
                        Precio = Convert.ToDecimal(r["precio"]),
                        Stock = Convert.ToDecimal(r["stock"]),
                        Cantidad = 1m,
                        DescPct = 0m
                    };
                    _carrito.Add(linea);
                    RecalcularTotales();
                }
            }
        }

        private void QuitarLineaSel()
        {
            if (dgvLineas.CurrentRow == null) return;
            var item = dgvLineas.CurrentRow.DataBoundItem as LineaVenta;
            if (item != null) _carrito.Remove(item);
            RecalcularTotales();
        }

        private void AgregarPago()
        {
            var total = CalcTotal();
            var pagado = _pagos.Sum(x => x.Monto);
            var saldo = Math.Max(0m, total - pagado);

            var pago = new PagoLinea { Metodo = "EFECTIVO", Monto = saldo, Referencia = "" };
            _pagos.Add(pago);
            RecalcularTotales();
        }

        private void QuitarPagoSel()
        {
            if (dgvPagos.CurrentRow == null) return;
            var p = dgvPagos.CurrentRow.DataBoundItem as PagoLinea;
            if (p != null) _pagos.Remove(p);
            RecalcularTotales();
        }

        private decimal CalcSubtotal() { return _carrito.Sum(l => l.Cantidad * l.Precio); }
        private decimal CalcDescuento() { return _carrito.Sum(l => l.Cantidad * l.Precio * (l.DescPct / 100m)); }

        private decimal CalcIva()
        {
            var baseImp = CalcSubtotal() - CalcDescuento();
            if (baseImp < 0) baseImp = 0;
            return chkIva.Checked ? Math.Round(baseImp * IVA_PCT, 2) : 0m;
        }

        private decimal CalcTotal()
        {
            var sub = CalcSubtotal();
            var desc = CalcDescuento();
            var iva = CalcIva();
            return Math.Round(sub - desc + iva, 2);
        }



        private void RecalcularTotales()
        {
            var sub = Math.Round(CalcSubtotal(), 2);
            var desc = Math.Round(CalcDescuento(), 2);
            var iva = CalcIva();                 // <---
            var total = CalcTotal();
            var pagado = Math.Round(_pagos.Sum(p => p.Monto), 2);
            var saldo = Math.Round(total - pagado, 2);
            var vuelto = saldo < 0 ? Math.Abs(saldo) : 0m;

            lblSubTotal.Text = "Subtotal: " + sub.ToString("N2");
            lblDescTotal.Text = "Descuento: " + desc.ToString("N2");
            lblIva.Text = "IVA: " + iva.ToString("N2");           // si ya implementaste CalcIva()
            lblTotal.Text = "Total: " + total.ToString("N2");
            lblPagado.Text = "Pagado: " + pagado.ToString("N2");
            lblSaldo.Text = "Saldo: " + (saldo > 0 ? saldo : 0m).ToString("N2");
            lblVuelto.Text = "Vuelto: " + vuelto.ToString("N2");


            btnGuardarFactura.Enabled = _idCliente.HasValue && _carrito.Count > 0 && saldo <= 0;

        }

        private void GuardarFactura()
        {
            if (!_idCliente.HasValue) { MessageBox.Show("Selecciona un cliente."); return; }
            if (_carrito.Count == 0) { MessageBox.Show("Agrega productos al carrito."); return; }

            var serie = Convert.ToString(cboSerie.SelectedItem);
            var numero = int.Parse(txtNumero.Text);
            var idUsuario = SessionActual.Usuario != null ? SessionActual.Usuario.IdUsuario : 0;

            using (var cn = Db.GetConn())
            {
                cn.Open();
                int idFactura = 0;

                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        idFactura = _ventas.CrearFactura(serie, numero, _idCliente.Value, idUsuario, cn, tx);

                        foreach (var l in _carrito)
                        {
                            if (l.Cantidad <= 0) throw new Exception("Cantidad inválida en " + l.Producto);
                            if (l.Cantidad > l.Stock) throw new Exception("Sin stock suficiente para " + l.Producto);
                            _ventas.AgregarDetalle(idFactura, l.IdProducto, l.Cantidad, l.Precio, l.DescPct, cn, tx);
                        }

                        foreach (var p in _pagos)
                        {
                            if (p.Monto <= 0) continue;
                            _ventas.RegistrarPago(idFactura, p.Metodo, p.Monto, p.Referencia, cn, tx);
                        }

                        tx.Commit();
                    }
                    catch (MySqlConnector.MySqlException ex)
                    {
                        try { tx.Rollback(); } catch { }
                        if (ex.Number == 1644 || string.Equals(ex.SqlState, "45000", StringComparison.Ordinal))
                            MessageBox.Show(ex.Message);
                        else
                            MessageBox.Show("Error MySQL: " + ex.Message);
                        return;
                    }
                    catch (Exception ex)
                    {
                        try { tx.Rollback(); } catch { }
                        MessageBox.Show("No se guardó: " + ex.Message);
                        return;
                    }
                }

                // ---- fuera de la transacción: calcula totales y genera el archivo
                var sub = Math.Round(CalcSubtotal(), 2);
                var desc = Math.Round(CalcDescuento(), 2);
                var total = CalcTotal();
                var pagado = Math.Round(_pagos.Sum(p => p.Monto), 2);
                var vuelto = total - pagado < 0 ? Math.Abs(total - pagado) : 0m;

                string savedPath = null;
                try
                {
                     savedPath = TicketPdf.Generar(
    "Paints",
    serie,
    numero,
    DateTime.Now,
    SessionActual.Usuario != null ? SessionActual.Usuario.Username : "",
    txtCliente.Text,
    _carrito,
    _pagos,
    sub, desc, total, pagado, vuelto
);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Factura guardada, pero falló la generación del PDF: " + ex.Message);
                }

                MessageBox.Show("Factura guardada." + (savedPath != null ? "\nArchivo generado en:\n" + savedPath : ""));

                _carrito.Clear(); _pagos.Clear(); _idCliente = null; txtCliente.Text = "";
                CalcularNumero(); RecalcularTotales();
            }
        }

    }
}
