
using BankingAPI.Data;
using BankingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BankingAPI.Controllers;

[ApiController]
[Route("api/customer")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CustomerController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var profile = await _db.CustomerProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            return NotFound(new { message = "Profile not found." });

        return Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        CustomerProfileRequest request)
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var profile = await _db.CustomerProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            profile = new CustomerProfile { UserId = userId };
            _db.CustomerProfiles.Add(profile);
        }

        profile.PhoneNumber = request.PhoneNumber;
        profile.Address = request.Address;
        profile.City = request.City;
        profile.State = request.State;
        profile.PostalCode = request.PostalCode;
        profile.DateOfBirth = request.DateOfBirth;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Profile updated successfully.",
            profile
        });
    }
}

public record CustomerProfileRequest(
    string PhoneNumber,
    string Address,
    string City,
    string State,
    string PostalCode,
    DateOnly? DateOfBirth
);
