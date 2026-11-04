
using BankingAPI.Data;
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
    public async Task<IActionResult> Submit(ApplicationRequest request)
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var product = await _db.BankingProducts
            .FirstOrDefaultAsync(p => p.Id == request.BankingProductId && p.IsActive);

        if (product == null)
            return NotFound(new { message = "Product not found." });

        if (request.RequestedAmount < product.MinimumAmount)
            return BadRequest(new
            {
                message = $"Minimum application amount is {product.MinimumAmount}."
            });

        if (request.RequestedAmount <= 0)
            return BadRequest(new { message = "Amount must be greater than zero." });

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

        return Ok(new
        {
            message = "Application submitted successfully.",
            application
        });
    }

    // Customer views their own applications
    [HttpGet("my")]
    public async Task<IActionResult> GetMyApplications()
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var applications = await _db.ProductApplications
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AppliedAt)
            .Select(a => new
            {
                a.Id,
                Product = _db.BankingProducts
                    .Where(p => p.Id == a.BankingProductId)
                    .Select(p => p.Name)
                    .FirstOrDefault(),
                a.RequestedAmount,
                a.Status,
                a.Remarks,
                a.AppliedAt,
                a.ReviewedAt
            })
            .ToListAsync();

        return Ok(applications);
    }

    // Admin views all applications
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllApplications()
    {
        var applications = await _db.ProductApplications
            .OrderByDescending(a => a.AppliedAt)
            .Select(a => new
            {
                a.Id,
                a.UserId,
                a.BankingProductId,
                a.RequestedAmount,
                a.Status,
                a.Remarks,
                a.AppliedAt
            })
            .ToListAsync();

        return Ok(applications);
    }

    // Admin approves or rejects an application
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        UpdateApplicationStatusRequest request)
    {
        var application = await _db.ProductApplications
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
            return NotFound(new { message = "Application not found." });

        if (request.Status != "Approved" && request.Status != "Rejected")
            return BadRequest(new { message = "Invalid status." });

        application.Status = request.Status;
        application.ReviewedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Application status updated.",
            application.Id,
            application.Status
        });
    }
}

public record ApplicationRequest(
    int BankingProductId,
    decimal RequestedAmount,
    string? Remarks
);

public record UpdateApplicationStatusRequest(string Status);
