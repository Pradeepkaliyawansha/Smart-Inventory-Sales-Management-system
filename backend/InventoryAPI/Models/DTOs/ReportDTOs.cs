using InventoryAPI.Models.Enums;

namespace InventoryAPI.Models.DTOs
{
    public class SalesReportDto
    {
        public DateTime ReportDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalDiscounts { get; set; }
        public decimal TotalTax { get; set; }
        public decimal AverageTransactionValue { get; set; }
        public List<PaymentMethodSummaryDto> PaymentMethodBreakdown { get; set; } = new List<PaymentMethodSummaryDto>();
        public List<DailySalesDto> DailySales { get; set; } = new List<DailySalesDto>();
        public List<TopSellingProductDto> TopSellingProducts { get; set; } = new List<TopSellingProductDto>();
    }

    public class PaymentMethodSummaryDto
    {
        public PaymentMethod PaymentMethod { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class DailySalesDto
    {
        public DateTime Date { get; set; }
        public decimal TotalSales { get; set; }
        public int TransactionCount { get; set; }
    }

    public class TopSellingProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class InventoryReportDto
    {
        public DateTime ReportDate { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int LowStockProductsCount { get; set; }
        public int OutOfStockProductsCount { get; set; }
        public List<ProductStockDto> ProductStockLevels { get; set; } = new List<ProductStockDto>();
        public List<CategoryStockDto> CategoryStockSummary { get; set; } = new List<CategoryStockDto>();
        public List<StockMovementSummaryDto> StockMovementsSummary { get; set; } = new List<StockMovementSummaryDto>();
    }

    public class ProductStockDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public string CategoryName { get; set; }
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public bool IsLowStock { get; set; }
        public bool IsOutOfStock { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal StockValue { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class CategoryStockDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public int TotalStockQuantity { get; set; }
        public decimal TotalStockValue { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
    }

    public class StockMovementSummaryDto
    {
        public StockMovementType MovementType { get; set; }
        public int MovementCount { get; set; }
        public int TotalQuantity { get; set; }
        public DateTime? LastMovementDate { get; set; }
    }

    public class CustomerReportDto
    {
        public DateTime ReportDate { get; set; }
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public decimal TotalLoyaltyPoints { get; set; }
        public decimal TotalCreditBalance { get; set; }
        public List<TopCustomerDto> TopCustomers { get; set; } = new List<TopCustomerDto>();
        public List<CustomerActivityDto> CustomerActivity { get; set; } = new List<CustomerActivityDto>();
    }

    public class TopCustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
        public decimal LoyaltyPoints { get; set; }
    }

    public class CustomerActivityDto
    {
        public DateTime Date { get; set; }
        public int NewCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public decimal TotalCustomerSpending { get; set; }
    }

    public class ProfitabilityReportDto
    {
        public DateTime ReportDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCostOfGoodsSold { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossProfitMargin { get; set; }
        public List<ProductProfitabilityDto> ProductProfitability { get; set; } = new List<ProductProfitabilityDto>();
        public List<CategoryProfitabilityDto> CategoryProfitability { get; set; } = new List<CategoryProfitabilityDto>();
    }

    public class ProductProfitabilityDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public decimal CostOfGoodsSold { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossProfitMargin { get; set; }
        public decimal UnitProfit { get; set; }
    }

    public class CategoryProfitabilityDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ProductsSold { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public decimal CostOfGoodsSold { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossProfitMargin { get; set; }
    }

    public class DashboardStatsDto
    {
        public decimal TodaySales { get; set; }
        public decimal MonthSales { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int TotalCustomers { get; set; }
        public int PendingOrders { get; set; }
        public List<DailySalesDto> WeeklySales { get; set; } = new List<DailySalesDto>();
        public List<TopSellingProductDto> TopProducts { get; set; } = new List<TopSellingProductDto>();
        public List<StockAlertDto> StockAlerts { get; set; } = new List<StockAlertDto>();
    }

    public class StockAlertDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public bool IsOutOfStock { get; set; }
        public string AlertLevel { get; set; } // "Critical", "Low", "Warning"
    }
}