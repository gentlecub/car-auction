namespace CarAuction.Application.Interfaces;

public class DashboardStats
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalCars { get; set; }
    public int ActiveAuctions { get; set; }
    public int CompletedAuctions { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalBids { get; set; }
    public int NewUsersThisMonth { get; set; }
    public IEnumerable<MonthlyStats> MonthlyAuctions { get; set; } = new List<MonthlyStats>();
    public IEnumerable<MonthlyStats> MonthlyRevenue { get; set; } = new List<MonthlyStats>();
}

public class MonthlyStats
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public interface IDashboardService
{
    Task<DashboardStats> GetDashboardStatsAsync();
}
