using EFSRT_IV.Models;
using Microsoft.AspNetCore.Mvc;

namespace EFSRT_IV.Controllers
{
    public class UserController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View(new User());
        }
        [HttpPost]
        public IActionResult Login(User user)
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View(new User());
        }
        [HttpPost]
        public IActionResult SignUp(User user)
        {
            return View();
        }

        public IActionResult LogOut()
        {
            return RedirectToAction("Login");
        }
    }
}
