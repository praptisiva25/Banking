
using BankingAPI.Data;
using BankingAPI.DTOs.Applications;
using BankingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BankingAPI.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ApplicationsController(ApplicationDbContext db)
    {
        _db = db;
    }

    // Customer submits an application
    [HttpPost]
    public async Task<IActionResult> Submit(
        CreateApplicationRequestDto request)
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var product = await _db.BankingProducts
            .FirstOrDefaultAsync(p =>
                p.Id == request.BankingProductId && p.IsActive);

        if (product == null)
            return NotFound(new { message = "Product not found." });

        if (request.RequestedAmount < product.MinimumAmount)
            return BadRequest(new
            {
                message = $"Minimum application amount is {product.MinimumAmount}."
            });

        if (request.RequestedAmount <= 0)
            return BadRequest(new
            {
                message = "Amount must be greater than zero."
            });

        var application = new ProductApplication
        {
            UserId = userId,
            BankingProductId = request.BankingProductId,
            RequestedAmount = request.RequestedAmount,
            Remarks = request.Remarks,
            Status = "Pending"
        };

        _db.ProductApplications.Add(application);
        await _db.SaveChangesAsync();

        var response = new ApplicationResponseDto
        {
            Id = application.Id,
            UserId = application.UserId,
            BankingProductId = application.BankingProductId,
            Product = product.Name,
            RequestedAmount = application.RequestedAmount,
            Status = application.Status,
            Remarks = application.Remarks,
            AppliedAt = application.AppliedAt,
            ReviewedAt = application.ReviewedAt
        };

        return Ok(new
        {
            message = "Application submitted successfully.",
            application = response
        });
    }

    // Customer views their own applications
    [HttpGet("my")]
    public async Task<ActionResult<List<ApplicationResponseDto>>>
        GetMyApplications()
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var applications = await _db.ProductApplications
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AppliedAt)
            .Select(a => new ApplicationResponseDto
            {
                Id = a.Id,
                UserId = a.UserId,
                BankingProductId = a.BankingProductId,
                Product = _db.BankingProducts
                 .Where(p => p.Id == a.BankingProductId)
                 .Select(p => p.Name)
                 .FirstOrDefault() ?? string.Empty,
                RequestedAmount = a.RequestedAmount,
                Status = a.Status,
                Remarks = a.Remarks,
                AppliedAt = a.AppliedAt,
                ReviewedAt = a.ReviewedAt
            })
            .ToListAsync();

        return Ok(applications);
    }

    // Admin views all applications
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<ApplicationResponseDto>>>
        GetAllApplications()
    {
        var applications = await _db.ProductApplications
            .OrderByDescending(a => a.AppliedAt)
            .Select(a => new ApplicationResponseDto
            {
                Id = a.Id,
                UserId = a.UserId,
                BankingProductId = a.BankingProductId,
                Product = _db.BankingProducts
                 .Where(p => p.Id == a.BankingProductId)
                 .Select(p => p.Name)
                 .FirstOrDefault() ?? string.Empty,
                RequestedAmount = a.RequestedAmount,
                Status = a.Status,
                Remarks = a.Remarks,
                AppliedAt = a.AppliedAt,
                ReviewedAt = a.ReviewedAt
            })
            .ToListAsync();

        return Ok(applications);
    }

    // Admin approves or rejects an application
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        UpdateApplicationStatusRequestDto request)
    {
        var application = await _db.ProductApplications
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
            return NotFound(new
            {
                message = "Application not found."
            });

        application.Status = request.Status;
        application.ReviewedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Application status updated.",
            application = new
            {
                application.Id,
                application.Status
            }
        });
    }
}
