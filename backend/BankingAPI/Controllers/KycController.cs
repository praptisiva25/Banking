
using BankingAPI.Data;
using BankingAPI.DTOs.Kyc;
using BankingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BankingAPI.Services;

namespace BankingAPI.Controllers;

[ApiController]
[Route("api/kyc")]
[Authorize]
public class KycController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly AuditLogService _auditLog;

    public KycController(
        ApplicationDbContext db,
        IWebHostEnvironment environment,
        AuditLogService auditLog)
    {
        _db = db;
        _environment = environment;
        _auditLog = auditLog;
    }

    // Customer uploads KYC document
    [HttpPost("upload")]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] KycUploadRequestDto request)
    {
        var allowedTypes = new[] { "PAN", "Aadhaar" };

        if (!allowedTypes.Contains(request.DocumentType))
            return BadRequest(new
            {
                message = "Invalid document type."
            });

        if (file == null || file.Length == 0 ||
            file.Length > 5_000_000)
        {
            return BadRequest(new
            {
                message = "File must be between 1 byte and 5 MB."
            });
        }

        var extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        if (extension != ".pdf" && extension != ".jpg" &&
            extension != ".jpeg" && extension != ".png")
        {
            return BadRequest(new
            {
                message = "Only PDF, JPG and PNG files are allowed."
            });
        }

        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var uploadPath = Path.Combine(
            _environment.ContentRootPath,
            "PrivateUploads",
            "Kyc");

        Directory.CreateDirectory(uploadPath);

        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(uploadPath, storedFileName);

        try
        {
            using (var stream = new FileStream(
                fullPath, FileMode.CreateNew))
            {
                await file.CopyToAsync(stream);
            }

            var document = new KycDocument
            {
                UserId = userId,
                DocumentType = request.DocumentType,
                FileName = Path.GetFileName(file.FileName),
                FilePath = fullPath,
                Status = "Pending"
            };

            _db.KycDocuments.Add(document);
            await _db.SaveChangesAsync();

            await _auditLog.LogAsync(
                userId,
                "KYC Document Uploaded",
                "KycDocument",
                document.Id,
                $"Customer uploaded a {document.DocumentType} document."
            );

            return Ok(new
            {
                message = "KYC document uploaded successfully.",
                document = new KycDocumentResponseDto
                {
                    Id = document.Id,
                    DocumentType = document.DocumentType,
                    FileName = document.FileName,
                    Status = document.Status,
                    UploadedAt = document.UploadedAt,
                    ReviewedAt = document.ReviewedAt
                }
            });
        }
        catch
        {
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            throw;
        }
    }

    // Customer views their own documents
    [HttpGet("my")]
    public async Task<ActionResult<List<KycDocumentResponseDto>>>
        GetMyDocuments()
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var documents = await _db.KycDocuments
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new KycDocumentResponseDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                Status = d.Status,
                UploadedAt = d.UploadedAt,
                ReviewedAt = d.ReviewedAt
            })
            .ToListAsync();

        return Ok(documents);
    }

    // Admin and Officer view all documents
    [HttpGet("all")]
    [Authorize(Roles = "Admin,Officer")]
    public async Task<ActionResult<List<KycDocumentResponseDto>>>
        GetAllDocuments()
    {
        var documents = await _db.KycDocuments
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new KycDocumentResponseDto
            {
                Id = d.Id,
                UserId = d.UserId,
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                Status = d.Status,
                UploadedAt = d.UploadedAt,
                ReviewedAt = d.ReviewedAt
            })
            .ToListAsync();

        return Ok(documents);
    }

    // Admin and Officer review documents
    [HttpPut("{id:int}/review")]
    [Authorize(Roles = "Admin,Officer")]
    public async Task<IActionResult> ReviewDocument(
        int id,
        KycReviewRequestDto request)
    {
        var document = await _db.KycDocuments
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
            return NotFound(new
            {
                message = "Document not found."
            });

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
            document = new
            {
                document.Id,
                document.Status,
                document.ReviewedAt
            }
        });
    }
}
