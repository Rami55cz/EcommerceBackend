using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Models{
    public class Product{
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }

        public List<Review> Reviews { get; set; } = new();
    }
}