using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using QuickBhandarWeb.Models;

namespace QuickBhandarWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public IActionResult Index()
        {
            var jsonPath = Path.Combine(_env.WebRootPath, "data", "products.json");
            var jsonData = System.IO.File.ReadAllText(jsonPath);
            var allProducts = JsonSerializer.Deserialize<List<Product>>(jsonData);

            var viewModel = new HomePageProductViewModel
            {
                TrendingProducts = allProducts.Where(p => p.IsTrending).ToList(),
                BestSellingProducts = allProducts.Where(p => p.IsBestSelling).ToList(),
                JustArrivedProducts = allProducts.Where(p => p.IsJustArrived).ToList(),
                MostPopularProducts = allProducts.Where(p => p.IsMostPopular).ToList()
            };

            return View(viewModel);
        }
        public IActionResult Checkout()
        {
            return View();
        }
        public IActionResult Wishlist(string ids)
        {
            var jsonPath = Path.Combine(_env.WebRootPath, "data", "products.json");
            var jsonData = System.IO.File.ReadAllText(jsonPath);
            var allProducts = JsonSerializer.Deserialize<List<Product>>(jsonData);

            // Handle case when no IDs passed
            if (string.IsNullOrEmpty(ids))
            {
                return View(new List<Product>());
            }

            // Parse comma-separated IDs
            var wishlistIds = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(id => int.TryParse(id, out var parsedId) ? parsedId : -1)
                                 .Where(id => id > 0)
                                 .ToList();

            var wishlistProducts = allProducts
                .Where(p => wishlistIds.Contains(p.Id))
                .ToList();

            return View(wishlistProducts);
        }
        public IActionResult Orders()
        {
            return View();
        }
        public IActionResult TrackOrder()
        {
            return View();
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
