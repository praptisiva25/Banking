using System.ComponentModel.DataAnnotations;

namespace BankingAPI.Models;

public class CustomerProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [MaxLength(15)]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(100)]
    public string State { get; set; } = string.Empty;

    [MaxLength(10)]
    public string PostalCode { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}