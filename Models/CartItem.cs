using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Models{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public UserProfile? User { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }
    }
}