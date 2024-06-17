using ClosedXML.Excel;
using CsvHelper;
using DB.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace EFSRT_IV.Controllers
{
    public class ReportsController : Controller
    {
        private readonly EfsrtIvContext _context;

        public ReportsController(EfsrtIvContext context)
        {
            _context = context;
        }

        public IActionResult GenerateReport(string format)
        {
            var productos = _context.Productos.ToList();

            byte[] reportData;
            string contentType;
            string fileName;

            switch (format.ToLower())
            {
                case "excel":
                    reportData = GenerateExcelReport(productos);
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    fileName = "productos.xlsx";
                    break;
                case "pdf":
                    reportData = GeneratePdfReport(productos);
                    contentType = "application/pdf";
                    fileName = "productos.pdf";
                    break;
                case "csv":
                    reportData = GenerateCsvReport(productos);
                    contentType = "text/csv";
                    fileName = "productos.csv";
                    break;
                default:
                    return BadRequest("Formato no soportado.");
            }

            return File(reportData, contentType, fileName);
        }

        public byte[] GenerateExcelReport(List<Producto> productos)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Productos");
            worksheet.Cell(1, 1).Value = "IdProducto";
            worksheet.Cell(1, 2).Value = "Nombre";
            worksheet.Cell(1, 3).Value = "Precio";
            worksheet.Cell(1, 4).Value = "Stock";
            worksheet.Cell(1, 5).Value = "Estado";
            worksheet.Cell(1, 6).Value = "IdCategoria";
            worksheet.Cell(1, 7).Value = "IdTienda";

            for (int i = 0; i < productos.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = productos[i].IdProducto;
                worksheet.Cell(i + 2, 2).Value = productos[i].Nombre;
                worksheet.Cell(i + 2, 3).Value = productos[i].Precio;
                worksheet.Cell(i + 2, 4).Value = productos[i].Stock;
                worksheet.Cell(i + 2, 5).Value = productos[i].Estado;
                worksheet.Cell(i + 2, 6).Value = productos[i].IdCategoriaProducto;
                worksheet.Cell(i + 2, 7).Value = productos[i].IdTienda;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GeneratePdfReport(List<Producto> productos)
        {
            using var stream = new MemoryStream();
            var document = new Document();
            PdfWriter.GetInstance(document, stream);
            document.Open();

            var table = new PdfPTable(7);
            table.AddCell("IdProducto");
            table.AddCell("Nombre");
            table.AddCell("Precio");
            table.AddCell("Stock");
            table.AddCell("Estado");
            table.AddCell("IdCategoria");
            table.AddCell("IdTienda");

            foreach (var producto in productos)
            {
                table.AddCell(producto.IdProducto.ToString());
                table.AddCell(producto.Nombre);
                table.AddCell(producto.Precio.ToString());
                table.AddCell(producto.Stock.ToString());
                table.AddCell(producto.Estado.ToString());
                table.AddCell(producto.IdCategoriaProducto.ToString());
                table.AddCell(producto.IdTienda.ToString());
            }

            document.Add(table);
            document.Close();
            return stream.ToArray();
        }

        public byte[] GenerateCsvReport(List<Producto> productos)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteHeader<Producto>();
            csv.NextRecord();
            foreach (var producto in productos)
            {
                csv.WriteRecord(producto);
                csv.NextRecord();
            }

            writer.Flush();
            return stream.ToArray();
        }
    }
}
