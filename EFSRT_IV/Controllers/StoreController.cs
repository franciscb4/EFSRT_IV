using DB;
using Microsoft.AspNetCore.Mvc;

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
            int storeId = Convert.ToInt32(HttpContext.Session.GetString("storeId"));
            string storeName = HttpContext.Session.GetString("storeName")!;
            ViewBag.storeId = storeId;
            ViewBag.storeName = storeName;
            return PartialView("_SideBar");
        }
        public IActionResult Panel(int store)
        {
            var found = _context.Tienda.FirstOrDefault(t => t.IdTienda == store);
            if (found == null)
                return RedirectToAction("Index", "User");

            HttpContext.Session.SetString("storeId", found.IdTienda.ToString());
            HttpContext.Session.SetString("storeName", found.Nombre);

            return View(found);
        }

        public IActionResult Productos()
        {
            return View();
        }

        public IActionResult Ventas()
        {
            return View();
        }

        public IActionResult Informes()
        {
            return View();
        }
    }
}
