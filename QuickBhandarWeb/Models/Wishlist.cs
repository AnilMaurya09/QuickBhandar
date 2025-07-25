namespace QuickBhandarWeb.Models
{
    public class Wishlist
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int ProductId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ✅ Navigation properties (optional)
        public User User { get; set; }
        public Product Product { get; set; }
    }
}
