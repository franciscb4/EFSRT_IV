using AspNetCoreHero.ToastNotification.Abstractions;
using DB.Models;
using EFSRT_IV.Models;
using EFSRT_IV.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Diagnostics;

namespace EFSRT_IV.Controllers
{
    public class ProductsController : Controller
    {
        private readonly EfsrtIvContext _context;
        private readonly INotyfService _notfy;
        public ProductsController(EfsrtIvContext context, INotyfService notyf) 
        {
            _context = context;
            _notfy = notyf;
        }

        public IActionResult FindAllProducts()
        {

            //OBTENER Y VALIDAR ID DE LA TIENDA ACTUAL
            string sessionStoreId = getFromSession(Constants.SESSION_STORE_ID_KEY);
            if (sessionStoreId.IsNullOrEmpty())
                return RedirectToAction("Index", "User");
            int storeId = Convert.ToInt32(sessionStoreId);

            //OBTENER PRODUCTOS DE LA TIENDA Y MAPEARLOS
            var products = _context.Productos
                .Where(p => p.IdTienda == storeId)
                .Select(p => mapperProduct(p))
                .ToList();

            var tempFilter = TempData["filter"];
            if (tempFilter != null)
            {
                string filter = tempFilter.ToString()!;

                var filtereds = _context.Productos
                    .Where(p => p.IdTienda == storeId && p.Nombre.Contains(filter))
                    .Select(p => mapperProduct(p))
                    .ToList();

                if (filtereds.IsNullOrEmpty())
                    _notfy.Warning("No se encontraron coincidencias.");
                else
                    products = filtereds;
            }

            string storeState = getFromSession(Constants.SESSION_STORE_STATE_KEY);
            ViewBag.storeState = Convert.ToBoolean(storeState);

            return View(products);
        }

        [HttpPost]
        public IActionResult FilterProducts(string filter)
        {

            TempData["filter"] = filter;
            return RedirectToAction("FindAllProducts");
        }

        public IActionResult CreateProduct()
        {
                //OBTENER CATEGORIAS Y PASARLAS AL VIEWBAG
                var categorias = _context.CategoriaProductos.ToList();
                ViewBag.categorias = new SelectList(categorias, "IdCategoriaProducto", "Nombre");

                return View(new Product());

        }
        [HttpPost]
        public IActionResult CreateProduct(Product product)
        {
            //VALIDAR STATE
            if (!ModelState.IsValid)
            {
                //OBTENER CATEGORIAS Y PASARLAS AL VIEWBAG
                var categorias = _context.CategoriaProductos.ToList();
                ViewBag.categorias = new SelectList(categorias, "IdCategoriaProducto", "Nombre");
                return View();
            }

            //OBTENER Y VALIDAR ID DE LA TIENDA ACTUAL
            string sessionStoreId = getFromSession(Constants.SESSION_STORE_ID_KEY);
            if (sessionStoreId.IsNullOrEmpty())
                return RedirectToAction("Index", "User");
            int storeId = Convert.ToInt32(sessionStoreId);

            //AGREGAR PRODUCTO A LA BD
            _context.Productos.Add(new Producto()
            {
                Nombre = product.name,
                Precio = Convert.ToDecimal(product.price),
                Stock = product.stock,
                IdCategoriaProducto = product.category,
                IdTienda = storeId,
                Estado = true
            });
            _context.SaveChanges();
            _notfy.Success("Producto registrado.");
            return RedirectToAction("FindAllProducts");
        }

        public IActionResult Find(int id)
        {
            //OBTENER Y VALIDAR ID DE LA TIENDA ACTUAL
            string sessionStoreId = getFromSession(Constants.SESSION_STORE_ID_KEY);
            if (sessionStoreId.IsNullOrEmpty())
                return RedirectToAction("Index", "User");
            int storeId = Convert.ToInt32(sessionStoreId);

            //BUSCAR PRODUCTO POR TIENDA Y ID
            var found = _context.Productos.FirstOrDefault(p => p.IdTienda == storeId && p.IdProducto == id);
            if (found == null) return RedirectToAction("FindAllProducts");
            
            //BUSCAR CATEGORIA PARA MOSTRAR SU NOMBRE
            var categoria = _context.CategoriaProductos.FirstOrDefault(c => c.IdCategoriaProducto == found.IdCategoriaProducto);
            if (categoria == null) return RedirectToAction("FindAllProducts");
            ViewBag.categoria = categoria.Nombre;

            string storeState = getFromSession(Constants.SESSION_STORE_STATE_KEY);
            ViewBag.storeState = Convert.ToBoolean(storeState);

            //PRODUCTO PARA LA VISTA
            Product product = mapperProduct(found);
            return View(product);
        }

        public IActionResult UpdateProduct(int id)
        {
            //OBTENER Y VALIDAR ID DE LA TIENDA ACTUAL
            string sessionStoreId = getFromSession(Constants.SESSION_STORE_ID_KEY);
            if (sessionStoreId.IsNullOrEmpty())
                return RedirectToAction("Index", "User");
            int storeId = Convert.ToInt32(sessionStoreId);
            
            //BUSCAR PRODUCTO POR TIENDA Y ID
            var found = _context.Productos.FirstOrDefault(p => p.IdTienda == storeId && p.IdProducto == id);
            if (found == null) return RedirectToAction("FindAllProducts");

            //BUSCAR CATEGORIA PARA MOSTRAR SU NOMBRE
            var categorias = _context.CategoriaProductos.ToList();
            ViewBag.categorias = new SelectList(categorias, "IdCategoriaProducto", "Nombre", found.IdCategoriaProducto);

            //PRODUCTO PARA LA VISTA
            Product product = mapperProduct(found);
            return View(product);
        }
        [HttpPost]
        public IActionResult UpdateProduct(Product product)
        {
            //VALIDAR STATE
            if (!ModelState.IsValid)
            {
                //OBTENER CATEGORIAS Y PASARLAS AL VIEWBAG
                var categorias = _context.CategoriaProductos.ToList();
                ViewBag.categorias = new SelectList(categorias, "IdCategoriaProducto", "Nombre", product.category);
                return View();
            }

            //OBTENER Y VALIDAR ID DE LA TIENDA ACTUAL
            string sessionStoreId = getFromSession(Constants.SESSION_STORE_ID_KEY);
            if (sessionStoreId.IsNullOrEmpty())
                return RedirectToAction("Index", "User");
            int storeId = Convert.ToInt32(sessionStoreId);

            //ACTUALIZAR PRODUCTO PARA LA BD
            _context.Productos.Update(new Producto()
            {
                IdProducto = product.id,
                Nombre = product.name,
                Precio = Convert.ToDecimal(product.price),
                Stock = product.stock,
                IdCategoriaProducto = product.category,
                IdTienda = storeId,
                Estado = product.state
            });
            _context.SaveChanges();
            _notfy.Success("Producto actualizado.");

            return RedirectToAction("Find", new { id = product.id });
        }
        public IActionResult ChangeStateProduct(int id, bool state)
        {
            //OBTENER Y VALIDAR ID DE LA TIENDA ACTUAL
            string sessionStoreId = getFromSession(Constants.SESSION_STORE_ID_KEY);
            if (sessionStoreId.IsNullOrEmpty())
                return RedirectToAction("Index", "User");
            int storeId = Convert.ToInt32(sessionStoreId);

            //BUSCAR PRODUCTO POR TIENDA Y ID
            var found = _context.Productos.FirstOrDefault(p => p.IdTienda == storeId && p.IdProducto == id);
            if (found == null)
                return RedirectToAction("Find", new { id = id });

            //DESABILITAR ESTADO
            found.Estado = state;
            _context.Productos.Update(found);
            _context.SaveChanges();
            return RedirectToAction("Find", new { id = id });
        }

        //UTILS METHODS
        private static Product mapperProduct(Producto p)
        {
            return new Product()
            {
                id = p.IdProducto,
                name = p.Nombre,
                price = p.Precio,
                category = p.IdCategoriaProducto,
                stock = p.Stock,
                state = p.Estado
            };
        }

        private string getFromSession(string key) => HttpContext.Session.GetString(key)!;
        private int getSessionStoreId() => Convert.ToInt32(getFromSession(Constants.SESSION_STORE_ID_KEY));
        private string getSessionStoreName() => getFromSession(Constants.SESSION_STORE_NAME_KEY)!;
        private int getSessionUserId() => Convert.ToInt32(getFromSession(Constants.SESSION_USER_ID_KEY));
    }
}
