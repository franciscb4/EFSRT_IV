using DB;
using EFSRT_IV.Models;
using EFSRT_IV.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

namespace EFSRT_IV.Controllers
{
    public class SellController : Controller
    {

        private readonly EfsrtIvContext _context;
        public SellController(EfsrtIvContext context)
        {
            _context = context;
        }

        public IActionResult FindAllSells()
        {
            //OBTENER Y VALIDAR ID DE LA TIENDA ACTUAL
            string sessionStoreId = getFromSession(Constants.SESSION_STORE_ID_KEY);
            if (sessionStoreId.IsNullOrEmpty())
                return RedirectToAction("Index", "User");
            int storeId = Convert.ToInt32(sessionStoreId);

            var list = _context.Venta
                //.Include(v => v.DetalleVenta)
                .Where(v => v.IdTienda == storeId)
                .ToList();

            ////SETTLED PARA EVITAR VALORES REPETIDOS
            //var settedList = firstList
            //    .Where(v =>
            //        !(v.IdVenta > (firstList.Last().IdVenta / 2)))
            //    .ToList();
            //List<Sell> list = settedList
            //    .Select(v => mapperSell(v))
            //    .ToList();
            return View(list);
        }

        public IActionResult FindSell(int id)
        {
            //BUSCAR Y VALIDAR VENTA DE LA BD
            var found = _context.Venta
                .Include(v => v.DetalleVenta)
                .FirstOrDefault(v => v.IdVenta == id);

            if (found == null) return RedirectToAction("FindAllSells");

            //GUARDAR DATOS UNICOS DE LA VENTA EN LA VIEWBAG
            ViewBag.total = found.Monto;
            ViewBag.date = found.Fecha.ToString("dd/MM/yyyy");

            //MAPEAR DE OBJETOS DE LA BD A OBJETOS QUE RECIBE LA VISTA
            List<SellDetail> details = new List<SellDetail>();
            for (int i=0; i < found.DetalleVenta.Count; i++)
            {
                var iterated = found.DetalleVenta.ElementAt(i);
                var productoFound = _context.Productos
                    .FirstOrDefault(p => p.IdProducto == iterated.IdProducto);
                if (productoFound == null) return RedirectToAction("FindAllSells");

                details.Add(mapperSellDetail(
                    iterated,
                    productoFound.Nombre,
                    productoFound.Precio
                    ));
            }
            return View(details);
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

        private static SellDetail mapperSellDetail(DetalleVentum dv, string productoName, decimal productoPrice)
        {
            return new SellDetail()
            {
                id = dv.IdDetalleVenta,
                product = productoName,
                quantity = dv.Cantidad,
                singlePrice = productoPrice,
                subtotal = dv.Subtotal
            };
        }

        private string getFromSession(string key) => HttpContext.Session.GetString(key);
    }
}
