using System.ComponentModel.DataAnnotations;

namespace BankingAPI.Models;

public class BankingProduct
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal InterestRate { get; set; }

    public decimal MinimumAmount { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}