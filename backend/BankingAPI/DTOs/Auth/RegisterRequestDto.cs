
using System.ComponentModel.DataAnnotations;

namespace BankingAPI.DTOs.Auth;

public class RegisterRequestDto
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(12)]
    public string Password { get; set; } = string.Empty;
}
