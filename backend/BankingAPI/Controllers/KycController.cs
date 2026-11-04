using BankingAPI.Data;
using BankingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BankingAPI.Controllers;

[ApiController]
[Route("api/kyc")]
[Authorize]
public class KycController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public KycController(
        ApplicationDbContext db,
        IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] string documentType)
    {
        var allowedTypes = new[] { "PAN", "Aadhaar" };

        if (!allowedTypes.Contains(documentType))
            return BadRequest(new { message = "Invalid document type." });

        if (file == null || file.Length == 0 || file.Length > 5_000_000)
            return BadRequest(new { message = "File must be between 1 byte and 5 MB." });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (extension != ".pdf" && extension != ".jpg" &&
            extension != ".jpeg" && extension != ".png")
            return BadRequest(new { message = "Only PDF, JPG and PNG files are allowed." });

        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var uploadPath = Path.Combine(
            _environment.ContentRootPath, "PrivateUploads", "Kyc");

        Directory.CreateDirectory(uploadPath);

        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(uploadPath, storedFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var document = new KycDocument
        {
            UserId = userId,
            DocumentType = documentType,
            FileName = Path.GetFileName(file.FileName),
            FilePath = fullPath,
            Status = "Pending"
        };

        _db.KycDocuments.Add(document);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "KYC document uploaded successfully.",
            document.Id,
            document.DocumentType,
            document.Status
        });
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyDocuments()
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var documents = await _db.KycDocuments
            .Where(d => d.UserId == userId)
            .Select(d => new
            {
                d.Id,
                d.DocumentType,
                d.FileName,
                d.Status,
                d.UploadedAt,
                d.ReviewedAt
            })
            .ToListAsync();

        return Ok(documents);
    }

    [HttpGet("all")]
[Authorize(Roles = "Admin,Officer")]
public async Task<IActionResult> GetAllDocuments()
{
    var documents = await _db.KycDocuments
        .OrderByDescending(d => d.UploadedAt)
        .Select(d => new
        {
            d.Id,
            d.UserId,
            d.DocumentType,
            d.FileName,
            d.Status,
            d.UploadedAt,
            d.ReviewedAt,
            d.ReviewedBy
        })
        .ToListAsync();

    return Ok(documents);
}

[HttpPut("{id:int}/review")]
[Authorize(Roles = "Admin,Officer")]
public async Task<IActionResult> ReviewDocument(
    int id,
    KycReviewRequest request)
{
    if (request.Status != "Verified" &&
        request.Status != "Rejected")
    {
        return BadRequest(new
        {
            message = "Status must be Verified or Rejected."
        });
    }

    var document = await _db.KycDocuments
        .FirstOrDefaultAsync(d => d.Id == id);

    if (document == null)
        return NotFound(new { message = "Document not found." });

    if (document.Status != "Pending")
        return BadRequest(new
        {
            message = "Only pending documents can be reviewed."
        });

    var reviewerId = int.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    document.Status = request.Status;
    document.ReviewedAt = DateTime.UtcNow;
    document.ReviewedBy = reviewerId;

    await _db.SaveChangesAsync();

    return Ok(new
    {
        message = "KYC document reviewed successfully.",
        document.Id,
        document.Status,
        document.ReviewedBy,
        document.ReviewedAt
    });
}
}

public record KycReviewRequest(string Status);