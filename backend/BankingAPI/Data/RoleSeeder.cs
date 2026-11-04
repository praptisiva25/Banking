
using BankingAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace BankingAPI.Data;

public static class RoleSeeder
{
    public static async Task SeedAsync(
        RoleManager<IdentityRole<int>> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        string[] roles = { "Customer", "Officer", "Admin" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole<int>(role));

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        // Backfill the existing test customer.
        var testCustomer = await userManager.FindByEmailAsync("test@example.com");

        if (testCustomer != null &&
            !await userManager.IsInRoleAsync(testCustomer, "Customer"))
        {
            var result = await userManager.AddToRoleAsync(
                testCustomer, "Customer");

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
