using Microsoft.AspNetCore.Identity;

namespace Clinic_Manager.Data;

public class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        string[] roleNames = { "Admin", "Lekarz", "Rejestratorka" };

        // Tworzenie ról, jeśli nie istnieją
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);

            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Tworzenie domyślnego konta Admina
        string adminEmail = "admin@clinic.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var user = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            // Ustaw hasło zgodne z polityką z Program.cs
            var createAdmin = await userManager.CreateAsync(user, "SecureAdmin123!");

            if (createAdmin.Succeeded)
            {
                // Przypisanie roli Admin
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}
