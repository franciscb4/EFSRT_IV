using DB.Models;
using EFSRT_IV.Models;
using EFSRT_IV.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EFSRT_IV.Controllers
{
    public class StoreController : Controller
    {
        private readonly EfsrtIvContext _context;
        public StoreController(EfsrtIvContext context)
        {
            _context = context;
        }
        public IActionResult SideBar()
        {
            //NO VALIDA POR QUE NO SE COMO MANEJAR PARTIAL VIEW
            string sessionStoreId = getFromSession(Constants.SESSION_STORE_ID_KEY);
            string storeName = getFromSession(Constants.SESSION_STORE_NAME_KEY);

            //if (sessionStoreId.IsNullOrEmpty() || storeName.IsNullOrEmpty())
            //    return RedirectToAction("Index", "User");
            int storeId = Convert.ToInt32(sessionStoreId);

            ViewBag.storeId = storeId;
            ViewBag.storeName = storeName;
            return PartialView("_SideBar");
        }
        public IActionResult Panel(int store)
        {
            var found = _context.Tienda.FirstOrDefault(t => t.IdTienda == store);
            if (found == null)
                return RedirectToAction("Index", "User");

            setStoreInSession(found.IdTienda.ToString(), found.Nombre);

            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-30);

            var ventas = _context.Venta
                .Where(v => v.Fecha >= startDate && v.Fecha <= endDate)
                .GroupBy(v => v.Fecha.Date)
                .Select(g => new { Fecha = g.Key, Total = g.Sum(v => v.Monto) })
                .ToList();

            var ventasLabels = ventas.Select(v => v.Fecha.ToString("dd MMM")).ToList();
            var ventasData = ventas.Select(v => v.Total).ToList();

            var model = new ChartViewModel
            {
                VentasLabels = ventasLabels,
                VentasData = ventasData
            };

            return View(model);
        }

        public IActionResult CreateStore()
        {
            return View(new Store());
        }

        [HttpPost]
        public IActionResult CreateStore(Store store)
        {
            if (!ModelState.IsValid)
                return View();

            //OBTENER Y VALIDAR ID DEL USUARIO ACTUAL
            string sessionUserId = getFromSession(Constants.SESSION_USER_ID_KEY);
            if (sessionUserId.IsNullOrEmpty())
                return RedirectToAction("Login");
            int userId = Convert.ToInt32(sessionUserId);

            _context.Tienda.Add(new Tiendum()
            {
                IdUsuario = userId,
                //ruc
                Nombre = store.businessName, //razon social
                //state
            });
            _context.SaveChanges();
            return RedirectToAction();
        }

        public IActionResult ChangeStateStore(bool state)
        {
            //OBTENER Y VALIDAR ID DE LA TIENDA ACTUAL
            string sessionStoreId = getFromSession(Constants.SESSION_STORE_ID_KEY);
            if (sessionStoreId.IsNullOrEmpty())
                return RedirectToAction("Index", "User");
            int storeId = Convert.ToInt32(sessionStoreId);

            //BUSCAR TIENDA POR ID
            var found = _context.Tienda.FirstOrDefault(s => s.IdTienda == storeId);
            if (found == null)
                return RedirectToAction("Index", "User");

            //CAMBIAR ESTADO DE LA TIENDA EN LA BD
            //found.Estado = state;
            //_context.Tienda.Update(found);
            //_context.SaveChanges();

            return RedirectToAction("Panel", new { store = storeId });
        }

        private string getFromSession(string key) => HttpContext.Session.GetString(key);
        private void setStoreInSession(string id, string name)
        {
            HttpContext.Session.SetString(Constants.SESSION_STORE_ID_KEY, id);
            HttpContext.Session.SetString(Constants.SESSION_STORE_NAME_KEY, name);
        }

    }
}
