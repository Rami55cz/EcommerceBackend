using System.ComponentModel.DataAnnotations;

public class UserProfile
    {
        public int Id { get; set; }

        [Required]
        public string? GitHubId { get; set; }

        [Required]
        public string? Username { get; set; }

        // Editable by the user
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        // Role: "customer", "administrator", or "vendor"
        [Required]
        public string? Role { get; set; }
    }