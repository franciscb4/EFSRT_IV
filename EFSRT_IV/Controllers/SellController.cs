using DB.Models;
using EFSRT_IV.Models;
using EFSRT_IV.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
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

            var firstList = _context.Venta
                .Where(v => v.IdTienda == storeId)
                .ToList();

            List<Sell> list = firstList
                .Select(v => mapperSell(v))
                .ToList();

            return View(list);
        }

        public IActionResult FindSell(int id)
        {
            //BUSCAR Y VALIDAR VENTA DE LA BD
            var found = _context.Venta
                .Include(v => v.DetalleVenta)
                .ThenInclude(dv => dv.IdProductoNavigation) //--
                .FirstOrDefault(v => v.IdVenta == id);

            if (found == null) return RedirectToAction("FindAllSells");

            //LLENANDO VALORES DE LA VENTA, INCLUYENDO LOS DETALLES
            Sell sell = new Sell()
            {
                id = found.IdVenta,
                total = found.Monto,
                date = found.Fecha,
                details = new List<SellDetail>()
            };

            foreach (var detalleVenta in found.DetalleVenta)
                sell.details.Add(mapperSellDetail(detalleVenta));

            return View(sell);
        }

        public IActionResult CreateSell()
        {
            //OBTENER Y VALIDAR ID DE LA TIENDA ACTUAL
            string sessionStoreId = getFromSession(Constants.SESSION_STORE_ID_KEY);
            if (sessionStoreId.IsNullOrEmpty())
                return RedirectToAction("Index", "User");
            int storeId = Convert.ToInt32(sessionStoreId);

            //OBTENER NOMBRES DE LOS PRODUCTOS DE LA BD
            var productos = _context.Productos.Where(p => p.IdTienda == storeId);
            List<string> names =  productos.Select(p => p.Nombre).ToList();
            List<SellItem> list = new List<SellItem>();
            return View(list);
        }
        [HttpPost]
        public IActionResult AddItemToSell([FromBody] SellItem sellItem)
        {
            //VALIDAR EL MODEL STATE
            if (!ModelState.IsValid)
                return RedirectToAction("CreateSell");

            //OBTENER LA LISTA DE ITEMS DE LA SESION Y VALIDAR
            string sessionSell = getFromSession(Constants.SESSION_SELL_LIST_KEY);
            if (sessionSell.IsNullOrEmpty())
                return RedirectToAction("CreateSell");
            List<SellItem> sellList = JsonConvert.DeserializeObject<List<SellItem>>(sessionSell)!;// -!

            //VERIFICAR QUE EL ITEM NO EXISTA EN LA LISTA
            if (sellList.Any(si => si.productId == sellItem.productId))
                return RedirectToAction("CreateSell");

            //AGREGAR EL ITEM EN LA LISTA Y GUARDAR EN LA SESSION
            sellList.Add(sellItem);
            string stringifiedList = JsonConvert.SerializeObject(sellList);
            setInSession(Constants.SESSION_SELL_LIST_KEY, stringifiedList);

            //OBTENER EL ANTIGUO MONTO TOTAL
            string sessionSellAmount = getFromSession(Constants.SESSION_SELL_LIST_AMOUNT_KEY);
            if (sessionSellAmount.IsNullOrEmpty())
                return RedirectToAction("CreateSell");
            decimal oldAmount = Convert.ToDecimal(sessionSellAmount);

            //ACTUALIZAR EL MONTO TOTAL Y GUARDAR EN LA SESSION
            var product = _context.Productos.FirstOrDefault(p => p.IdProducto == sellItem.productId);
            if (product == null)
                return RedirectToAction("CreateSell");
            oldAmount += product.Precio;
            setInSession(Constants.SESSION_SELL_LIST_AMOUNT_KEY, oldAmount.ToString());

            return RedirectToAction("CreateSell");
        }
        public IActionResult ChangeQuantity(int productId, bool increase)
        {
            //OBTENER LA LISTA DE ITEMS DE LA SESION Y VALIDAR
            string sessionSell = getFromSession(Constants.SESSION_SELL_LIST_KEY);
            if (sessionSell.IsNullOrEmpty())
                return RedirectToAction("CreateSell");
            List<SellItem> sellList = JsonConvert.DeserializeObject<List<SellItem>>(sessionSell)!;// -!

            int index = sellList.FindIndex(si => si.productId == productId);
            if (index == -1)
                return RedirectToAction("CreateSell");

            sellList[index].quantity = increase
                ? sellList[index].quantity + 1
                : sellList[index].quantity - 1;

            //OBTENER EL ANTIGUO MONTO TOTAL
            string sessionSellAmount = getFromSession(Constants.SESSION_SELL_LIST_AMOUNT_KEY);
            if (sessionSellAmount.IsNullOrEmpty())
                return RedirectToAction("CreateSell");
            decimal oldAmount = Convert.ToDecimal(sessionSellAmount);

            //OBTENER EL PRECIO DEL PRODUCTO EN CUESTION
            var producto = _context.Productos.FirstOrDefault(p => p.IdProducto == productId);
            if (producto != null)
                return RedirectToAction("CreateSell");
            decimal price = producto.Precio;

            //ACTUALIZAR EL MONTO TOTAL Y GUARDAR EN LA SESION
            oldAmount = increase
                ? oldAmount + price
                : oldAmount - price;

            setInSession(Constants.SESSION_SELL_LIST_AMOUNT_KEY, oldAmount.ToString());
            return RedirectToAction("CreateSell");
        }

        [HttpPost]
        public IActionResult CreateSell(int JA)
        {
            //OBTENER LA LISTA DE ITEMS DE LA SESION Y VALIDAR
            string sessionSell = getFromSession(Constants.SESSION_SELL_LIST_KEY);
            if (sessionSell.IsNullOrEmpty())
                return RedirectToAction("CreateSell");
            List<SellItem> sellList = JsonConvert.DeserializeObject<List<SellItem>>(sessionSell)!;// -!

            //OBTENER LOS PRODUCTOS QUE HAYAN SIDO AGREGADOS DE LA BD
            var productsList = _context.Productos
                                .Where(p => sellList.Any(si => si.productId == p.IdProducto))
                                .ToList();

            //OBTENER Y VALIDAR ID DE LA TIENDA ACTUAL
            string sessionStoreId = getFromSession(Constants.SESSION_STORE_ID_KEY);
            if (sessionStoreId.IsNullOrEmpty())
                return RedirectToAction("Index", "User");
            int storeId = Convert.ToInt32(sessionStoreId);

            //CREAR OBJETO DE VENTA PARA LA BD
            Ventum ventum = new Ventum()
            {
                Fecha = DateTime.Now,
                DetalleVenta = new List<DetalleVentum>(),
                IdTienda = storeId
            };

            //OBTENER EL ANTIGUO MONTO TOTAL
            string sessionSellAmount = getFromSession(Constants.SESSION_SELL_LIST_AMOUNT_KEY);
            if (sessionSellAmount.IsNullOrEmpty())
                return RedirectToAction("CreateSell");
            decimal amount = Convert.ToDecimal(sessionSellAmount);

            //CALCULAR LOS DATOS PARA LA LISTA DE DETALLES
            foreach (var si in sellList)
            {
                Producto producto = productsList.FirstOrDefault(p => p.IdProducto == si.productId);
                if (producto == null)
                    throw new Exception();

                int quantity = si.quantity;
                decimal subtotal = Math.Truncate(100 * (producto.Precio * quantity)) / 100;//TR
                DetalleVentum detalleVentum = new DetalleVentum()
                {
                    IdProducto = si.productId,
                    Cantidad = quantity,
                    Subtotal = subtotal
                };
                ventum.DetalleVenta.Add(detalleVentum);
                ventum.Monto = amount;
            }
            
            //GUARDAR VENTA EN LA BD
            _context.Venta.Add(ventum);
            _context.SaveChanges();

            return View();
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
        private void setInSession(string key, string value) => HttpContext.Session.SetString(key, value);
        //private void setSellListOnSession(string list) => HttpContext.Session.SetString(Constants.SESSION_SELL_LIST_KEY, list);
    }
}
