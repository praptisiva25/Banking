namespace BankingAPI.DTOs.Admin;

public class AdminDashboardDto
{
    public int TotalCustomers { get; set; }
    public int PendingApplications { get; set; }
    public int ApprovedApplications { get; set; }
    public int PendingKyc { get; set; }
    public List<RecentActivityDto> RecentActivity { get; set; } = new();
}