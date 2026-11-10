
using System.ComponentModel.DataAnnotations;

namespace BankingAPI.DTOs.SupportTickets;

public class CreateSupportTicketRequestDto
{
    [Required, MaxLength(150)]
    public string Subject { get; set; } = string.Empty;

    [Required, MaxLength(3000)]
    public string Description { get; set; } = string.Empty;

    [RegularExpression("^(Low|Normal|High|Urgent)$")]
    public string Priority { get; set; } = "Normal";
}

public class UpdateTicketStatusRequestDto
{
    [Required]
    [RegularExpression("^(Open|InProgress|Resolved|Closed)$")]
    public string Status { get; set; } = string.Empty;
}

public class AssignTicketRequestDto
{
    [Range(1, int.MaxValue)]
    public int OfficerId { get; set; }
}

public class SupportTicketResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? AssignedOfficerId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
