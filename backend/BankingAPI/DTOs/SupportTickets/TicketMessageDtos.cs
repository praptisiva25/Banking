using System.ComponentModel.DataAnnotations;

namespace BankingAPI.DTOs.SupportTickets;

public class CreateTicketMessageRequestDto
{
    [Required]
    [StringLength(3000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;
}

public class TicketMessageResponseDto
{
    public int Id { get; set; }

    public int SupportTicketId { get; set; }

    public int SenderUserId { get; set; }

    public string SenderRole { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }
}