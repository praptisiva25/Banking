
using BankingAPI.Data;
using BankingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankingAPI.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ProductsController(ApplicationDbContext db)
    {
        _db = db;
    }

    // List active products
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _db.BankingProducts
            .Where(p => p.IsActive)
            .ToListAsync();

        return Ok(products);
    }

    // Get product by ID
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _db.BankingProducts
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null)
            return NotFound(new { message = "Product not found." });

        return Ok(product);
    }

    // Create product (Admin only)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateProduct(
        BankingProductRequest request)
    {
        var product = new BankingProduct
        {
            Name = request.Name,
            Category = request.Category,
            Description = request.Description,
            InterestRate = request.InterestRate,
            MinimumAmount = request.MinimumAmount
        };

        _db.BankingProducts.Add(product);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            product);
    }
}

public record BankingProductRequest(
    string Name,
    string Category,
    string Description,
    decimal InterestRate,
    decimal MinimumAmount
);
