namespace BloodDonationApp.ViewModels
{
    public class AdminUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? BloodGroup { get; set; }
        public DateTime? LastDonationDate { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsAdmin { get; set; }
        public int TotalRequests { get; set; }
        public int TotalDonations { get; set; }
    }
}
