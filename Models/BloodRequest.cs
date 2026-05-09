using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BloodDonationApp.Models
{
    public enum UrgencyLevel
    {
        Normal = 1,
        Urgent = 2,
        Critical = 3
    }

    public enum RequestStatus
    {
        Open,
        Fulfilled,
        Cancelled
    }

    public class BloodRequest
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Patient Name")]
        public string PatientName { get; set; } = string.Empty;

        [Required]
        [StringLength(5)]
        [Display(Name = "Blood Group")]
        public string BloodGroup { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Hospital { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Range(1, 20)]
        [Display(Name = "Units Needed")]
        public int UnitsNeeded { get; set; }

        [Display(Name = "Urgency Level")]
        public UrgencyLevel UrgencyLevel { get; set; } = UrgencyLevel.Normal;

        [Required]
        [Phone]
        [StringLength(20)]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        public RequestStatus Status { get; set; } = RequestStatus.Open;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key to ApplicationUser
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }
    }
}
