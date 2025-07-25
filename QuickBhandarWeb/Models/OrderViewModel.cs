using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuickBhandarWeb.Models
{
    public class OrderViewModel
    {
        // Billing Details
        [Required, MaxLength(100)]
        public string FirstName { get; set; }

        [Required, MaxLength(100)]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MaxLength(20)]
        public string Phone { get; set; }

        [Required, MaxLength(250)]
        public string Address { get; set; }

        [Required, MaxLength(100)]
        public string City { get; set; }

        [Required, MaxLength(20)]
        public string Zip { get; set; }

        [Required]
        public string PaymentMethod { get; set; }

        // Cart Items
        public List<CartItemVM> CartItems { get; set; } = new List<CartItemVM>();

        public decimal TotalAmount { get; set; }
    }

    public class CartItemVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal => Quantity * Price;
    }
}
