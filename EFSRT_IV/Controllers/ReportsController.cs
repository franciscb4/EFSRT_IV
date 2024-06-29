using ClosedXML.Excel;
using CsvHelper;
using DB.Models;
using EFSRT_IV.Models;
using EFSRT_IV.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Rotativa.AspNetCore;
using System.Globalization;
using System.Linq;

namespace EFSRT_IV.Controllers
{
    public class ReportsController : Controller
    {
        private readonly EfsrtIvContext _context;

        public ReportsController(EfsrtIvContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GenerateReport()
        {
            //OBTENER Y VALIDAR ID DE LA TIENDA ACTUAL
            string sessionStoreId = getFromSession(Constants.SESSION_STORE_ID_KEY);
            if (sessionStoreId.IsNullOrEmpty())
                return RedirectToAction("Index", "User");
            int storeId = Convert.ToInt32(sessionStoreId);

            var ventas = _context.Venta
                .Where(v => v.IdTienda == storeId)
                .Select(v => mapperSell(v))
                .ToList();

            return View(ventas);
        }

        [HttpPost]
        public IActionResult GenerateReport(string format)
        {
            var usuarios = _context.Usuarios.ToList();
            var tiendas = _context.Tienda.ToList();
            var categoriasProducto = _context.CategoriaProductos.ToList();
            var productos = _context.Productos.ToList();
            var ventas = _context.Venta.ToList();
            var detallesVenta = _context.DetalleVenta.ToList();
            var categoriasGasto = _context.CategoriaGastos.ToList();
            var proveedores = _context.Proveedors.ToList();
            var gastos = _context.Gastos.ToList();
            var detallesGasto = _context.DetalleGastos.ToList();

            byte[] reportData;
            string contentType;
            string fileName;

            switch (format.ToLower())
            {
                case "excel":
                    reportData = GenerateExcelReport(usuarios, tiendas, categoriasProducto, productos, ventas, detallesVenta, categoriasGasto, proveedores, gastos, detallesGasto);
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    fileName = "reporte " + DateTime.Now.ToString() + ".xlsx";
                    break;
                //case "pdf":
                //    reportData = GeneratePdfReport(productos);
                //    contentType = "application/pdf";
                //    fileName = "productos " + DateTime.Now.ToString() + ".pdf";
                //    break;
                case "csv":
                    reportData = GenerateCsvReport(productos);
                    contentType = "text/csv";
                    fileName = "productos " + DateTime.Now.ToString() + ".csv";
                    break;
                default:
                    return BadRequest("Formato no soportado.");
            }

            return File(reportData, contentType, fileName);
        }

        public byte[] GenerateExcelReport(
            List<Usuario> usuarios,
            List<Tiendum> tiendas,
            List<CategoriaProducto> categoriasProducto,
            List<Producto> productos,
            List<Ventum> ventas,
            List<DetalleVentum> detallesVenta,
            List<CategoriaGasto> categoriasGasto,
            List<Proveedor> proveedores,
            List<Gasto> gastos,
            List<DetalleGasto> detallesGasto)
        {
            using var workbook = new XLWorkbook();

            string sessionStoreId = getFromSession(Constants.SESSION_STORE_ID_KEY);

            int idTienda = Convert.ToInt32(sessionStoreId);

            // Report: Ventas por Tienda (Suma de ventas agrupadas por cada tienda)
            var ventasPorTiendaSheet = workbook.Worksheets.Add("VentasPorTienda");
            ventasPorTiendaSheet.Cell(1, 1).Value = "IdTienda";
            ventasPorTiendaSheet.Cell(1, 2).Value = "Tienda";
            ventasPorTiendaSheet.Cell(1, 3).Value = "MontoTotal";

            var ventasPorTienda = ventas.Where(v => v.IdTienda == idTienda)
                                        .GroupBy(v => v.IdTienda)
                                        .Select(g => new { IdTienda = g.Key, MontoTotal = g.Sum(v => v.Monto) })
                                        .ToList();
            for (int i = 0; i < ventasPorTienda.Count; i++)
            {
                var tienda = tiendas.FirstOrDefault(t => t.IdTienda == ventasPorTienda[i].IdTienda);
                ventasPorTiendaSheet.Cell(i + 2, 1).Value = ventasPorTienda[i].IdTienda;
                ventasPorTiendaSheet.Cell(i + 2, 2).Value = tienda?.RazonSocial;
                ventasPorTiendaSheet.Cell(i + 2, 3).Value = ventasPorTienda[i].MontoTotal;
            }

            // Report: Productos más vendidos por tienda
            var productosMasVendidosSheet = workbook.Worksheets.Add("ProductosMasVendidos");
            productosMasVendidosSheet.Cell(1, 1).Value = "IdTienda";
            productosMasVendidosSheet.Cell(1, 2).Value = "Tienda";
            productosMasVendidosSheet.Cell(1, 3).Value = "IdProducto";
            productosMasVendidosSheet.Cell(1, 4).Value = "Producto";
            productosMasVendidosSheet.Cell(1, 5).Value = "CantidadVendida";

            var productosMasVendidos = detallesVenta.Where(dv => ventas.First(v => v.IdVenta == dv.IdVenta).IdTienda == idTienda)
                                                    .GroupBy(dv => new { dv.IdProducto, dv.IdVenta })
                                                    .Select(g => new { g.Key.IdProducto, IdTienda = ventas.First(v => v.IdVenta == g.Key.IdVenta).IdTienda, CantidadVendida = g.Sum(dv => dv.Cantidad) })
                                                    .GroupBy(d => new { d.IdTienda, d.IdProducto })
                                                    .Select(g => new { g.Key.IdTienda, g.Key.IdProducto, CantidadVendida = g.Sum(d => d.CantidadVendida) })
                                                    .OrderByDescending(p => p.CantidadVendida)
                                                    .ToList();
            for (int i = 0; i < productosMasVendidos.Count; i++)
            {
                var tienda = tiendas.FirstOrDefault(t => t.IdTienda == productosMasVendidos[i].IdTienda);
                var producto = productos.FirstOrDefault(p => p.IdProducto == productosMasVendidos[i].IdProducto);
                productosMasVendidosSheet.Cell(i + 2, 1).Value = productosMasVendidos[i].IdTienda;
                productosMasVendidosSheet.Cell(i + 2, 2).Value = tienda?.RazonSocial;
                productosMasVendidosSheet.Cell(i + 2, 3).Value = productosMasVendidos[i].IdProducto;
                productosMasVendidosSheet.Cell(i + 2, 4).Value = producto?.Nombre;
                productosMasVendidosSheet.Cell(i + 2, 5).Value = productosMasVendidos[i].CantidadVendida;
            }

            // Report: Gastos por Categoría (Total de gastos agrupados por categoría)
            var gastosPorCategoriaSheet = workbook.Worksheets.Add("GastosPorCategoria");
            gastosPorCategoriaSheet.Cell(1, 1).Value = "IdCategoriaGasto";
            gastosPorCategoriaSheet.Cell(1, 2).Value = "Categoria";
            gastosPorCategoriaSheet.Cell(1, 3).Value = "MontoTotal";

            var gastosPorCategoria = gastos.Where(g => g.IdTienda == idTienda)
                                           .GroupBy(g => g.IdCategoriaGasto)
                                           .Select(g => new { IdCategoriaGasto = g.Key, MontoTotal = g.Sum(g => g.Monto) })
                                           .ToList();
            for (int i = 0; i < gastosPorCategoria.Count; i++)
            {
                var categoria = categoriasGasto.FirstOrDefault(c => c.IdCategoriaGasto == gastosPorCategoria[i].IdCategoriaGasto);
                gastosPorCategoriaSheet.Cell(i + 2, 1).Value = gastosPorCategoria[i].IdCategoriaGasto;
                gastosPorCategoriaSheet.Cell(i + 2, 2).Value = categoria?.Nombre;
                gastosPorCategoriaSheet.Cell(i + 2, 3).Value = gastosPorCategoria[i].MontoTotal;
            }

            // Report: Gastos detallados por proveedor
            var gastosPorProveedorSheet = workbook.Worksheets.Add("GastosPorProveedor");
            gastosPorProveedorSheet.Cell(1, 1).Value = "IdProveedor";
            gastosPorProveedorSheet.Cell(1, 2).Value = "Proveedor";
            gastosPorProveedorSheet.Cell(1, 3).Value = "MontoTotal";

            var gastosPorProveedor = gastos.Where(g => g.IdTienda == idTienda)
                                           .GroupBy(g => g.IdProveedor)
                                           .Select(g => new { IdProveedor = g.Key, MontoTotal = g.Sum(g => g.Monto) })
                                           .ToList();
            for (int i = 0; i < gastosPorProveedor.Count; i++)
            {
                var proveedor = proveedores.FirstOrDefault(p => p.IdProveedor == gastosPorProveedor[i].IdProveedor);
                gastosPorProveedorSheet.Cell(i + 2, 1).Value = gastosPorProveedor[i].IdProveedor;
                gastosPorProveedorSheet.Cell(i + 2, 2).Value = proveedor?.Nombre;
                gastosPorProveedorSheet.Cell(i + 2, 3).Value = gastosPorProveedor[i].MontoTotal;
            }

            // Report: Productos con bajo inventario
            var productosBajoInventarioSheet = workbook.Worksheets.Add("ProductosBajoInventario");
            productosBajoInventarioSheet.Cell(1, 1).Value = "IdProducto";
            productosBajoInventarioSheet.Cell(1, 2).Value = "Producto";
            productosBajoInventarioSheet.Cell(1, 3).Value = "Stock";
            productosBajoInventarioSheet.Cell(1, 4).Value = "IdTienda";
            productosBajoInventarioSheet.Cell(1, 5).Value = "Tienda";

            var productosBajoInventario = productos.Where(p => p.IdTienda == idTienda && p.Stock < 10) // Consideramos bajo inventario cuando es menos de 10
                                                   .ToList();
            for (int i = 0; i < productosBajoInventario.Count; i++)
            {
                var tienda = tiendas.FirstOrDefault(t => t.IdTienda == productosBajoInventario[i].IdTienda);
                productosBajoInventarioSheet.Cell(i + 2, 1).Value = productosBajoInventario[i].IdProducto;
                productosBajoInventarioSheet.Cell(i + 2, 2).Value = productosBajoInventario[i].Nombre;
                productosBajoInventarioSheet.Cell(i + 2, 3).Value = productosBajoInventario[i].Stock;
                productosBajoInventarioSheet.Cell(i + 2, 4).Value = productosBajoInventario[i].IdTienda;
                productosBajoInventarioSheet.Cell(i + 2, 5).Value = tienda?.RazonSocial;
            }

            // Report: Valor total del inventario por tienda y categoría de producto
            var valorInventarioSheet = workbook.Worksheets.Add("ValorInventario");
            valorInventarioSheet.Cell(1, 1).Value = "IdTienda";
            valorInventarioSheet.Cell(1, 2).Value = "Tienda";
            valorInventarioSheet.Cell(1, 3).Value = "IdCategoriaProducto";
            valorInventarioSheet.Cell(1, 4).Value = "Categoria";
            valorInventarioSheet.Cell(1, 5).Value = "ValorTotal";

            var valorInventario = productos.Where(p => p.IdTienda == idTienda)
                                           .GroupBy(p => new { p.IdTienda, p.IdCategoriaProducto })
                                           .Select(g => new { g.Key.IdTienda, g.Key.IdCategoriaProducto, ValorTotal = g.Sum(p => p.Precio * p.Stock) })
                                           .ToList();
            for (int i = 0; i < valorInventario.Count; i++)
            {
                var tienda = tiendas.FirstOrDefault(t => t.IdTienda == valorInventario[i].IdTienda);
                var categoria = categoriasProducto.FirstOrDefault(c => c.IdCategoriaProducto == valorInventario[i].IdCategoriaProducto);
                valorInventarioSheet.Cell(i + 2, 1).Value = valorInventario[i].IdTienda;
                valorInventarioSheet.Cell(i + 2, 2).Value = tienda?.RazonSocial;
                valorInventarioSheet.Cell(i + 2, 3).Value = valorInventario[i].IdCategoriaProducto;
                valorInventarioSheet.Cell(i + 2, 4).Value = categoria?.Nombre;
                valorInventarioSheet.Cell(i + 2, 5).Value = valorInventario[i].ValorTotal;
            }

            // Report: Estado de Usuarios y Tiendas (Listado de usuarios y tiendas activas e inactivas)
            var estadoUsuariosSheet = workbook.Worksheets.Add("EstadoUsuarios");
            estadoUsuariosSheet.Cell(1, 1).Value = "IdUsuario";
            estadoUsuariosSheet.Cell(1, 2).Value = "Usuario";
            estadoUsuariosSheet.Cell(1, 3).Value = "Estado";
            for (int i = 0; i < usuarios.Count; i++)
            {
                estadoUsuariosSheet.Cell(i + 2, 1).Value = usuarios[i].IdUsuario;
                estadoUsuariosSheet.Cell(i + 2, 2).Value = usuarios[i].Nombre;
                estadoUsuariosSheet.Cell(i + 2, 3).Value = usuarios[i].Estado;
            }

            var estadoTiendasSheet = workbook.Worksheets.Add("EstadoTiendas");
            estadoTiendasSheet.Cell(1, 1).Value = "IdTienda";
            estadoTiendasSheet.Cell(1, 2).Value = "Tienda";
            estadoTiendasSheet.Cell(1, 3).Value = "Estado";
            var tiendaEstado = tiendas.FirstOrDefault(t => t.IdTienda == idTienda);
            estadoTiendasSheet.Cell(2, 1).Value = tiendaEstado?.IdTienda;
            estadoTiendasSheet.Cell(2, 2).Value = tiendaEstado?.RazonSocial;
            //estadoTiendasSheet.Cell(2, 3).Value = tiendaEstado?.Estado;

            // Report: Ingresos y Egresos (Comparación entre ingresos y egresos)
            var ingresosEgresosSheet = workbook.Worksheets.Add("IngresosEgresos");
            ingresosEgresosSheet.Cell(1, 1).Value = "IdTienda";
            ingresosEgresosSheet.Cell(1, 2).Value = "Tienda";
            ingresosEgresosSheet.Cell(1, 3).Value = "TotalIngresos";
            ingresosEgresosSheet.Cell(1, 4).Value = "TotalEgresos";
            ingresosEgresosSheet.Cell(1, 5).Value = "Rentabilidad";

            var ingresosEgresos = new
            {
                IdTienda = idTienda,
                Tienda = tiendas.FirstOrDefault(t => t.IdTienda == idTienda)?.RazonSocial,
                TotalIngresos = ventas.Where(v => v.IdTienda == idTienda).Sum(v => v.Monto),
                TotalEgresos = gastos.Where(g => g.IdTienda == idTienda).Sum(g => g.Monto)
            };
            var rentabilidad = ingresosEgresos.TotalIngresos - ingresosEgresos.TotalEgresos;

            ingresosEgresosSheet.Cell(2, 1).Value = ingresosEgresos.IdTienda;
            ingresosEgresosSheet.Cell(2, 2).Value = ingresosEgresos.Tienda;
            ingresosEgresosSheet.Cell(2, 3).Value = ingresosEgresos.TotalIngresos;
            ingresosEgresosSheet.Cell(2, 4).Value = ingresosEgresos.TotalEgresos;
            ingresosEgresosSheet.Cell(2, 5).Value = rentabilidad;

            // Report: Histórico de Ventas y Gastos (Tendencias a lo largo del tiempo)
            var historicoVentasGastosSheet = workbook.Worksheets.Add("HistoricoVentasGastos");
            historicoVentasGastosSheet.Cell(1, 1).Value = "Fecha";
            historicoVentasGastosSheet.Cell(1, 2).Value = "TotalVentas";
            historicoVentasGastosSheet.Cell(1, 3).Value = "TotalGastos";

            var historicoVentas = ventas.Where(v => v.IdTienda == idTienda)
                                        .GroupBy(v => v.Fecha.Date)
                                        .Select(g => new { Fecha = g.Key, TotalVentas = g.Sum(v => v.Monto) })
                                        .ToList();
            var historicoGastos = gastos.Where(g => g.IdTienda == idTienda)
                                        .GroupBy(g => g.Fecha.Date)
                                        .Select(g => new { Fecha = g.Key, TotalGastos = g.Sum(g => g.Monto) })
                                        .ToList();

            var fechas = historicoVentas.Select(v => v.Fecha)
                                        .Union(historicoGastos.Select(g => g.Fecha))
                                        .Distinct()
                                        .OrderBy(f => f)
                                        .ToList();

            for (int i = 0; i < fechas.Count; i++)
            {
                var totalVentas = historicoVentas.FirstOrDefault(v => v.Fecha == fechas[i])?.TotalVentas ?? 0;
                var totalGastos = historicoGastos.FirstOrDefault(g => g.Fecha == fechas[i])?.TotalGastos ?? 0;
                historicoVentasGastosSheet.Cell(i + 2, 1).Value = fechas[i];
                historicoVentasGastosSheet.Cell(i + 2, 2).Value = totalVentas;
                historicoVentasGastosSheet.Cell(i + 2, 3).Value = totalGastos;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public IActionResult ImprimirVenta(int id)
        {
            var first = _context.Venta
                .Include(v => v.DetalleVenta)
                .ThenInclude(dv => dv.IdProductoNavigation)
                .FirstOrDefault(v => v.IdVenta == id);

            Sell sell = mapperSell(first);
            sell.details = first.DetalleVenta.Select(dv => mapperSellDetail(dv)).ToList();
            return new ViewAsPdf("ImprimirVenta", sell)
            {
                FileName = $"Venta {sell.id}.pdf",
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };
        }

        public byte[] GenerateCsvReport(List<Producto> productos)
        {
            return null;
        }

        private static Sell mapperSell(Ventum v)
        {
            return new Sell()
            {
                id = v.IdVenta,
                date = v.Fecha,
                total = v.Monto
            };
        }

        private static SellDetail mapperSellDetail(DetalleVentum dv)
        {
            var producto = dv.IdProductoNavigation;
            SellDetail sellDetail = new SellDetail()
            {
                id = dv.IdDetalleVenta,
                quantity = dv.Cantidad,
                singlePrice = producto.Precio,
                subtotal = dv.Subtotal,
                product = producto.Nombre
            };

            return sellDetail;
        }

        private string getFromSession(string key) => HttpContext.Session.GetString(key);
    }
}
