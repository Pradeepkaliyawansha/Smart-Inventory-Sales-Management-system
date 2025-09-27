using InventoryAPI.Models.DTOs;

namespace InventoryAPI.Services.Interfaces
{
    public interface IReportService
    {
        Task<SalesReportDto> GenerateSalesReportAsync(DateTime fromDate, DateTime toDate);
        Task<InventoryReportDto> GenerateInventoryReportAsync();
        Task<CustomerReportDto> GenerateCustomerReportAsync(DateTime fromDate, DateTime toDate);
        Task<ProfitabilityReportDto> GenerateProfitabilityReportAsync(DateTime fromDate, DateTime toDate);
        Task<byte[]> ExportSalesReportToPdfAsync(DateTime fromDate, DateTime toDate);
        Task<byte[]> ExportInventoryReportToPdfAsync();
        Task<byte[]> ExportSalesReportToExcelAsync(DateTime fromDate, DateTime toDate);
        Task<byte[]> ExportInventoryReportToExcelAsync();
        Task<IEnumerable<TopSellingProductDto>> GetTopSellingProductsAsync(DateTime fromDate, DateTime toDate, int count = 10);
        Task<IEnumerable<TopCustomerDto>> GetTopCustomersAsync(DateTime fromDate, DateTime toDate, int count = 10);
        Task<IEnumerable<StockAlertDto>> GetStockAlertsAsync();
    }

    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<IEnumerable<DailySalesDto>> GetWeeklySalesAsync();
        Task<IEnumerable<DailySalesDto>> GetMonthlySalesAsync();
        Task<IEnumerable<TopSellingProductDto>> GetTodayTopProductsAsync(int count = 5);
        Task<IEnumerable<StockAlertDto>> GetCriticalStockAlertsAsync();
        Task<decimal> GetTodayRevenueAsync();
        Task<decimal> GetMonthRevenueAsync();
        Task<int> GetTodayTransactionsAsync();
        Task<int> GetMonthTransactionsAsync();
    }

    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendStockAlertEmailAsync(List<StockAlertDto> stockAlerts);
        Task<bool> SendInvoiceEmailAsync(string customerEmail, InvoiceDto invoice);
        Task<bool> SendPasswordResetEmailAsync(string email, string resetToken);
        Task<bool> SendWelcomeEmailAsync(string email, string username, string temporaryPassword);
    }
}