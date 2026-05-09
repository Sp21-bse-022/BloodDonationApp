using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BloodDonationApp.Models
{
    /// <summary>
    /// Extends IdentityUser with blood donation-specific profile fields.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(5)]
        public string? BloodGroup { get; set; }   // e.g. "A+", "O-"

        public DateTime? LastDonationDate { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Navigation properties
        public ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();
        public ICollection<DonationHistory> DonationHistories { get; set; } = new List<DonationHistory>();
    }
}
