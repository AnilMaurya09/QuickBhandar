using Microsoft.AspNetCore.Mvc;
using QuickBhandarWeb.Models;

namespace QuickBhandarWeb.Controllers
{
    public class AuthController : Controller
    {
        private const string ValidOTP = "1234";

        [HttpPost]
        public IActionResult SendOtp([FromBody] OTPLoginViewModel model)
        {
            if (model.MobileNumber?.Length == 10)
            {
                // Send OTP logic can be added here (for now, assume static 1234)
                HttpContext.Session.SetString("MobileNumber", model.MobileNumber);
                return Json(new { success = true, message = "OTP Sent" });
            }
            return Json(new { success = false, message = "Invalid Mobile Number" });
        }

        [HttpPost]
        public IActionResult VerifyOtp([FromBody] OTPLoginViewModel model)
        {
            var sessionMobile = HttpContext.Session.GetString("MobileNumber");
            if (model.OTP == ValidOTP && sessionMobile == model.MobileNumber)
            {
                HttpContext.Session.SetString("IsLoggedIn", "true");
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Invalid OTP" });
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }

}
