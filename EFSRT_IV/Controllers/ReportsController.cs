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
            string sessionStoreId = getFromSession(Constants.SESSION_STORE_ID_KEY);
            if (sessionStoreId.IsNullOrEmpty())
            {
                return RedirectToAction("Index", "User");
            }

            int storeId = Convert.ToInt32(sessionStoreId);

            var ventas = _context.Venta
                .Where(v => v.IdTienda == storeId)
                .GroupBy(v => v.Fecha.Date)
                .Select(v => new Sell
                {
                    id = v.First().IdVenta,
                    date = v.Key,
                    total = v.Sum(x => x.Monto),
                    details = v.Select(x => new SellDetail
                    {
                        id = x.IdVenta,
                        quantity = x.DetalleVenta.Sum(dv => dv.Cantidad),
                        singlePrice = x.DetalleVenta.First().IdProductoNavigation.Precio,
                        subtotal = x.DetalleVenta.Sum(dv => dv.Subtotal),
                        product = x.DetalleVenta.First().IdProductoNavigation.Nombre
                    }).ToList()
                })
                .OrderByDescending(v => v.date)
                .ToList();

            return View(ventas);
        }

        public IActionResult GenerateReportPDF(DateTime date)
        {
            string sessionStoreId = getFromSession(Constants.SESSION_STORE_ID_KEY);
            if (sessionStoreId.IsNullOrEmpty())
            {
                return RedirectToAction("Index", "User");
            }

            int storeId = Convert.ToInt32(sessionStoreId);

            var ventasDelDia = _context.Venta
                .Include(v => v.DetalleVenta)
                .ThenInclude(dv => dv.IdProductoNavigation)
                .Where(v => v.Fecha.Date == date.Date && v.IdTienda == storeId)
                .ToList();

            var sells = ventasDelDia.Select(v => mapperSell(v)).ToList();

            foreach (var sell in sells)
            {
                sell.details = ventasDelDia
                    .Where(v => v.IdVenta == sell.id)
                    .SelectMany(v => v.DetalleVenta.Select(dv => mapperSellDetail(dv)))
                    .ToList();
            }

            return new ViewAsPdf("GenerateReportPDF", sells)
            {
                FileName = $"Reporte Ventas {date.ToShortDateString()}.pdf",
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };
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
