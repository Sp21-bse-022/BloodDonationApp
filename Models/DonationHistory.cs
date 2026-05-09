using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BloodDonationApp.Models
{
    public class DonationHistory
    {
        public int Id { get; set; }

        // Foreign key to ApplicationUser (the donor)
        [Required]
        public string DonorId { get; set; } = string.Empty;

        [ForeignKey(nameof(DonorId))]
        public ApplicationUser? Donor { get; set; }

        [Required]
        [Display(Name = "Donation Date")]
        public DateTime DonationDate { get; set; }

        [Required]
        [StringLength(150)]
        public string Hospital { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
