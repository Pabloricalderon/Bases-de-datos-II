using Bases_de_datos_II.Utils;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System;
using System.ComponentModel;
using System.IO;

namespace Bases_de_datos_II
{
    public static class TicketPdf
    {
        public static string Generar(
            string empresa,
            string serie,
            int numero,
            DateTime fecha,
            string usuario,
            string cliente,
            BindingList<LineaVenta> lineas,
            BindingList<PagoLinea> pagos,
            decimal subtotal,
            decimal descuento,
            decimal total,
            decimal pagado,
            decimal vuelto)
        {
            var doc = new Document();
            doc.Info.Title = $"Ticket {serie}-{numero}";

            var sec = doc.AddSection();
            sec.PageSetup.PageFormat = PageFormat.A6;
            sec.PageSetup.Orientation = Orientation.Landscape;
            sec.PageSetup.TopMargin = "10mm";
            sec.PageSetup.BottomMargin = "10mm";
            sec.PageSetup.LeftMargin = "10mm";
            sec.PageSetup.RightMargin = "10mm";

            // Encabezado
            var p = sec.AddParagraph(empresa);
            p.Format.Font.Size = 14;
            p.Format.Font.Bold = true;
            p.Format.Alignment = ParagraphAlignment.Center;

            var p2 = sec.AddParagraph($"Factura {serie}-{numero}  |  {fecha:dd/MM/yyyy HH:mm}");
            p2.Format.Font.Size = 9;
            p2.Format.Alignment = ParagraphAlignment.Center;

            sec.AddParagraph($"Atendido por: {usuario}").Format.Font.Size = 8;
            sec.AddParagraph($"Cliente: {cliente}").Format.Font.Size = 8;

            // Tabla de líneas
            var tbl = sec.AddTable();
            tbl.Borders.Width = 0.25;
            tbl.Rows.LeftIndent = 0;
            tbl.AddColumn(Unit.FromCentimeter(5.0)); // Producto
            tbl.AddColumn(Unit.FromCentimeter(2.0)); // Precio
            tbl.AddColumn(Unit.FromCentimeter(1.5)); // Cant.
            tbl.AddColumn(Unit.FromCentimeter(1.5)); // Desc.%
            tbl.AddColumn(Unit.FromCentimeter(2.0)); // Subtotal

            var hdr = tbl.AddRow();
            hdr.Shading.Color = Colors.LightGray;
            hdr.Cells[0].AddParagraph("Producto");
            hdr.Cells[1].AddParagraph("Precio").Format.Alignment = ParagraphAlignment.Right;
            hdr.Cells[2].AddParagraph("Cant.").Format.Alignment = ParagraphAlignment.Right;
            hdr.Cells[3].AddParagraph("Desc.%").Format.Alignment = ParagraphAlignment.Right;
            hdr.Cells[4].AddParagraph("Subtot.").Format.Alignment = ParagraphAlignment.Right;

            foreach (var l in lineas)
            {
                var r = tbl.AddRow();
                r.Cells[0].AddParagraph($"{l.Producto} ({l.Unidad})");
                r.Cells[1].AddParagraph(l.Precio.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;
                r.Cells[2].AddParagraph(l.Cantidad.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;
                r.Cells[3].AddParagraph(l.DescPct.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;
                r.Cells[4].AddParagraph(l.Subtotal.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;
            }

            sec.AddParagraph();

            // Totales
            var t2 = sec.AddTable();
            t2.AddColumn(Unit.FromCentimeter(6.5));
            t2.AddColumn(Unit.FromCentimeter(3.5));

            void addTot(string etiqueta, decimal valor, bool bold = false)
            {
                var rr = t2.AddRow();
                rr.Cells[0].AddParagraph(etiqueta + ":").Format.Alignment = ParagraphAlignment.Right;
                var cell = rr.Cells[1].AddParagraph(valor.ToString("N2"));
                cell.Format.Alignment = ParagraphAlignment.Right;
                if (bold) { rr.Cells[0].Format.Font.Bold = rr.Cells[1].Format.Font.Bold = true; }
            }

            addTot("Subtotal", subtotal);
            addTot("Descuento", descuento);
            addTot("Total", total, bold: true);
            addTot("Pagado", pagado);
            if (vuelto > 0) addTot("Vuelto", vuelto);

            // Pagos
            if (pagos != null && pagos.Count > 0)
            {
                sec.AddParagraph().AddFormattedText("Pagos", TextFormat.Bold).Font.Size = 9;

                var t3 = sec.AddTable();
                t3.Borders.Width = 0.25;
                t3.AddColumn(Unit.FromCentimeter(3.0));
                t3.AddColumn(Unit.FromCentimeter(2.5));
                t3.AddColumn(Unit.FromCentimeter(4.5));

                var rh = t3.AddRow();
                rh.Shading.Color = Colors.LightGray;
                rh.Cells[0].AddParagraph("Método");
                rh.Cells[1].AddParagraph("Monto").Format.Alignment = ParagraphAlignment.Right;
                rh.Cells[2].AddParagraph("Referencia");

                foreach (var pg in pagos)
                {
                    var rr = t3.AddRow();
                    rr.Cells[0].AddParagraph(pg.Metodo);
                    rr.Cells[1].AddParagraph(pg.Monto.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;
                    rr.Cells[2].AddParagraph(pg.Referencia ?? "");
                }
            }

            sec.AddParagraph().AddFormattedText("¡Gracias por su compra!", TextFormat.Italic).Font.Size = 8;

            // Guardar PDF
            var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Paints", "Tickets");
            Directory.CreateDirectory(baseDir);
            var stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var pdfPath = Path.Combine(baseDir, $"ticket_{serie}_{numero}_{stamp}.pdf");

            var renderer = new PdfDocumentRenderer(true) { Document = doc };
            renderer.RenderDocument();
            renderer.PdfDocument.Save(pdfPath);

            return pdfPath;
        }
    }
}
