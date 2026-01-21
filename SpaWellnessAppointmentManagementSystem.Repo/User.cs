using System.ComponentModel.DataAnnotations;

namespace SpaWellnessAppointmentManagementSystem.Repo.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; } // [cite: 107]

        [Required]
        [StringLength(50)]
        public string Username { get; set; } // [cite: 108]

        [Required]
        public string Password { get; set; } // This will store the hashed password [cite: 109]

        [Required]
        [EmailAddress]
        public string Email { get; set; } // [cite: 110]

        [Required]
        public string Role { get; set; } // Values: "ADMIN", "STAFF", "CUSTOMER" [cite: 111]

        public string? Phone { get; set; }
        public string? Location { get; set; }
    }
}