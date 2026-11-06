
using System.ComponentModel.DataAnnotations;

namespace BankingAPI.Models;

public class AuditLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    [Required, MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    public int? EntityId { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
