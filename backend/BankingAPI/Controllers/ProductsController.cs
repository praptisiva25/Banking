
using BankingAPI.Data;
using BankingAPI.DTOs.Products;
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
    public async Task<ActionResult<List<ProductResponseDto>>> GetProducts()
    {
        var products = await _db.BankingProducts
            .Where(p => p.IsActive)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Description = p.Description,
                InterestRate = p.InterestRate,
                MinimumAmount = p.MinimumAmount
            })
            .ToListAsync();

        return Ok(products);
    }

    // Get product by ID
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
    {
        var product = await _db.BankingProducts
            .Where(p => p.Id == id && p.IsActive)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Description = p.Description,
                InterestRate = p.InterestRate,
                MinimumAmount = p.MinimumAmount
            })
            .FirstOrDefaultAsync();

        if (product == null)
            return NotFound(new { message = "Product not found." });

        return Ok(product);
    }

    // Create product (Admin only)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductResponseDto>> CreateProduct(
        CreateProductRequestDto request)
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

        var response = new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Category = product.Category,
            Description = product.Description,
            InterestRate = product.InterestRate,
            MinimumAmount = product.MinimumAmount
        };

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            response);
    }
}
