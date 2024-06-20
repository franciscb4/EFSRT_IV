using DB.Models;
using EFSRT_IV.Models;
using EFSRT_IV.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EFSRT_IV.Controllers
{
    public class UserController : Controller
    {
        private readonly EfsrtIvContext _context;
        public UserController(EfsrtIvContext context)
        {
            _context = context;
        }

        public IActionResult UserIconButton()
        {
            //NO VALIDA QUE EXISTA USUARIO ACTUAL PORQUE NO SE COMO MANEJAR UN PARTIALVIEW xd
            string sessionUserId = getFromSession(Constants.SESSION_USER_ID_KEY);
            int userId = Convert.ToInt32(sessionUserId);
            ViewBag.logged = userId > 0 ? true : false;

            string userName = getFromSession(Constants.SESSION_USER_NAME_KEY);
            ViewBag.userName = userName;
            return PartialView("_UserIconButton");
        }
        public IActionResult Index(bool? leave = null)
        {
            //RUTA PARA SALIR DE UNA TIENDA
            if (leave != null && (bool)leave)
                unsetStoreInSession();

            //OBTENER Y VALIDAR ID DEL USUARIO ACTUAL
            string sessionUserId = getFromSession(Constants.SESSION_USER_ID_KEY);
            if (sessionUserId.IsNullOrEmpty())
                return RedirectToAction("Login");
            int userId = Convert.ToInt32(sessionUserId);

            //OBTENER TIENDAS A NOMBRE DEL USUARIO ACTUAL
            var foundStores = _context.Tienda.Where(s => s.IdUsuario == userId).ToList();
            ViewBag.userName = getFromSession(Constants.SESSION_USER_NAME_KEY);
            return View(foundStores);
        }

        public IActionResult Login()
        {
            return View(new User());
        }
        [HttpPost]
        public IActionResult Login(User user)
        {
            var found = _context.Usuarios.FirstOrDefault(u => u.Correo == user.email);
            if (found == null)
            {
                ViewBag.userNotFound = "Correo inválido";
                return View();
            }

            if (found.Clave != user.password)
            {
                ViewBag.passInvalid = "Contraseña inválida";
                return View();
            }

            setUserInSession(found.IdUsuario.ToString(), found.Nombre);
            return RedirectToAction("Index");
        }

        public IActionResult SignUp()
        {
            return View(new User());
        }
        [HttpPost]
        public IActionResult SignUp(User user)
        {
            _context.Usuarios.Add(new Usuario()
            {
                Nombre = user.name,
                Correo = user.email,
                Dni = user.dni,
                Telefono = user.phone,
                Clave = user.password
            });
            _context.SaveChanges();
            return View();
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private string getFromSession(string key) => HttpContext.Session.GetString(key)!;
        private void setUserInSession(string id, string name)
        {
            HttpContext.Session.SetString(Constants.SESSION_USER_ID_KEY, id);
            HttpContext.Session.SetString(Constants.SESSION_USER_NAME_KEY, name);
            HttpContext.Session.SetString("logged", true.ToString());
        }
        private void unsetStoreInSession()
        {
            HttpContext.Session.Remove(Constants.SESSION_STORE_ID_KEY);
            HttpContext.Session.Remove(Constants.SESSION_STORE_NAME_KEY);
        }
    }
}
