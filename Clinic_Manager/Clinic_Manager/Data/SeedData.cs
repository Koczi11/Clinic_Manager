using Clinic_Manager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Manager.Data;

public class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        string[] roleNames = { "Admin", "Lekarz", "Rejestratorka" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);

            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

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

            var createAdmin = await userManager.CreateAsync(user, "SecureAdmin123!");

            if (createAdmin.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }

        await SeedPatients(context);
        await SeedMedications(context);
    }

     private static async Task SeedPatients(ApplicationDbContext context)
    {
        if (await context.Patients.AnyAsync())
        {
            return;
        }

        var patients = new List<Patient>
        {
            new() { Pesel = "90010112345", FirstName = "Jan", LastName = "Kowalski", DateOfBirth = new DateOnly(1990, 1, 1), PhoneNumber = "601234567", Email = "jan.kowalski@example.com", Address = "ul. Kwiatowa 12, Warszawa" },
            new() { Pesel = "85052254321", FirstName = "Anna", LastName = "Nowak", DateOfBirth = new DateOnly(1985, 5, 22), PhoneNumber = "602345678", Email = "anna.nowak@example.com", Address = "ul. Słoneczna 5, Kraków" },
            new() { Pesel = "78113098765", FirstName = "Piotr", LastName = "Wiśniewski", DateOfBirth = new DateOnly(1978, 11, 30), PhoneNumber = "603456789", Email = "piotr.wisniewski@example.com", Address = "ul. Lipowa 8, Gdańsk" },
            new() { Pesel = "95030567890", FirstName = "Katarzyna", LastName = "Wójcik", DateOfBirth = new DateOnly(1995, 3, 5), PhoneNumber = "604567890", Email = "katarzyna.wojcik@example.com", Address = "ul. Polna 3, Poznań" },
            new() { Pesel = "82072412312", FirstName = "Tomasz", LastName = "Kamiński", DateOfBirth = new DateOnly(1982, 7, 24), PhoneNumber = "605678901", Email = "tomasz.kaminski@example.com", Address = "ul. Leśna 17, Wrocław" },
            new() { Pesel = "99121598732", FirstName = "Magdalena", LastName = "Lewandowska", DateOfBirth = new DateOnly(1999, 12, 15), PhoneNumber = "606789012", Email = "magdalena.lewandowska@example.com", Address = "ul. Ogrodowa 22, Łódź" },
            new() { Pesel = "70040311223", FirstName = "Marek", LastName = "Zieliński", DateOfBirth = new DateOnly(1970, 4, 3), PhoneNumber = "607890123", Email = "marek.zielinski@example.com", Address = "ul. Krótka 9, Szczecin" },
            new() { Pesel = "88092845678", FirstName = "Agnieszka", LastName = "Szymańska", DateOfBirth = new DateOnly(1988, 9, 28), PhoneNumber = "608901234", Email = "agnieszka.szymanska@example.com", Address = "ul. Długa 41, Lublin" }
        };

        await context.Patients.AddRangeAsync(patients);
        await context.SaveChangesAsync();
    }

    // Seed przykładowego katalogu leków - tylko gdy tabela jest pusta.
    private static async Task SeedMedications(ApplicationDbContext context)
    {
        if (await context.Medications.AnyAsync())
        {
            return;
        }

        var medications = new List<Medication>
        {
            new() { Name = "Apap", Description = "Paracetamol 500 mg, lek przeciwbólowy i przeciwgorączkowy", UnitPrice = 8.99m, Unit = "tabletka" },
            new() { Name = "Ibuprom", Description = "Ibuprofen 200 mg, lek przeciwzapalny", UnitPrice = 12.50m, Unit = "tabletka" },
            new() { Name = "Aspiryna", Description = "Kwas acetylosalicylowy 500 mg", UnitPrice = 9.20m, Unit = "tabletka" },
            new() { Name = "Amoksiklav", Description = "Antybiotyk - amoksycylina z kwasem klawulanowym", UnitPrice = 24.99m, Unit = "tabletka" },
            new() { Name = "Augmentin", Description = "Antybiotyk o szerokim spektrum działania", UnitPrice = 28.40m, Unit = "tabletka" },
            new() { Name = "Polopiryna", Description = "Kwas acetylosalicylowy, przeciwgorączkowy", UnitPrice = 6.75m, Unit = "tabletka" },
            new() { Name = "Nurofen", Description = "Ibuprofen 400 mg", UnitPrice = 15.30m, Unit = "tabletka" },
            new() { Name = "Gripex", Description = "Lek na objawy przeziębienia i grypy", UnitPrice = 17.99m, Unit = "tabletka" },
            new() { Name = "Rutinoscorbin", Description = "Witamina C z rutyną, wzmacnia odporność", UnitPrice = 13.49m, Unit = "tabletka" },
            new() { Name = "No-Spa", Description = "Drotaweryna, lek rozkurczowy", UnitPrice = 19.90m, Unit = "tabletka" },
            new() { Name = "Xanax", Description = "Alprazolam, lek przeciwlękowy (na receptę)", UnitPrice = 22.00m, Unit = "tabletka" },
            new() { Name = "Metformina", Description = "Lek przeciwcukrzycowy", UnitPrice = 11.80m, Unit = "tabletka" },
            new() { Name = "Euthyrox", Description = "Lewotyroksyna, hormon tarczycy", UnitPrice = 14.60m, Unit = "tabletka" },
            new() { Name = "Syrop Herbapect", Description = "Syrop na kaszel mokry", UnitPrice = 16.20m, Unit = "ml" },
            new() { Name = "Maść Tribiotic", Description = "Maść z antybiotykiem do stosowania na skórę", UnitPrice = 21.50m, Unit = "g" }
        };

        await context.Medications.AddRangeAsync(medications);
        await context.SaveChangesAsync();
    }
}
