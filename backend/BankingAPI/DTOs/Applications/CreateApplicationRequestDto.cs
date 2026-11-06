
using System.ComponentModel.DataAnnotations;

namespace BankingAPI.DTOs.Applications;

public class CreateApplicationRequestDto
{
    [Range(1, int.MaxValue)]
    public int BankingProductId { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal RequestedAmount { get; set; }

    [MaxLength(1000)]
    public string? Remarks { get; set; }
}
