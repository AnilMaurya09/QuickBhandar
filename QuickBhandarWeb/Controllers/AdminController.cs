using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickBhandarWeb.Models;

namespace QuickBhandarWeb.Controllers
{
        //[Authorize(Roles = "Admin")]
        public class AdminController : Controller
        {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var totalUsers = _context.Users.Count();
            var totalOrders = _context.Orders.Count();
            var totalRevenue = _context.Orders.Sum(o => o.TotalAmount);

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;

            return View();
        }

        public IActionResult Products(int? id)
        {
            var products = _context.Products.ToList();
            Product product = new Product();

            if (id.HasValue) // Edit mode
            {
                product = _context.Products.Find(id.Value);
            }

            ViewBag.Products = products;
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> SaveProduct(Product product, IFormFile ImageFile)
        {
                ModelState.Remove("ImageUrl");
                if (product.Id == 0 && (ImageFile == null || ImageFile.Length == 0))
                {
                    ModelState.AddModelError("ImageFile", "Please upload an image for new product.");
                }

                if (ModelState.IsValid)
                {
                    // ✅ Upload new image if provided
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var fileName = Path.GetFileName(ImageFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/product", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        product.ImageUrl = "/images/product/" + fileName;
                    }
                    else if (product.Id != 0)
                    {
                        // ✅ For edit, keep existing ImageUrl
                        var existingProduct = await _context.Products.FindAsync(product.Id);
                        if (existingProduct != null)
                        {
                            product.ImageUrl = existingProduct.ImageUrl;
                        }
                    }

                    // ✅ Add or Update
                    if (product.Id == 0)
                        _context.Products.Add(product);
                    else
                        _context.Products.Update(product);

                    await _context.SaveChangesAsync();
                }

                // If model is invalid, redisplay view or modal
                return RedirectToAction("Products");
            }

        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);

            if (product != null)
            {
                // ✅ Delete image from wwwroot if exists
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                _context.Products.Remove(product);
                _context.SaveChanges();
            }

            return RedirectToAction("Products");
        }

        public IActionResult Orders()
        {
            var orders = _context.Orders
                .Select(o => new
                {
                    o.Id,
                    CustomerName = _context.Users.FirstOrDefault(u => u.Id == o.UserId).Name,
                    o.TotalAmount,
                    o.Status,
                    Date = DateTime.Now.ToShortDateString()
                }).ToList();

            ViewBag.AllOrders = orders;
            return View();
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = status;
                _context.SaveChanges();
            }
            return RedirectToAction("Orders");
        }

    }
}
