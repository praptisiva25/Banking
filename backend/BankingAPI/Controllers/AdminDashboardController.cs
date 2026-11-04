
using BankingAPI.Data;
using BankingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BankingAPI.DTOs.Admin;

namespace BankingAPI.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminDashboardController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var customers = await _userManager.GetUsersInRoleAsync("Customer");

        var pendingApplications = await _db.ProductApplications
            .CountAsync(a => a.Status == "Pending");

        var approvedApplications = await _db.ProductApplications
            .CountAsync(a => a.Status == "Approved");

        var pendingKyc = await _db.KycDocuments
            .CountAsync(k => k.Status == "Pending");

        var recentApplications = await (
            from application in _db.ProductApplications
            join user in _db.Users
                on application.UserId equals user.Id
            join product in _db.BankingProducts
                on application.BankingProductId equals product.Id
            orderby application.AppliedAt descending
            select new RecentActivityDto
            {
                Type = "Application",
                Description = $"{user.FullName} applied for {product.Name}",
                Status = application.Status,
                Timestamp = application.ReviewedAt ?? application.AppliedAt
            }
        ).Take(10).ToListAsync();

        var recentKyc = await (
            from document in _db.KycDocuments
            join user in _db.Users
                on document.UserId equals user.Id
            orderby document.UploadedAt descending
            select new RecentActivityDto
            {
                Type = "KYC",
                Description = $"{user.FullName} uploaded a {document.DocumentType} document",
                Status = document.Status,
                Timestamp = document.ReviewedAt ?? document.UploadedAt
            }
        ).Take(10).ToListAsync();

        var recentActivity = recentApplications
            .Concat(recentKyc)
            .OrderByDescending(a => a.Timestamp)
            .Take(5)
            .ToList();

        var dashboard = new AdminDashboardDto
        {
            TotalCustomers = customers.Count,
            PendingApplications = pendingApplications,
            ApprovedApplications = approvedApplications,
            PendingKyc = pendingKyc,
            RecentActivity = recentActivity
        };
        
        return Ok(dashboard);
    }
}

