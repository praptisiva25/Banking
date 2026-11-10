using System.ComponentModel.DataAnnotations;

namespace BankingAPI.Models;

public class TicketMessage
{
    public int Id { get; set; }

    public int SupportTicketId { get; set; }

    public int SenderUserId { get; set; }

    [Required, MaxLength(20)]
    public string SenderRole { get; set; } = string.Empty;

    [Required, MaxLength(3000)]
    public string Message { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}