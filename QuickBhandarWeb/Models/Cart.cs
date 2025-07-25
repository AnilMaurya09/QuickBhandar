using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickBhandarWeb.Models
{
        public class Cart
        {
            public int Id { get; set; }

            [Required]
            public int UserId { get; set; }

            [Required]
            public int ProductId { get; set; }

            [Required]
            [Range(1, 100)]
            public int Quantity { get; set; } = 1;

            public DateTime CreatedAt { get; set; } = DateTime.Now;

            // Navigation Properties
            [ForeignKey("UserId")]
            public virtual User User { get; set; }

            [ForeignKey("ProductId")]
            public virtual Product Product { get; set; }
        }
    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

}
