using EFSRT_IV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace EFSRT_IV.Controllers
{
    public class SellController : Controller
    {
        static List<SellDetail> details = new List<SellDetail>()
        {
            new SellDetail()
            {
                id = 1,
                product = "Cifrut",
                singlePrice = 2.00,
                quantity = 5,
                subtotal = 10.00
            },
            new SellDetail()
            {
                id = 2,
                product = "Cifrut",
                singlePrice = 2.00,
                quantity = 5,
                subtotal = 10.00
            },
            new SellDetail()
            {
                id = 3,
                product = "Cifrut",
                singlePrice = 2.00,
                quantity = 5,
                subtotal = 10.00
            },
            new SellDetail()
            {
                id = 4,
                product = "Cifrut",
                singlePrice = 2.00,
                quantity = 5,
                subtotal = 10.00
            },
            new SellDetail()
            {
                id = 5,
                product = "Cifrut",
                singlePrice = 2.00,
                quantity = 5,
                subtotal = 10.00
            },
            new SellDetail()
            {
                id = 6,
                product = "Cifrut",
                singlePrice = 2.00,
                quantity = 5,
                subtotal = 10.00
            }
        };

        static List<Sell> list = new List<Sell>()
        {
            new Sell()
            {
                id = 1,
                client = "uno",
                total = 20.50,
                date = DateTime.Now,
                details = details
            },
            new Sell()
            {
                id = 2,
                client = "uno",
                total = 20.50,
                date = DateTime.Now,
                details = details
            },
            new Sell()
            {
                id = 3,
                client = "uno",
                total = 20.50,
                date = DateTime.Now,
                details = details
            },
            new Sell()
            {
                id = 4,
                client = "uno",
                total = 20.50,
                date = DateTime.Now,
                details = details
            },
            new Sell()
            {
                id = 5,
                client = "uno",
                total = 20.50,
                date = DateTime.Now,
                details = details
            },
        };

        public IActionResult FindAllSells()
        {
            return View(list);
        }

        public IActionResult FindSell(int id)
        {
            Sell? found = list.Find(x => x.id == id) ?? null;
            if (found == null) return RedirectToAction("FindAllSells");

            ViewBag.client = found.client;
            ViewBag.total = found.total;
            ViewBag.date = found.date;
            return View(found.details);
        }
    }
}
