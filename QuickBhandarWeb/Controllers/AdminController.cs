using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
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
            ViewBag.Categories = _context.Products
                                .Select(p => p.Category)
                                .Distinct()
                                .ToList();
            ViewBag.Products = products;
            return View(product);
        }

        [HttpGet]
        public IActionResult GetProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(x => x.Id == id);
            if (product == null) return NotFound();
            return Json(product);
        }
        [HttpPost]
    public async Task<IActionResult> SaveProduct(Product product, IFormFile ImageFile, [FromServices] Cloudinary cloudinary)
    {
        ModelState.Remove("ImageUrl");
        ModelState.Remove("ImageFile");

        if (product.Id == 0 && (ImageFile == null || ImageFile.Length == 0))
        {
            ModelState.AddModelError("ImageFile", "Please upload an image for new product.");
        }

        if (ModelState.IsValid)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                using var stream = ImageFile.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(ImageFile.FileName, stream),
                    Folder = "products",
                    Transformation = new Transformation().Width(1024).Height(1024).Crop("limit")
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);
                var cloudImageUrl = uploadResult.SecureUrl.ToString();

                // ✅ Delete old image from Cloudinary if edit mode
                if (product.Id != 0)
                {
                    var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == product.Id);
                    if (existingProduct != null && !string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        var oldUrl = existingProduct.ImageUrl;
                        var urlParts = oldUrl.Split('/');
                        var fileName = Path.GetFileNameWithoutExtension(urlParts.Last());
                        var folderIndex = urlParts.ToList().FindIndex(x => x == "upload") + 1;
                        var publicId = string.Join("/", urlParts.Skip(folderIndex)).Split('.')[0];

                        await cloudinary.DestroyAsync(new DeletionParams(publicId));
                    }
                }

                product.ImageUrl = cloudImageUrl;
            }
            else if (product.Id != 0)
            {
                    var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == product.Id);
                    if (existingProduct != null)
                {
                    product.ImageUrl = existingProduct.ImageUrl;
                }
            }

            if (product.Id == 0)
                _context.Products.Add(product);
            else
                _context.Products.Update(product);

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Products");
    }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id, [FromServices] Cloudinary cloudinary)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                // ✅ Delete image from Cloudinary
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    try
                    {
                        // Extract public ID from URL
                        var urlParts = product.ImageUrl.Split('/');
                        var fileNameWithExt = urlParts.Last();
                        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileNameWithExt);

                        // Get folder if exists (like products)
                        var folderIndex = urlParts.ToList().FindIndex(x => x == "upload") + 1;
                        var folderAndFile = string.Join("/", urlParts.Skip(folderIndex).Take(urlParts.Length - folderIndex - 1)) + "/" + fileNameWithoutExt;

                        var delParams = new DeletionParams(folderAndFile);
                        await cloudinary.DestroyAsync(delParams);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting from Cloudinary: {ex.Message}");
                    }
                }

                // ✅ Remove product from DB
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
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
