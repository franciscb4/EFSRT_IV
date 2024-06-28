using AspNetCoreHero.ToastNotification.Abstractions;
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
        private readonly INotyfService _notfy;
        public SellController(EfsrtIvContext context, INotyfService notyf)
        {
            _context = context;
            _notfy = notyf;
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

            string storeState = getFromSession(Constants.SESSION_STORE_STATE_KEY);
            ViewBag.storeState = Convert.ToBoolean(storeState);

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
            var productos = _context.Productos.Where(p => p.IdTienda == storeId && p.Estado == true);
            List<string> names =  productos.Select(p => p.Nombre).ToList();
            ViewBag.names = names;

            //OBTENER LA LISTA DE ITEMS DE LA SESION Y VALIDAR
            string sessionSell = getFromSession(Constants.SESSION_SELL_LIST_KEY);
            List<SellItem> sellList = sessionSell.IsNullOrEmpty()
                    ? new List<SellItem>()
                    : JsonConvert.DeserializeObject<List<SellItem>>(sessionSell)!;// -!

            //OBTENER EL ANTIGUO MONTO TOTAL
            string sessionSellAmount = getFromSession(Constants.SESSION_SELL_LIST_AMOUNT_KEY);
            decimal amount = sessionSellAmount.IsNullOrEmpty()
                    ? 0
                    : Convert.ToDecimal(sessionSellAmount);
            ViewBag.total = amount;

            return View(sellList);
        }
        [HttpPost]
        public IActionResult AddItemToSell(SellItem sellItem)
        {
            //VALIDAR EL MODEL STATE
            if (!ModelState.IsValid)
                return RedirectToAction("CreateSell");
            
            //OBTENER DATOS DEL PRODUCTO SELECCIONADO DESDE LA BD Y VALIDAR
            var product = _context.Productos.FirstOrDefault(p => p.Nombre.Contains(sellItem.productName));
            if (product == null)
            {
                _notfy.Error("Producto no encontrado.");
                return RedirectToAction("CreateSell");
            }

            if (!product.Estado)
            {
                _notfy.Error("Producto no disponible actualmente.");
                return RedirectToAction("CreateSell");
            }

            sellItem.productId = product.IdProducto;
            sellItem.productPrice = product.Precio;
            sellItem.max = product.Stock;

            if (sellItem.quantity > sellItem.max)
            {
                _notfy.Error("La cantidad excede el máximo disponible.");
                return RedirectToAction("CreateSell");
            }

            //OBTENER LA LISTA DE ITEMS DE LA SESION Y VALIDAR
            string sessionSell = getFromSession(Constants.SESSION_SELL_LIST_KEY);
            List<SellItem> sellList = sessionSell.IsNullOrEmpty()
                    ? new List<SellItem>()
                    : JsonConvert.DeserializeObject<List<SellItem>>(sessionSell)!;// -!

            //VERIFICAR SI EL ITEM EXISTA EN LA LISTA Y ACTUALIZARLO
            if (sellList.Any(si => si.productId == sellItem.productId))
            {
                int index = sellList.FindIndex(si => si.productId == sellItem.productId);
                int newQuantity = sellList[index].quantity += sellItem.quantity;
                if (newQuantity > sellItem.max)
                {
                    _notfy.Error("La cantidad excede el máximo permitido.");
                    return RedirectToAction("CreateSell");
                }
                sellList[index].quantity = newQuantity;
            }
            else 
                sellList.Add(sellItem);

            string stringifiedList = JsonConvert.SerializeObject(sellList);
            setInSession(Constants.SESSION_SELL_LIST_KEY, stringifiedList);

            //OBTENER EL ANTIGUO MONTO TOTAL Y ACTUALIZARLO
            string sessionSellAmount = getFromSession(Constants.SESSION_SELL_LIST_AMOUNT_KEY);
            decimal amount = sessionSellAmount.IsNullOrEmpty()
                    ? 0
                    : Convert.ToDecimal(sessionSellAmount);
            amount += sellItem.quantity * sellItem.productPrice;

            //GUARDAR LA LISTA Y EL MONTO EN LA SESSION
            setInSession(Constants.SESSION_SELL_LIST_AMOUNT_KEY, amount.ToString());

            return RedirectToAction("CreateSell");
        }
        public IActionResult CancelSell()
        {
            removeFromSession(Constants.SESSION_SELL_LIST_KEY);
            removeFromSession(Constants.SESSION_SELL_LIST_AMOUNT_KEY);
            return RedirectToAction("FindAllSells");
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
            {
                _notfy.Error("Producto no identificado.");
                return RedirectToAction("CreateSell");
            }

            //VALIDR LIMITES DE CANTIDAD
            SellItem indexed = sellList[index];
            if (indexed.quantity <= 1 && !increase)
                return RedirectToAction("CreateSell");
            else if (indexed.quantity >= indexed.max)
                return RedirectToAction("CreateSell");

            //ACTUALIZAR CANTIDAD DEL ITEM
            sellList[index].quantity = increase
                ? indexed.quantity + 1
                : indexed.quantity - 1;

            //OBTENER EL ANTIGUO MONTO TOTAL
            string sessionSellAmount = getFromSession(Constants.SESSION_SELL_LIST_AMOUNT_KEY);
            if (sessionSellAmount.IsNullOrEmpty())
                return RedirectToAction("CreateSell");
            decimal oldAmount = Convert.ToDecimal(sessionSellAmount);

            //ACTUALIZAR EL MONTO TOTAL Y GUARDAR EN LA SESION
            oldAmount = increase
                ? oldAmount + indexed.productPrice
                : oldAmount - indexed.productPrice;

            string stringifiedList = JsonConvert.SerializeObject(sellList);
            setInSession(Constants.SESSION_SELL_LIST_KEY, stringifiedList);
            setInSession(Constants.SESSION_SELL_LIST_AMOUNT_KEY, oldAmount.ToString());
            return RedirectToAction("CreateSell");
        }

        public IActionResult DeleteItemFromSell(int productId)
        {
            //OBTENER LA LISTA DE ITEMS DE LA SESION Y VALIDAR
            string sessionSell = getFromSession(Constants.SESSION_SELL_LIST_KEY);
            if (sessionSell.IsNullOrEmpty())
                return RedirectToAction("CreateSell");
            List<SellItem> sellList = JsonConvert.DeserializeObject<List<SellItem>>(sessionSell)!;// -!

            //VERIFICAR QUE EL ITEM EXISTA EN LA LISTA
            SellItem found = sellList.FirstOrDefault(si => si.productId == productId);
            if (found == null)
            {
                _notfy.Error("Producto no identificado.");
                return RedirectToAction("CreateSell");
            }

            //OBTENER EL ANTIGUO MONTO TOTAL
            string sessionSellAmount = getFromSession(Constants.SESSION_SELL_LIST_AMOUNT_KEY);
            if (sessionSellAmount.IsNullOrEmpty())
                return RedirectToAction("CreateSell");
            decimal oldAmount = Convert.ToDecimal(sessionSellAmount);

            //ACTUALIZAR OBJETOS Y GUARDAR EN LA SESION
            oldAmount -= (found.quantity * found.productPrice);
            sellList.Remove(found);

            string stringifiedList = JsonConvert.SerializeObject(sellList);
            setInSession(Constants.SESSION_SELL_LIST_KEY, stringifiedList);
            setInSession(Constants.SESSION_SELL_LIST_AMOUNT_KEY, oldAmount.ToString());

            _notfy.Success("Producto eliminado");
            return RedirectToAction("CreateSell");
        }

        public IActionResult MakeSell()
        {
            //OBTENER LA LISTA DE ITEMS DE LA SESION Y VALIDAR
            string sessionSell = getFromSession(Constants.SESSION_SELL_LIST_KEY);
            if (sessionSell.IsNullOrEmpty())
                return RedirectToAction("CreateSell");
            List<SellItem> sellList = JsonConvert.DeserializeObject<List<SellItem>>(sessionSell)!;// -!

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
            ventum.Monto = amount;

            //CALCULAR LOS DATOS PARA LA LISTA DE DETALLES
            foreach (var si in sellList)
            {
                int quantity = si.quantity;
                decimal subtotal = si.productPrice * quantity;
                DetalleVentum detalleVentum = new DetalleVentum()
                {
                    IdProducto = si.productId,
                    Cantidad = quantity,
                    Subtotal = subtotal
                };
                ventum.DetalleVenta.Add(detalleVentum);

                var found = _context.Productos.FirstOrDefault(p => p.IdProducto == si.productId);
                if (found != null)
                {
                    found.Stock -= si.quantity;
                }
            }
            
            //GUARDAR VENTA EN LA BD
            _context.Venta.Add(ventum);
            _context.SaveChanges();
            removeFromSession(Constants.SESSION_SELL_LIST_KEY);
            removeFromSession(Constants.SESSION_SELL_LIST_AMOUNT_KEY);
            _notfy.Success("Venta registrada");

            return RedirectToAction("FindAllSells");
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
        private void removeFromSession(string key) => HttpContext.Session.Remove(key);
        private void setInSession(string key, string value) => HttpContext.Session.SetString(key, value);
        //private void setSellListOnSession(string list) => HttpContext.Session.SetString(Constants.SESSION_SELL_LIST_KEY, list);
    }
}
