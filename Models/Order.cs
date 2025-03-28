using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Models{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Pending"; // Shipped, Delivered

        public int UserId { get; set; }
        public UserProfile? User { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}