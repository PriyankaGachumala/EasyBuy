using EasyBuy.Data;
using EasyBuy.Models;
using EasyBuy.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EasyBuy.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            MainPageViewModel MainVM = new MainPageViewModel()
            {
                Category = await _db.Category.ToListAsync(),
                Product = await _db.Product.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync(),
                Coupon = await _db.Coupon.Where(m => m.IsActive == true).ToListAsync()
            };
            return View(MainVM);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
