using QuickBhandarWeb.Models;

namespace QuickBhandarWeb.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }

        public bool IsTrending { get; set; }
        public bool IsBestSelling { get; set; }
        public bool IsJustArrived { get; set; }
        public bool IsMostPopular { get; set; }
    }
}
public class HomePageProductViewModel
{
    public List<Product> TrendingProducts { get; set; }
    public List<Product> BestSellingProducts { get; set; }
    public List<Product> JustArrivedProducts { get; set; }
    public List<Product> MostPopularProducts { get; set; }
}