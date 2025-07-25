namespace QuickBhandarWeb.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MobileNumber { get; set; }
        public string Role { get; set; } // "Admin" or "User"
        public string OTP { get; set; } // Temporary OTP
        public DateTime? OTPExpiry { get; set; } // OTP valid for 5 min
    }
}
