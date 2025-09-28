using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryAPI.Models.DTOs;
using InventoryAPI.Services.Interfaces;
using InventoryAPI.Exceptions;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ReportsController> _logger;
        
        public ReportsController(
            IReportService reportService, 
            IEmailService emailService,
            ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _emailService = emailService;
            _logger = logger;
        }
        
        /// <summary>
        /// Generate sales report for specified date range
        /// </summary>
        [HttpGet("sales")]
        public async Task<ActionResult<SalesReportDto>> GetSalesReport(
            [FromQuery] DateTime fromDate, 
            [FromQuery] DateTime toDate)
        {
            try
            {
                if (fromDate > toDate)
                    return BadRequest(new { message = "From date cannot be later than to date" });

                if (toDate > DateTime.UtcNow)
                    return BadRequest(new { message = "To date cannot be in the future" });

                var daysDifference = (toDate - fromDate).TotalDays;
                if (daysDifference > 365)
                    return BadRequest(new { message = "Date range cannot exceed 365 days" });

                var report = await _reportService.GenerateSalesReportAsync(fromDate, toDate);
                _logger.LogInformation("Generated sales report for period {FromDate} to {ToDate}", fromDate, toDate);
                
                return Ok(report);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid arguments for sales report: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sales report for period {FromDate} to {ToDate}", fromDate, toDate);
                return StatusCode(500, new { message = "Error generating sales report" });
            }
        }
        
        /// <summary>
        /// Generate current inventory report
        /// </summary>
        [HttpGet("inventory")]
        public async Task<ActionResult<InventoryReportDto>> GetInventoryReport()
        {
            try
            {
                var report = await _reportService.GenerateInventoryReportAsync();
                _logger.LogInformation("Generated inventory report");
                
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating inventory report");
                return StatusCode(500, new { message = "Error generating inventory report" });
            }
        }
        
        /// <summary>
        /// Generate customer report for specified date range
        /// </summary>
        [HttpGet("customers")]
        public async Task<ActionResult<CustomerReportDto>> GetCustomerReport(
            [FromQuery] DateTime fromDate, 
            [FromQuery] DateTime toDate)
        {
            try
            {
                if (fromDate > toDate)
                    return BadRequest(new { message = "From date cannot be later than to date" });

                var daysDifference = (toDate - fromDate).TotalDays;
                if (daysDifference > 365)
                    return BadRequest(new { message = "Date range cannot exceed 365 days" });

                var report = await _reportService.GenerateCustomerReportAsync(fromDate, toDate);
                _logger.LogInformation("Generated customer report for period {FromDate} to {ToDate}", fromDate, toDate);
                
                return Ok(report);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid arguments for customer report: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating customer report for period {FromDate} to {ToDate}", fromDate, toDate);
                return StatusCode(500, new { message = "Error generating customer report" });
            }
        }
        
        /// <summary>
        /// Generate profitability report for specified date range
        /// </summary>
        [HttpGet("profitability")]
        public async Task<ActionResult<ProfitabilityReportDto>> GetProfitabilityReport(
            [FromQuery] DateTime fromDate, 
            [FromQuery] DateTime toDate)
        {
            try
            {
                if (fromDate > toDate)
                    return BadRequest(new { message = "From date cannot be later than to date" });

                var daysDifference = (toDate - fromDate).TotalDays;
                if (daysDifference > 365)
                    return BadRequest(new { message = "Date range cannot exceed 365 days" });

                var report = await _reportService.GenerateProfitabilityReportAsync(fromDate, toDate);
                _logger.LogInformation("Generated profitability report for period {FromDate} to {ToDate}", fromDate, toDate);
                
                return Ok(report);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid arguments for profitability report: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating profitability report for period {FromDate} to {ToDate}", fromDate, toDate);
                return StatusCode(500, new { message = "Error generating profitability report" });
            }
        }
        
        /// <summary>
        /// Export sales report as PDF (Admin/Manager only)
        /// </summary>
        [HttpGet("sales/pdf")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> ExportSalesReportPdf(
            [FromQuery] DateTime fromDate, 
            [FromQuery] DateTime toDate)
        {
            try
            {
                if (fromDate > toDate)
                    return BadRequest(new { message = "From date cannot be later than to date" });

                var pdfBytes = await _reportService.ExportSalesReportToPdfAsync(fromDate, toDate);
                var fileName = $"SalesReport_{fromDate:yyyy-MM-dd}_to_{toDate:yyyy-MM-dd}.pdf";
                
                _logger.LogInformation("Exported sales report PDF for period {FromDate} to {ToDate}", fromDate, toDate);
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting sales report PDF for period {FromDate} to {ToDate}", fromDate, toDate);
                return BadRequest(new { message = "Failed to generate PDF report", error = ex.Message });
            }
        }
        
        /// <summary>
        /// Export inventory report as PDF (Admin/Manager only)
        /// </summary>
        [HttpGet("inventory/pdf")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> ExportInventoryReportPdf()
        {
            try
            {
                var pdfBytes = await _reportService.ExportInventoryReportToPdfAsync();
                var fileName = $"InventoryReport_{DateTime.Now:yyyy-MM-dd}.pdf";
                
                _logger.LogInformation("Exported inventory report PDF");
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting inventory report PDF");
                return BadRequest(new { message = "Failed to generate PDF report", error = ex.Message });
            }
        }
        
        /// <summary>
        /// Export sales report as Excel (Admin/Manager only)
        /// </summary>
        [HttpGet("sales/excel")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> ExportSalesReportExcel(
            [FromQuery] DateTime fromDate, 
            [FromQuery] DateTime toDate)
        {
            try
            {
                if (fromDate > toDate)
                    return BadRequest(new { message = "From date cannot be later than to date" });

                var excelBytes = await _reportService.ExportSalesReportToExcelAsync(fromDate, toDate);
                var fileName = $"SalesReport_{fromDate:yyyy-MM-dd}_to_{toDate:yyyy-MM-dd}.xlsx";
                
                _logger.LogInformation("Exported sales report Excel for period {FromDate} to {ToDate}", fromDate, toDate);
                
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting sales report Excel for period {FromDate} to {ToDate}", fromDate, toDate);
                return BadRequest(new { message = "Failed to generate Excel report", error = ex.Message });
            }
        }
        
        /// <summary>
        /// Export inventory report as Excel (Admin/Manager only)
        /// </summary>
        [HttpGet("inventory/excel")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> ExportInventoryReportExcel()
        {
            try
            {
                var excelBytes = await _reportService.ExportInventoryReportToExcelAsync();
                var fileName = $"InventoryReport_{DateTime.Now:yyyy-MM-dd}.xlsx";
                
                _logger.LogInformation("Exported inventory report Excel");
                
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting inventory report Excel");
                return BadRequest(new { message = "Failed to generate Excel report", error = ex.Message });
            }
        }
        
        /// <summary>
        /// Get top selling products for specified period
        /// </summary>
        [HttpGet("top-selling-products")]
        public async Task<ActionResult<IEnumerable<TopSellingProductDto>>> GetTopSellingProducts(
            [FromQuery] DateTime fromDate, 
            [FromQuery] DateTime toDate,
            [FromQuery] int count = 10)
        {
            try
            {
                if (fromDate > toDate)
                    return BadRequest(new { message = "From date cannot be later than to date" });

                if (count <= 0 || count > 100)
                    return BadRequest(new { message = "Count must be between 1 and 100" });

                var products = await _reportService.GetTopSellingProductsAsync(fromDate, toDate, count);
                _logger.LogInformation("Retrieved top {Count} selling products for period {FromDate} to {ToDate}", count, fromDate, toDate);
                
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top selling products for period {FromDate} to {ToDate}", fromDate, toDate);
                return StatusCode(500, new { message = "Error retrieving top selling products" });
            }
        }
        
        /// <summary>
        /// Get top customers for specified period
        /// </summary>
        [HttpGet("top-customers")]
        public async Task<ActionResult<IEnumerable<TopCustomerDto>>> GetTopCustomers(
            [FromQuery] DateTime fromDate, 
            [FromQuery] DateTime toDate,
            [FromQuery] int count = 10)
        {
            try
            {
                if (fromDate > toDate)
                    return BadRequest(new { message = "From date cannot be later than to date" });

                if (count <= 0 || count > 100)
                    return BadRequest(new { message = "Count must be between 1 and 100" });

                var customers = await _reportService.GetTopCustomersAsync(fromDate, toDate, count);
                _logger.LogInformation("Retrieved top {Count} customers for period {FromDate} to {ToDate}", count, fromDate, toDate);
                
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top customers for period {FromDate} to {ToDate}", fromDate, toDate);
                return StatusCode(500, new { message = "Error retrieving top customers" });
            }
        }
        
        /// <summary>
        /// Get current stock alerts
        /// </summary>
        [HttpGet("stock-alerts")]
        public async Task<ActionResult<IEnumerable<StockAlertDto>>> GetStockAlerts()
        {
            try
            {
                var alerts = await _reportService.GetStockAlertsAsync();
                _logger.LogInformation("Retrieved {Count} stock alerts", alerts.Count());
                
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock alerts");
                return StatusCode(500, new { message = "Error retrieving stock alerts" });
            }
        }

        /// <summary>
        /// Email stock alerts to specified recipients (Admin/Manager only)
        /// </summary>
        [HttpPost("email-stock-alerts")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> EmailStockAlerts([FromBody] EmailStockAlertsRequestDto emailRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var alerts = await _reportService.GetStockAlertsAsync();
                var alertsList = alerts.ToList();

                if (emailRequest.IncludeCriticalOnly)
                {
                    alertsList = alertsList.Where(a => a.AlertLevel == "Critical").ToList();
                }
                
                if (!alertsList.Any())
                    return Ok(new { message = "No stock alerts to send", alertCount = 0 });

                var success = await _emailService.SendStockAlertEmailAsync(alertsList);
                
                if (success)
                {
                    _logger.LogInformation("Stock alerts sent successfully to {RecipientCount} recipients", emailRequest.Recipients?.Count ?? 0);
                    return Ok(new { 
                        message = "Stock alerts sent successfully", 
                        alertCount = alertsList.Count,
                        recipients = emailRequest.Recipients?.Count ?? 0
                    });
                }
                else
                {
                    _logger.LogWarning("Failed to send stock alerts");
                    return BadRequest(new { message = "Failed to send stock alerts" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending stock alerts via email");
                return BadRequest(new { message = "Failed to send stock alerts", error = ex.Message });
            }
        }

        /// <summary>
        /// Get sales summary for quick overview
        /// </summary>
        [HttpGet("sales-summary")]
        public async Task<ActionResult> GetSalesSummary(
            [FromQuery] DateTime? fromDate = null, 
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                fromDate ??= DateTime.UtcNow.Date.AddDays(-30);
                toDate ??= DateTime.UtcNow.Date;

                if (fromDate > toDate)
                    return BadRequest(new { message = "From date cannot be later than to date" });

                var report = await _reportService.GenerateSalesReportAsync(fromDate.Value, toDate.Value);
                
                var summary = new
                {
                    period = new { from = fromDate, to = toDate },
                    totalSales = report.TotalSales,
                    totalTransactions = report.TotalTransactions,
                    averageTransaction = report.AverageTransactionValue,
                    totalDiscounts = report.TotalDiscounts,
                    totalTax = report.TotalTax,
                    dailyAverage = ((toDate.Value - fromDate.Value).TotalDays > 0) ? 
                        report.TotalSales / (decimal)(toDate.Value - fromDate.Value).TotalDays : 0,
                    transactionTrends = report.DailySales?.OrderBy(d => d.Date).Take(7),
                    topPaymentMethod = report.PaymentMethodBreakdown?.OrderByDescending(p => p.TotalAmount).FirstOrDefault()
                };
                
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sales summary for period {FromDate} to {ToDate}", fromDate, toDate);
                return StatusCode(500, new { message = "Error retrieving sales summary" });
            }
        }

        /// <summary>
        /// Get comprehensive business analytics
        /// </summary>
        [HttpGet("analytics")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> GetBusinessAnalytics([FromQuery] int days = 30)
        {
            try
            {
                if (days <= 0 || days > 365)
                    return BadRequest(new { message = "Days must be between 1 and 365" });

                var toDate = DateTime.UtcNow.Date;
                var fromDate = toDate.AddDays(-days);

                // Get all reports
                var salesReport = await _reportService.GenerateSalesReportAsync(fromDate, toDate);
                var inventoryReport = await _reportService.GenerateInventoryReportAsync();
                var customerReport = await _reportService.GenerateCustomerReportAsync(fromDate, toDate);
                var profitabilityReport = await _reportService.GenerateProfitabilityReportAsync(fromDate, toDate);

                var analytics = new
                {
                    period = new { from = fromDate, to = toDate, days },
                    salesPerformance = new
                    {
                        totalRevenue = salesReport.TotalSales,
                        totalTransactions = salesReport.TotalTransactions,
                        averageOrderValue = salesReport.AverageTransactionValue,
                        dailyAverage = salesReport.TotalSales / days,
                        growthTrend = CalculateGrowthTrend(salesReport.DailySales)
                    },
                    profitability = new
                    {
                        grossProfit = profitabilityReport.GrossProfit,
                        grossMargin = profitabilityReport.GrossProfitMargin,
                        totalCosts = profitabilityReport.TotalCostOfGoodsSold,
                        profitPerTransaction = profitabilityReport.TotalRevenue > 0 ? 
                            profitabilityReport.GrossProfit / salesReport.TotalTransactions : 0
                    },
                    inventory = new
                    {
                        totalValue = inventoryReport.TotalInventoryValue,
                        totalProducts = inventoryReport.TotalProducts,
                        stockAlerts = inventoryReport.LowStockProductsCount + inventoryReport.OutOfStockProductsCount,
                        turnoverRate = CalculateInventoryTurnover(salesReport, inventoryReport)
                    },
                    customers = new
                    {
                        totalCustomers = customerReport.TotalCustomers,
                        activeCustomers = customerReport.ActiveCustomers,
                        customerGrowth = customerReport.TotalCustomers - customerReport.ActiveCustomers,
                        averageLifetimeValue = customerReport.TopCustomers?.Any() == true ? 
                            customerReport.TopCustomers.Average(c => c.TotalSpent) : 0
                    },
                    topPerformers = new
                    {
                        products = salesReport.TopSellingProducts?.Take(5),
                        customers = customerReport.TopCustomers?.Take(5),
                        categories = profitabilityReport.CategoryProfitability?.OrderByDescending(c => c.GrossProfit).Take(5)
                    },
                    generatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Generated business analytics for {Days} days", days);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating business analytics for {Days} days", days);
                return StatusCode(500, new { message = "Error generating business analytics" });
            }
        }

        /// <summary>
        /// Get scheduled reports status and configuration
        /// </summary>
        [HttpGet("scheduled")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> GetScheduledReports()
        {
            try
            {
                // This would typically come from a database or configuration
                var scheduledReports = new
                {
                    dailyReports = new
                    {
                        enabled = true,
                        lastRun = DateTime.UtcNow.Date,
                        nextRun = DateTime.UtcNow.Date.AddDays(1),
                        recipients = new[] { "admin@company.com", "manager@company.com" }
                    },
                    weeklyReports = new
                    {
                        enabled = true,
                        lastRun = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek),
                        nextRun = DateTime.UtcNow.AddDays(7 - (int)DateTime.UtcNow.DayOfWeek),
                        recipients = new[] { "admin@company.com" }
                    },
                    monthlyReports = new
                    {
                        enabled = true,
                        lastRun = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1),
                        nextRun = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1),
                        recipients = new[] { "admin@company.com", "finance@company.com" }
                    },
                    alertReports = new
                    {
                        stockAlerts = true,
                        salesAlerts = true,
                        customerAlerts = false
                    }
                };

                return Ok(scheduledReports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scheduled reports configuration");
                return StatusCode(500, new { message = "Error retrieving scheduled reports" });
            }
        }

        /// <summary>
        /// Export comprehensive business report (Admin only)
        /// </summary>
        [HttpGet("comprehensive/pdf")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ExportComprehensiveReport(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                fromDate ??= DateTime.UtcNow.AddDays(-30);
                toDate ??= DateTime.UtcNow;

                if (fromDate > toDate)
                    return BadRequest(new { message = "From date cannot be later than to date" });

                // This would generate a comprehensive report combining all data
                // For now, we'll export the sales report as it's the most comprehensive
                var pdfBytes = await _reportService.ExportSalesReportToPdfAsync(fromDate.Value, toDate.Value);
                var fileName = $"ComprehensiveReport_{fromDate.Value:yyyy-MM-dd}_to_{toDate.Value:yyyy-MM-dd}.pdf";
                
                _logger.LogInformation("Exported comprehensive report PDF for period {FromDate} to {ToDate}", fromDate, toDate);
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting comprehensive report PDF for period {FromDate} to {ToDate}", fromDate, toDate);
                return BadRequest(new { message = "Failed to generate comprehensive report", error = ex.Message });
            }
        }

        #region Private Helper Methods

        private static string CalculateGrowthTrend(IEnumerable<DailySalesDto> dailySales)
        {
            if (dailySales?.Any() != true) return "No data";

            var salesList = dailySales.OrderBy(d => d.Date).ToList();
            if (salesList.Count < 2) return "Insufficient data";

            var firstHalf = salesList.Take(salesList.Count / 2).Sum(s => s.TotalSales);
            var secondHalf = salesList.Skip(salesList.Count / 2).Sum(s => s.TotalSales);

            if (firstHalf == 0) return secondHalf > 0 ? "Growing" : "Stable";
            
            var growthRate = ((secondHalf - firstHalf) / firstHalf) * 100;
            
            return growthRate switch
            {
                > 10 => "Strong Growth",
                > 5 => "Growing",
                > -5 => "Stable",
                > -10 => "Declining",
                _ => "Strong Decline"
            };
        }

        private static decimal CalculateInventoryTurnover(SalesReportDto salesReport, InventoryReportDto inventoryReport)
        {
            if (inventoryReport.TotalInventoryValue == 0) return 0;
            
            // Simple inventory turnover calculation: COGS / Average Inventory
            // In a real scenario, this would use more sophisticated calculations
            var estimatedCOGS = salesReport.TotalSales * 0.6m; // Assuming 60% COGS
            return estimatedCOGS / inventoryReport.TotalInventoryValue;
        }

        #endregion
    }
}

// Additional DTOs for Reports
namespace InventoryAPI.Models.DTOs
{
    public class EmailStockAlertsRequestDto
    {
        public List<string> Recipients { get; set; } = new List<string>();
        public string Subject { get; set; } = "Stock Alert Notification";
        public bool IncludeCriticalOnly { get; set; } = false;
        public string Notes { get; set; } = "";
    }

    public class ReportScheduleDto
    {
        public string ReportType { get; set; }
        public string Frequency { get; set; }
        public List<string> Recipients { get; set; } = new List<string>();
        public bool Enabled { get; set; }
        public DateTime? LastRun { get; set; }
        public DateTime? NextRun { get; set; }
    }

    public class BusinessAnalyticsDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int PeriodDays { get; set; }
        public SalesPerformanceDto SalesPerformance { get; set; }
        public ProfitabilityOverviewDto Profitability { get; set; }
        public InventoryOverviewDto Inventory { get; set; }
        public CustomerOverviewDto Customers { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class SalesPerformanceDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal DailyAverage { get; set; }
        public string GrowthTrend { get; set; }
    }

    public class ProfitabilityOverviewDto
    {
        public decimal GrossProfit { get; set; }
        public decimal GrossMargin { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal ProfitPerTransaction { get; set; }
    }

    public class InventoryOverviewDto
    {
        public decimal TotalValue { get; set; }
        public int TotalProducts { get; set; }
        public int StockAlerts { get; set; }
        public decimal TurnoverRate { get; set; }
    }

    public class CustomerOverviewDto
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int CustomerGrowth { get; set; }
        public decimal AverageLifetimeValue { get; set; }
    }
}