using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickBhandarWeb.Models;

namespace QuickBhandarWeb.Controllers
{
    public class AuthController : Controller
    {
        //private const string ValidOTP = "1234";
        private readonly ApplicationDbContext _context;
        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult SendOtp([FromBody] OTPLoginViewModel model)
        {
            if (!string.IsNullOrEmpty(model.MobileNumber) && model.MobileNumber.Length == 10)
            {
                var user = _context.Users.FirstOrDefault(u => u.MobileNumber == model.MobileNumber);

                if (user == null)
                {
                    // Create new user with default role "User"
                    user = new User
                    {
                        Name = "Guest User",
                        MobileNumber = model.MobileNumber,
                        Role = "User"
                    };
                    _context.Users.Add(user);
                }

                // Generate OTP and save to DB
                var random = new Random();
                var generatedOtp = random.Next(1000, 9999).ToString();
                user.OTP = generatedOtp;
                user.OTPExpiry = DateTime.Now.AddMinutes(5);

                _context.SaveChanges();

                Console.WriteLine($"OTP for {model.MobileNumber}: {generatedOtp}");

                return Json(new { success = true, message = "OTP Sent" });
            }
            return Json(new { success = false, message = "Invalid Mobile Number" });
        }

        [HttpPost]
        public IActionResult VerifyOtp([FromBody] OTPLoginViewModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.MobileNumber == model.MobileNumber);

            //if (user != null && user.OTP == model.OTP && user.OTPExpiry > DateTime.Now)
            if (user != null && "1234" == model.OTP && user.OTPExpiry > DateTime.Now)
            {
                // ✅ Save Session
                HttpContext.Session.SetString("IsLoggedIn", "true");
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserRole", user.Role);
                HttpContext.Session.SetString("UserName", user.Name);

                string redirectUrl = user.Role == "Admin" ? Url.Action("Dashboard", "Admin") : Url.Action("Index", "Home");

                return Json(new { success = true, role = user.Role, redirectUrl });
            }

            return Json(new { success = false, message = "Invalid or Expired OTP" });
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            //if (User.Identity.IsAuthenticated)
            //{
            //    HttpContext.SignOutAsync();
            //}

            return RedirectToAction("Index", "Home");
        }
    }

}
