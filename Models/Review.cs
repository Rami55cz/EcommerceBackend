using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Models{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int UserId { get; set; }
        public UserProfile? User { get; set; }
    }
}