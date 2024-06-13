using EFSRT_IV.Models;
using Microsoft.AspNetCore.Mvc;

namespace EFSRT_IV.Controllers
{
    public class ProductsController : Controller
    {
        static List<Product> list = new List<Product>
        {
           new Product()
            {
                id = 1,
                name = "Test",
                image = "",
                price = Convert.ToDecimal(10.50),
                stock = 40,
                minStock = 5,
                maxStock = 80,
                state = true
            },
           new Product()
            {
                id = 2,
                name = "Test",
                image = "",
                price = Convert.ToDecimal(10.50),
                stock = 40,
                minStock = 5,
                maxStock = 80,
                state = true
            },
           new Product()
            {
                id = 3,
                name = "Test",
                image = "",
                price = Convert.ToDecimal(10.50),
                stock = 40,
                minStock = 5,
                maxStock = 80,
                state = true
            },
           new Product()
            {
                id = 4,
                name = "Test",
                image = "",
                price = Convert.ToDecimal(10.50),
                stock = 40,
                minStock = 5,
                maxStock = 80,
                state = true
            },
           new Product()
            {
                id = 5,
                name = "Test",
                image = "",
                price = Convert.ToDecimal(10.50),
                stock = 40,
                minStock = 5,
                maxStock = 80,
                state = true
            },
           new Product()
            {
                id = 6,
                name = "Test",
                image = "",
                price = Convert.ToDecimal(10.50),
                stock = 40,
                minStock = 5,
                maxStock = 80,
                state = true
            },
        };

        public IActionResult FindAllProducts(bool? showDisabled = null)
        {
            ViewBag.showDisabled = showDisabled ?? false;
            return View(list);
        }


        public IActionResult CreateProduct()
        {
            return View(new Product());
        }
        [HttpPost]
        public IActionResult CreateProduct(Product product)
        {
            list.Add(product);
            return RedirectToAction("FindAllProducts");
        }


        public IActionResult Find(int id)
        {
            Product? found = list.Find(x => x.id == id) ?? null;
            if (found == null) return RedirectToAction("Panel", "Store");
            return View(found);
        }


        public IActionResult UpdateProduct(int id)
        {
            Product? found = list.Find(x => x.id == id) ?? null;
            if (found == null) return RedirectToAction("Panel", "Store");

            list[found.id] = found;
            return View(found);
        }
        [HttpPost]
        public IActionResult UpdateProduct(Product product)
        {
            return RedirectToAction("Find", new { id = product.id });
        }
        

        public IActionResult DisableProduct(int id)
        {
            // ...
            return RedirectToAction("Find", new { id = id });
        }
    }
}
