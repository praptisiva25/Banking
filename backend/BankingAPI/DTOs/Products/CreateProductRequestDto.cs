
using System.ComponentModel.DataAnnotations;

namespace BankingAPI.DTOs.Products;

public class CreateProductRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "100")]
    public decimal InterestRate { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal MinimumAmount { get; set; }
}
