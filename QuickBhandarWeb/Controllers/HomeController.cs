using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickBhandarWeb.Models;
using System.Diagnostics;

namespace QuickBhandarWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context,ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        public IActionResult Index()
        {
            var viewModel = new HomePageProductViewModel
            {
                TrendingProducts = _context.Products.Where(p => p.IsTrending).ToList(),
                BestSellingProducts = _context.Products.Where(p => p.IsBestSelling).ToList(),
                JustArrivedProducts = _context.Products.Where(p => p.IsJustArrived).ToList(),
                MostPopularProducts = _context.Products.Where(p => p.IsMostPopular).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AddToCart([FromBody] AddToCartRequest request)
        {
            // Check if user is logged in
            int userId = GetLoggedInUserId();
            if (userId <= 0)
            {
                return Json(new { success = false, message = "User not logged in" });
            }
            // ✅ Save to DB
            var existingCartItem = _context.Cart.FirstOrDefault(c => c.UserId == userId && c.ProductId == request.ProductId);
                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += request.Quantity;
                }
                else
                {
                    _context.Cart.Add(new Cart
                    {
                        UserId = userId,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity
                    });
                }
                _context.SaveChanges();
                return Json(new { success = true, message = "Added to Cart (DB)" });
            
        }

        [HttpGet]
        public IActionResult GetCartItems()
        {
            int userId = GetLoggedInUserId();
            if (userId <= 0)
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            var cartItems = _context.Cart
                .Where(c => c.UserId == userId)
                .Select(c => new
                {
                    c.Product.Id,
                    c.Product.Name,
                    c.Product.Description,
                    c.Product.Price,
                    c.Quantity
                }).ToList();

            return Json(new { success = true, items = cartItems });
        }
        [HttpPost]
        public IActionResult RemoveToCart([FromBody] int productId)
        {
            int userId = GetLoggedInUserId();
            if (userId <= 0)
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            var cartItem = _context.Cart.FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);
            if (cartItem != null)
            {
                _context.Cart.Remove(cartItem);
                _context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Item not found in cart" });
        }


        public IActionResult Checkout()
        {
            // ✅ Check login
            if (HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                // Save pending checkout flag
                HttpContext.Session.SetString("PendingCheckout", "true");
                return RedirectToAction("Index"); // Or return login popup
            }

            int userId = int.Parse(HttpContext.Session.GetString("UserId"));

            // ✅ Fetch user details
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            // ✅ Fetch cart with product details
            var cartItems = _context.Cart
                .Where(c => c.UserId == userId)
                .Join(_context.Products,
                    cart => cart.ProductId,
                    product => product.Id,
                    (cart, product) => new
                    {
                        product.Id,
                        product.Name,
                        product.Price,
                        cart.Quantity,
                        Total = cart.Quantity * product.Price
                    })
                .ToList();

            ViewBag.User = user;
            ViewBag.CartItems = cartItems;
            ViewBag.TotalAmount = cartItems.Sum(c => c.Total);

            return View();
        }

        [HttpPost]
        public IActionResult ToggleWishlist([FromQuery] int productId)
        {
            int userId = GetLoggedInUserId();
            if (userId <= 0)
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            var wishlistItem = _context.Wishlist.FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);

            if (wishlistItem == null)
            {
                // ✅ Add to wishlist
                _context.Wishlist.Add(new Wishlist { UserId = userId, ProductId = productId });
                _context.SaveChanges();
                return Json(new { success = true, added = true });
            }
            else
            {
                // ✅ Remove from wishlist
                _context.Wishlist.Remove(wishlistItem);
                _context.SaveChanges();
                return Json(new { success = true, added = false });
            }
        }
        public IActionResult Wishlist()
        {
            int userId = GetLoggedInUserId(); // ✅ Your helper to get user ID from session/cookie

            List<Product> wishlistProducts = new List<Product>();

            if (userId <= 0)
            {
                return Json(new { success = false, message = "User not logged in" });
            }
            
                wishlistProducts = (from w in _context.Wishlist
                                    join p in _context.Products on w.ProductId equals p.Id
                                    where w.UserId == userId
                                    select p).ToList();

            return View(wishlistProducts);
        }

        public IActionResult Orders()
        {
            return View();
        }
        [HttpPost]
        public IActionResult PlaceOrder([FromBody] OrderViewModel model)
        {
            int userId = GetLoggedInUserId();

            if (userId <= 0)
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            var cartItems = _context.Cart
         .Where(c => c.UserId == userId)
         .Include(c => c.Product)
         .ToList();
            if (!cartItems.Any())
            {
                return Json(new { success = false, message = "Your cart is empty." });
            }

            // Save Order
            var order = new Order
            {
                UserId = userId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                City = model.City,
                Zip = model.Zip,
                PaymentMethod = model.PaymentMethod,
                CreatedAt = DateTime.Now,
                TotalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity)
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // Save Order Items
            foreach (var item in cartItems)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = _context.Products.First(p => p.Id == item.ProductId).Price
                });
            }

            _context.Cart.RemoveRange(cartItems);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        public IActionResult TrackOrder()
        {
            return View();
        }

        private int GetLoggedInUserId()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return 0; // No user logged in
            return Convert.ToInt32(userId);
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
