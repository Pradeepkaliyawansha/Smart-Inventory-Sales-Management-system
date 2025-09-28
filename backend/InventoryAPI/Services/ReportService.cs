
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using InventoryAPI.Data;
using InventoryAPI.Models.DTOs;
using InventoryAPI.Models.Enums;
using InventoryAPI.Services.Interfaces;
using InventoryAPI.Helpers;
using OfficeOpenXml;

namespace InventoryAPI.Services
{
    public class ReportService : IReportService
    {
        private readonly InventoryContext _context;
        private readonly IMapper _mapper;
        private readonly PdfGenerator _pdfGenerator;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            InventoryContext context,
            IMapper mapper,
            PdfGenerator pdfGenerator,
            ILogger<ReportService> logger)
        {
            _context = context;
            _mapper = mapper;
            _pdfGenerator = pdfGenerator;
            _logger = logger;
        }

        public async Task<SalesReportDto> GenerateSalesReportAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var sales = await _context.Sales
                    .Include(s => s.Customer)
                    .Include(s => s.User)
                    .Include(s => s.SaleItems)
                        .ThenInclude(si => si.Product)
                            .ThenInclude(p => p.Category)
                    .Where(s => s.SaleDate >= fromDate && s.SaleDate <= toDate && s.IsCompleted)
                    .ToListAsync();

                var report = new SalesReportDto
                {
                    ReportDate = DateTime.UtcNow,
                    FromDate = fromDate,
                    ToDate = toDate,
                    TotalSales = sales.Sum(s => s.TotalAmount),
                    TotalTransactions = sales.Count,
                    TotalDiscounts = sales.Sum(s => s.DiscountAmount),
                    TotalTax = sales.Sum(s => s.TaxAmount),
                    AverageTransactionValue = sales.Any() ? sales.Average(s => s.TotalAmount) : 0
                };

                // Payment method breakdown
                report.PaymentMethodBreakdown = sales
                    .GroupBy(s => s.PaymentMethod)
                    .Select(g => new PaymentMethodSummaryDto
                    {
                        PaymentMethod = g.Key,
                        TransactionCount = g.Count(),
                        TotalAmount = g.Sum(s => s.TotalAmount),
                        Percentage = report.TotalSales > 0 ? (g.Sum(s => s.TotalAmount) / report.TotalSales) * 100 : 0
                    })
                    .ToList();

                // Daily sales
                report.DailySales = sales
                    .GroupBy(s => s.SaleDate.Date)
                    .Select(g => new DailySalesDto
                    {
                        Date = g.Key,
                        TotalSales = g.Sum(s => s.TotalAmount),
                        TransactionCount = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                // Top selling products
                report.TopSellingProducts = sales
                    .SelectMany(s => s.SaleItems)
                    .GroupBy(si => new { si.ProductId, si.Product.Name, si.Product.Category.Name })
                    .Select(g => new TopSellingProductDto
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.Name,
                        CategoryName = g.Key.Name,
                        QuantitySold = g.Sum(si => si.Quantity),
                        TotalRevenue = g.Sum(si => si.TotalPrice),
                        UnitPrice = g.Average(si => si.UnitPrice)
                    })
                    .OrderByDescending(p => p.QuantitySold)
                    .Take(10)
                    .ToList();

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sales report for period {FromDate} to {ToDate}", fromDate, toDate);
                throw;
            }
        }

        public async Task<InventoryReportDto> GenerateInventoryReportAsync()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .Where(p => p.IsActive)
                    .ToListAsync();

                var report = new InventoryReportDto
                {
                    ReportDate = DateTime.UtcNow,
                    TotalProducts = products.Count,
                    TotalInventoryValue = products.Sum(p => p.StockQuantity * p.CostPrice),
                    LowStockProductsCount = products.Count(p => p.StockQuantity <= p.MinStockLevel),
                    OutOfStockProductsCount = products.Count(p => p.StockQuantity == 0)
                };

                // Product stock levels
                report.ProductStockLevels = products.Select(p => new ProductStockDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    SKU = p.SKU,
                    CategoryName = p.Category.Name,
                    CurrentStock = p.StockQuantity,
                    MinStockLevel = p.MinStockLevel,
                    IsLowStock = p.StockQuantity <= p.MinStockLevel,
                    IsOutOfStock = p.StockQuantity == 0,
                    UnitPrice = p.Price,
                    StockValue = p.StockQuantity * p.CostPrice,
                    LastUpdated = p.UpdatedAt
                }).ToList();

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating inventory report");
                throw;
            }
        }

        public async Task<CustomerReportDto> GenerateCustomerReportAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var customers = await _context.Customers
                    .Include(c => c.Sales.Where(s => s.SaleDate >= fromDate && s.SaleDate <= toDate))
                    .Where(c => c.IsActive)
                    .ToListAsync();

                var report = new CustomerReportDto
                {
                    ReportDate = DateTime.UtcNow,
                    TotalCustomers = customers.Count,
                    ActiveCustomers = customers.Count(c => c.Sales.Any()),
                    TotalLoyaltyPoints = customers.Sum(c => c.LoyaltyPoints),
                    TotalCreditBalance = customers.Sum(c => c.CreditBalance)
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating customer report for period {FromDate} to {ToDate}", fromDate, toDate);
                throw;
            }
        }

        public async Task<ProfitabilityReportDto> GenerateProfitabilityReportAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var sales = await _context.Sales
                    .Include(s => s.SaleItems)
                        .ThenInclude(si => si.Product)
                    .Where(s => s.SaleDate >= fromDate && s.SaleDate <= toDate && s.IsCompleted)
                    .ToListAsync();

                var totalRevenue = sales.Sum(s => s.TotalAmount);
                var totalCostOfGoodsSold = sales
                    .SelectMany(s => s.SaleItems)
                    .Sum(si => si.Quantity * si.Product.CostPrice);

                var report = new ProfitabilityReportDto
                {
                    ReportDate = DateTime.UtcNow,
                    FromDate = fromDate,
                    ToDate = toDate,
                    TotalRevenue = totalRevenue,
                    TotalCostOfGoodsSold = totalCostOfGoodsSold,
                    GrossProfit = totalRevenue - totalCostOfGoodsSold,
                    GrossProfitMargin = totalRevenue > 0 ? ((totalRevenue - totalCostOfGoodsSold) / totalRevenue) * 100 : 0
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating profitability report for period {FromDate} to {ToDate}", fromDate, toDate);
                throw;
            }
        }

        public async Task<byte[]> ExportSalesReportToPdfAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var report = await GenerateSalesReportAsync(fromDate, toDate);
                return _pdfGenerator.GenerateSalesReportPdf(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting sales report to PDF");
                throw;
            }
        }

        public async Task<byte[]> ExportInventoryReportToPdfAsync()
        {
            try
            {
                var report = await GenerateInventoryReportAsync();
                return _pdfGenerator.GenerateInventoryReportPdf(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting inventory report to PDF");
                throw;
            }
        }

        public async Task<byte[]> ExportSalesReportToExcelAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var report = await GenerateSalesReportAsync(fromDate, toDate);

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Sales Report");

                worksheet.Cells[1, 1].Value = "Sales Report";
                worksheet.Cells[2, 1].Value = $"Period: {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}";

                // Headers
                worksheet.Cells[4, 1].Value = "Metric";
                worksheet.Cells[4, 2].Value = "Value";

                // Data
                worksheet.Cells[5, 1].Value = "Total Sales";
                worksheet.Cells[5, 2].Value = report.TotalSales;
                worksheet.Cells[6, 1].Value = "Total Transactions";
                worksheet.Cells[6, 2].Value = report.TotalTransactions;

                return package.GetAsByteArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting sales report to Excel");
                throw;
            }
        }

        public async Task<byte[]> ExportInventoryReportToExcelAsync()
        {
            try
            {
                var report = await GenerateInventoryReportAsync();

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Inventory Report");

                worksheet.Cells[1, 1].Value = "Inventory Report";
                worksheet.Cells[2, 1].Value = $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm}";

                return package.GetAsByteArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting inventory report to Excel");
                throw;
            }
        }

        public async Task<IEnumerable<TopSellingProductDto>> GetTopSellingProductsAsync(DateTime fromDate, DateTime toDate, int count = 10)
        {
            try
            {
                var topProducts = await _context.Sales
                    .Where(s => s.SaleDate >= fromDate && s.SaleDate <= toDate && s.IsCompleted)
                    .SelectMany(s => s.SaleItems)
                    .GroupBy(si => new { si.ProductId, si.Product.Name, si.Product.Category.Name })
                    .Select(g => new TopSellingProductDto
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.Name,
                        CategoryName = g.Key.Name,
                        QuantitySold = g.Sum(si => si.Quantity),
                        TotalRevenue = g.Sum(si => si.TotalPrice),
                        UnitPrice = g.Average(si => si.UnitPrice)
                    })
                    .OrderByDescending(p => p.QuantitySold)
                    .Take(count)
                    .ToListAsync();

                return topProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top selling products");
                throw;
            }
        }

        public async Task<IEnumerable<TopCustomerDto>> GetTopCustomersAsync(DateTime fromDate, DateTime toDate, int count = 10)
        {
            try
            {
                var topCustomers = await _context.Customers
                    .Include(c => c.Sales.Where(s => s.SaleDate >= fromDate && s.SaleDate <= toDate))
                    .Where(c => c.Sales.Any())
                    .Select(c => new TopCustomerDto
                    {
                        CustomerId = c.Id,
                        CustomerName = c.Name,
                        Email = c.Email,
                        TotalTransactions = c.Sales.Count(),
                        TotalSpent = c.Sales.Sum(s => s.TotalAmount),
                        LastPurchaseDate = c.LastPurchaseDate,
                        LoyaltyPoints = c.LoyaltyPoints
                    })
                    .OrderByDescending(c => c.TotalSpent)
                    .Take(count)
                    .ToListAsync();

                return topCustomers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top customers");
                throw;
            }
        }

        public async Task<IEnumerable<StockAlertDto>> GetStockAlertsAsync()
        {
            try
            {
                var alerts = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive && (p.StockQuantity <= p.MinStockLevel || p.StockQuantity == 0))
                    .Select(p => new StockAlertDto
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        SKU = p.SKU,
                        CurrentStock = p.StockQuantity,
                        MinStockLevel = p.MinStockLevel,
                        IsOutOfStock = p.StockQuantity == 0,
                        AlertLevel = p.StockQuantity == 0 ? "Critical" : "Low"
                    })
                    .OrderBy(a => a.CurrentStock)
                    .ToListAsync();

                return alerts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock alerts");
                throw;
            }
        }
    }

    public class DashboardService : IDashboardService
    {
        private readonly InventoryContext _context;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(InventoryContext context, ILogger<DashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                var stats = new DashboardStatsDto
                {
                    TodaySales = await GetTodayRevenueAsync(),
                    MonthSales = await GetMonthRevenueAsync(),
                    TotalProducts = await _context.Products.CountAsync(p => p.IsActive),
                    LowStockProducts = await _context.Products.CountAsync(p => p.IsActive && p.StockQuantity <= p.MinStockLevel),
                    TotalCustomers = await _context.Customers.CountAsync(c => c.IsActive),
                    PendingOrders = 0 // Placeholder - would require order management system
                };

                stats.WeeklySales = (await GetWeeklySalesAsync()).ToList();
                stats.TopProducts = (await GetTodayTopProductsAsync(5)).ToList();
                stats.StockAlerts = (await GetCriticalStockAlertsAsync()).ToList();

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                throw;
            }
        }

        public async Task<IEnumerable<DailySalesDto>> GetWeeklySalesAsync()
        {
            try
            {
                var weekAgo = DateTime.UtcNow.Date.AddDays(-7);
                var today = DateTime.UtcNow.Date.AddDays(1);

                var sales = await _context.Sales
                    .Where(s => s.SaleDate >= weekAgo && s.SaleDate < today && s.IsCompleted)
                    .GroupBy(s => s.SaleDate.Date)
                    .Select(g => new DailySalesDto
                    {
                        Date = g.Key,
                        TotalSales = g.Sum(s => s.TotalAmount),
                        TransactionCount = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToListAsync();

                return sales;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weekly sales");
                throw;
            }
        }

        public async Task<IEnumerable<DailySalesDto>> GetMonthlySalesAsync()
        {
            try
            {
                var monthAgo = DateTime.UtcNow.Date.AddDays(-30);
                var today = DateTime.UtcNow.Date.AddDays(1);

                var sales = await _context.Sales
                    .Where(s => s.SaleDate >= monthAgo && s.SaleDate < today && s.IsCompleted)
                    .GroupBy(s => s.SaleDate.Date)
                    .Select(g => new DailySalesDto
                    {
                        Date = g.Key,
                        TotalSales = g.Sum(s => s.TotalAmount),
                        TransactionCount = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToListAsync();

                return sales;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly sales");
                throw;
            }
        }

        public async Task<IEnumerable<TopSellingProductDto>> GetTodayTopProductsAsync(int count = 5)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                var topProducts = await _context.Sales
                    .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow && s.IsCompleted)
                    .SelectMany(s => s.SaleItems)
                    .GroupBy(si => new { si.ProductId, si.Product.Name, si.Product.Category.Name })
                    .Select(g => new TopSellingProductDto
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.Name,
                        CategoryName = g.Key.Name,
                        QuantitySold = g.Sum(si => si.Quantity),
                        TotalRevenue = g.Sum(si => si.TotalPrice),
                        UnitPrice = g.Average(si => si.UnitPrice)
                    })
                    .OrderByDescending(p => p.QuantitySold)
                    .Take(count)
                    .ToListAsync();

                return topProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today's top products");
                throw;
            }
        }

        public async Task<IEnumerable<StockAlertDto>> GetCriticalStockAlertsAsync()
        {
            try
            {
                var alerts = await _context.Products
                    .Where(p => p.IsActive && p.StockQuantity == 0)
                    .Select(p => new StockAlertDto
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        SKU = p.SKU,
                        CurrentStock = p.StockQuantity,
                        MinStockLevel = p.MinStockLevel,
                        IsOutOfStock = true,
                        AlertLevel = "Critical"
                    })
                    .Take(10)
                    .ToListAsync();

                return alerts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting critical stock alerts");
                throw;
            }
        }

        public async Task<decimal> GetTodayRevenueAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                var revenue = await _context.Sales
                    .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow && s.IsCompleted)
                    .SumAsync(s => s.TotalAmount);

                return revenue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today's revenue");
                throw;
            }
        }

        public async Task<decimal> GetMonthRevenueAsync()
        {
            try
            {
                var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var nextMonth = thisMonth.AddMonths(1);

                var revenue = await _context.Sales
                    .Where(s => s.SaleDate >= thisMonth && s.SaleDate < nextMonth && s.IsCompleted)
                    .SumAsync(s => s.TotalAmount);

                return revenue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting month's revenue");
                throw;
            }
        }

        public async Task<int> GetTodayTransactionsAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                var count = await _context.Sales
                    .CountAsync(s => s.SaleDate >= today && s.SaleDate < tomorrow && s.IsCompleted);

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today's transactions");
                throw;
            }
        }

        public async Task<int> GetMonthTransactionsAsync()
        {
            try
            {
                var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var nextMonth = thisMonth.AddMonths(1);

                var count = await _context.Sales
                    .CountAsync(s => s.SaleDate >= thisMonth && s.SaleDate < nextMonth && s.IsCompleted);

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting month's transactions");
                throw;
            }
        }
    }
}