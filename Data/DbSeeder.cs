using BloodDonationApp.Models;
using Microsoft.AspNetCore.Identity;

namespace BloodDonationApp.Data
{
    /// <summary>
    /// Seeds the Admin role and a default admin user on first run.
    ///
    /// Credentials are read exclusively from configuration (appsettings.json /
    /// environment variables). No fallback values are used — the app will throw
    /// on startup if any required key is missing, making misconfiguration obvious.
    ///
    /// Required config keys:
    ///   AdminSeed:Email
    ///   AdminSeed:Password
    ///   AdminSeed:FullName
    /// </summary>
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider services, IConfiguration config)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // ── 1. Ensure the Admin role exists ──────────────────────────────
            const string adminRole = "Admin";
            if (!await roleManager.RoleExistsAsync(adminRole))
                await roleManager.CreateAsync(new IdentityRole(adminRole));

            // ── 2. Read seed credentials — fail fast if not configured ────────
            var email    = config["AdminSeed:Email"]
                ?? throw new InvalidOperationException(
                    "AdminSeed:Email is not configured. Add it to appsettings.json.");

            var password = config["AdminSeed:Password"]
                ?? throw new InvalidOperationException(
                    "AdminSeed:Password is not configured. Add it to appsettings.json.");

            var fullName = config["AdminSeed:FullName"]
                ?? throw new InvalidOperationException(
                    "AdminSeed:FullName is not configured. Add it to appsettings.json.");

            // ── 3. Create admin user if not already present ───────────────────
            var existing = await userManager.FindByEmailAsync(email);
            if (existing == null)
            {
                var admin = new ApplicationUser
                {
                    UserName       = email,
                    Email          = email,
                    FullName       = fullName,
                    IsAvailable    = false,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, adminRole);
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create admin user: {errors}");
                }
            }
            else if (!await userManager.IsInRoleAsync(existing, adminRole))
            {
                await userManager.AddToRoleAsync(existing, adminRole);
            }
        }
    }
}
