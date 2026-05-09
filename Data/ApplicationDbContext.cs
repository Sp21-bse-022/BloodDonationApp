using BloodDonationApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationApp.Data
{
    /// <summary>
    /// Main EF Core DbContext. Inherits from IdentityDbContext so that
    /// ASP.NET Core Identity tables are managed alongside our own entities.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<BloodRequest> BloodRequests { get; set; }
        public DbSet<DonationHistory> DonationHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // must call base to configure Identity tables

            // BloodRequest → ApplicationUser (restrict delete so we don't cascade-delete requests)
            builder.Entity<BloodRequest>()
                .HasOne(r => r.User)
                .WithMany(u => u.BloodRequests)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // DonationHistory → ApplicationUser
            builder.Entity<DonationHistory>()
                .HasOne(d => d.Donor)
                .WithMany(u => u.DonationHistories)
                .HasForeignKey(d => d.DonorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index on BloodGroup for fast donor searches
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.BloodGroup);

            builder.Entity<BloodRequest>()
                .HasIndex(r => r.BloodGroup);

            builder.Entity<BloodRequest>()
                .HasIndex(r => r.Status);
        }
    }
}
