using Microsoft.AspNetCore.Mvc;

namespace EFSRT_IV.Controllers
{
    public class StoreController : Controller
    {

        public IActionResult SideBar()
        {
            return PartialView("_SideBar");
        }
        public IActionResult Panel()
        {
            return View();
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
