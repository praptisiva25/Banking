
using BankingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingAPI.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (await db.BankingProducts.AnyAsync())
            return;

        var products = new List<BankingProduct>
        {
            new()
            {
                Name = "Personal Loan",
                Category = "Loan",
                Description = "Flexible personal loans for planned and unexpected expenses.",
                InterestRate = 10.5m,
                MinimumAmount = 50000
            },
            new()
            {
                Name = "Home Loan",
                Category = "Loan",
                Description = "Financing solutions for purchasing your dream home.",
                InterestRate = 8.5m,
                MinimumAmount = 500000
            },
            new()
            {
                Name = "Savings Account",
                Category = "Account",
                Description = "A savings account with competitive interest rates.",
                InterestRate = 3.5m,
                MinimumAmount = 1000
            },
            new()
            {
                Name = "Fixed Deposit",
                Category = "Deposit",
                Description = "Grow your savings with a fixed-term deposit.",
                InterestRate = 7.0m,
                MinimumAmount = 10000
            }
        };

        db.BankingProducts.AddRange(products);
        await db.SaveChangesAsync();
    }
}
