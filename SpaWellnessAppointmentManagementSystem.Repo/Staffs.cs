using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaWellnessAppointmentManagementSystem.Repo.Models
{
    [Table("Staffs")]
    public class Staffs
    {
        [Key]
        public int StaffId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        // Added '?' to make these nullable
        public string? Name { get; set; }

        public string? Specialization { get; set; }

        public string? Bio { get; set; }

        // This was likely the cause of your last crash!
        public string? ProfilePicture { get; set; }

        public bool IsActive { get; set; }
    }
}