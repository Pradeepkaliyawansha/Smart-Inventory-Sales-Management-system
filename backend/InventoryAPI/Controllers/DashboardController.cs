using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryAPI.Models.DTOs;
using InventoryAPI.Services.Interfaces;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IReportService _reportService;
        private readonly IProductService _productService;
        private readonly ISaleService _saleService;
        private readonly ICustomerService _customerService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;
        private readonly ILogger<DashboardController> _logger;
        
        public DashboardController(
            IDashboardService dashboardService,
            IReportService reportService,
            IProductService productService,
            ISaleService saleService,
            ICustomerService customerService,
            ICategoryService categoryService,
            ISupplierService supplierService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _reportService = reportService;
            _productService = productService;
            _saleService = saleService;
            _customerService = customerService;
            _categoryService = categoryService;
            _supplierService = supplierService;
            _logger = logger;
        }
        
        /// <summary>
        /// Get main dashboard statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
        {
            try
            {
                var stats = await _dashboardService.GetDashboardStatsAsync();
                _logger.LogInformation("Retrieved dashboard statistics");
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                return StatusCode(500, new { message = "Error retrieving dashboard statistics" });
            }
        }
        
        /// <summary>
        /// Get weekly sales data for charts
        /// </summary>
        [HttpGet("weekly-sales")]
        public async Task<ActionResult<IEnumerable<DailySalesDto>>> GetWeeklySales()
        {
            try
            {
                var sales = await _dashboardService.GetWeeklySalesAsync();
                return Ok(sales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving weekly sales");
                return StatusCode(500, new { message = "Error retrieving weekly sales data" });
            }
        }
        
        /// <summary>
        /// Get monthly sales data for charts
        /// </summary>
        [HttpGet("monthly-sales")]
        public async Task<ActionResult<IEnumerable<DailySalesDto>>> GetMonthlySales()
        {
            try
            {
                var sales = await _dashboardService.GetMonthlySalesAsync();
                return Ok(sales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving monthly sales");
                return StatusCode(500, new { message = "Error retrieving monthly sales data" });
            }
        }
        
        /// <summary>
        /// Get top selling products
        /// </summary>
        [HttpGet("top-products")]
        public async Task<ActionResult<IEnumerable<TopSellingProductDto>>> GetTopProducts([FromQuery] int count = 5)
        {
            try
            {
                if (count <= 0 || count > 50)
                    return BadRequest(new { message = "Count must be between 1 and 50" });

                var products = await _dashboardService.GetTodayTopProductsAsync(count);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top products");
                return StatusCode(500, new { message = "Error retrieving top products" });
            }
        }
        
        /// <summary>
        /// Get critical stock alerts
        /// </summary>
        [HttpGet("stock-alerts")]
        public async Task<ActionResult<IEnumerable<StockAlertDto>>> GetStockAlerts()
        {
            try
            {
                var alerts = await _dashboardService.GetCriticalStockAlertsAsync();
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock alerts");
                return StatusCode(500, new { message = "Error retrieving stock alerts" });
            }
        }
        
        /// <summary>
        /// Get today's revenue
        /// </summary>
        [HttpGet("today-revenue")]
        public async Task<ActionResult> GetTodayRevenue()
        {
            try
            {
                var revenue = await _dashboardService.GetTodayRevenueAsync();
                var response = new
                {
                    revenue,
                    date = DateTime.UtcNow.Date,
                    formatted = revenue.ToString("C"),
                    currency = "USD"
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving today's revenue");
                return StatusCode(500, new { message = "Error retrieving today's revenue" });
            }
        }
        
        /// <summary>
        /// Get this month's revenue
        /// </summary>
        [HttpGet("month-revenue")]
        public async Task<ActionResult> GetMonthRevenue()
        {
            try
            {
                var revenue = await _dashboardService.GetMonthRevenueAsync();
                var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var response = new
                {
                    revenue,
                    month = firstDayOfMonth.ToString("MMMM yyyy"),
                    period = new { start = firstDayOfMonth, end = DateTime.UtcNow },
                    formatted = revenue.ToString("C"),
                    currency = "USD"
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving month's revenue");
                return StatusCode(500, new { message = "Error retrieving month's revenue" });
            }
        }

        /// <summary>
        /// Get today's transaction count
        /// </summary>
        [HttpGet("today-transactions")]
        public async Task<ActionResult> GetTodayTransactions()
        {
            try
            {
                var count = await _dashboardService.GetTodayTransactionsAsync();
                var response = new
                {
                    transactionCount = count,
                    date = DateTime.UtcNow.Date,
                    dayOfWeek = DateTime.UtcNow.DayOfWeek.ToString()
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving today's transactions");
                return StatusCode(500, new { message = "Error retrieving today's transactions" });
            }
        }

        /// <summary>
        /// Get this month's transaction count
        /// </summary>
        [HttpGet("month-transactions")]
        public async Task<ActionResult> GetMonthTransactions()
        {
            try
            {
                var count = await _dashboardService.GetMonthTransactionsAsync();
                var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var response = new
                {
                    transactionCount = count,
                    month = firstDayOfMonth.ToString("MMMM yyyy"),
                    period = new { start = firstDayOfMonth, end = DateTime.UtcNow },
                    averagePerDay = DateTime.UtcNow.Day > 0 ? (decimal)count / DateTime.UtcNow.Day : 0
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving month's transactions");
                return StatusCode(500, new { message = "Error retrieving month's transactions" });
            }
        }

        /// <summary>
        /// Get low stock products for alerts
        /// </summary>
        [HttpGet("low-stock-products")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStockProducts()
        {
            try
            {
                var products = await _productService.GetLowStockProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving low stock products");
                return StatusCode(500, new { message = "Error retrieving low stock products" });
            }
        }

        /// <summary>
        /// Get out of stock products
        /// </summary>
        [HttpGet("out-of-stock-products")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetOutOfStockProducts()
        {
            try
            {
                var products = await _productService.GetOutOfStockProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving out of stock products");
                return StatusCode(500, new { message = "Error retrieving out of stock products" });
            }
        }

        /// <summary>
        /// Get recent sales for activity feed
        /// </summary>
        [HttpGet("recent-sales")]
        public async Task<ActionResult<IEnumerable<SaleDto>>> GetRecentSales([FromQuery] int count = 10)
        {
            try
            {
                if (count <= 0 || count > 50)
                    return BadRequest(new { message = "Count must be between 1 and 50" });

                var sales = await _saleService.GetTodaysSalesAsync();
                var recentSales = sales.Take(count);
                return Ok(recentSales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent sales");
                return StatusCode(500, new { message = "Error retrieving recent sales" });
            }
        }

        /// <summary>
        /// Get top customers by spending
        /// </summary>
        [HttpGet("top-customers")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetTopCustomers([FromQuery] int count = 5)
        {
            try
            {
                if (count <= 0 || count > 20)
                    return BadRequest(new { message = "Count must be between 1 and 20" });

                var customers = await _customerService.GetTopCustomersAsync(count);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top customers");
                return StatusCode(500, new { message = "Error retrieving top customers" });
            }
        }

        /// <summary>
        /// Get comprehensive quick statistics
        /// </summary>
        [HttpGet("quick-stats")]
        public async Task<ActionResult> GetQuickStats()
        {
            try
            {
                var todayRevenue = await _dashboardService.GetTodayRevenueAsync();
                var monthRevenue = await _dashboardService.GetMonthRevenueAsync();
                var todayTransactions = await _dashboardService.GetTodayTransactionsAsync();
                var monthTransactions = await _dashboardService.GetMonthTransactionsAsync();
                
                // Get product counts
                var allProducts = await _productService.GetAllProductsAsync();
                var lowStockProducts = await _productService.GetLowStockProductsAsync();
                var outOfStockProducts = await _productService.GetOutOfStockProductsAsync();
                
                // Get customer stats
                var allCustomers = await _customerService.GetAllCustomersAsync();
                
                // Get category and supplier counts
                var categories = await _categoryService.GetAllCategoriesAsync();
                var suppliers = await _supplierService.GetAllSuppliersAsync();

                var quickStats = new
                {
                    revenue = new
                    {
                        today = todayRevenue,
                        month = monthRevenue,
                        todayFormatted = todayRevenue.ToString("C"),
                        monthFormatted = monthRevenue.ToString("C")
                    },
                    transactions = new
                    {
                        today = todayTransactions,
                        month = monthTransactions,
                        averagePerDay = DateTime.UtcNow.Day > 0 ? (decimal)monthTransactions / DateTime.UtcNow.Day : 0
                    },
                    inventory = new
                    {
                        totalProducts = allProducts.Count(),
                        lowStockCount = lowStockProducts.Count(),
                        outOfStockCount = outOfStockProducts.Count(),
                        totalInventoryValue = allProducts.Sum(p => p.StockQuantity * p.CostPrice)
                    },
                    customers = new
                    {
                        totalCustomers = allCustomers.Count(),
                        activeCustomers = allCustomers.Count(c => c.LastPurchaseDate.HasValue && 
                                                                 c.LastPurchaseDate.Value >= DateTime.UtcNow.AddDays(-30))
                    },
                    catalog = new
                    {
                        totalCategories = categories.Count(),
                        activeCategories = categories.Count(c => c.IsActive),
                        totalSuppliers = suppliers.Count(),
                        activeSuppliers = suppliers.Count(s => s.IsActive)
                    },
                    alerts = new
                    {
                        stockAlerts = lowStockProducts.Count() + outOfStockProducts.Count(),
                        criticalAlerts = outOfStockProducts.Count()
                    },
                    timestamp = DateTime.UtcNow
                };

                return Ok(quickStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quick statistics");
                return StatusCode(500, new { message = "Error retrieving quick statistics" });
            }
        }

        /// <summary>
        /// Get sales performance comparison (today vs yesterday, month vs last month)
        /// </summary>
        [HttpGet("sales-comparison")]
        public async Task<ActionResult> GetSalesComparison()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var yesterday = today.AddDays(-1);
                var thisMonth = new DateTime(today.Year, today.Month, 1);
                var lastMonth = thisMonth.AddMonths(-1);
                var lastMonthEnd = thisMonth.AddDays(-1);

                // Get today vs yesterday
                var todayRevenue = await _dashboardService.GetTodayRevenueAsync();
                var todayTransactions = await _dashboardService.GetTodayTransactionsAsync();
                
                var yesterdayRevenue = await _saleService.GetTotalSalesAmountAsync(yesterday, today);
                var yesterdayTransactions = (await _saleService.GetSalesByDateRangeAsync(yesterday, today)).Count();

                // Get this month vs last month
                var thisMonthRevenue = await _dashboardService.GetMonthRevenueAsync();
                var thisMonthTransactions = await _dashboardService.GetMonthTransactionsAsync();
                
                var lastMonthRevenue = await _saleService.GetTotalSalesAmountAsync(lastMonth, lastMonthEnd.AddDays(1));
                var lastMonthTransactions = (await _saleService.GetSalesByDateRangeAsync(lastMonth, lastMonthEnd.AddDays(1))).Count();

                var comparison = new
                {
                    daily = new
                    {
                        today = new { revenue = todayRevenue, transactions = todayTransactions },
                        yesterday = new { revenue = yesterdayRevenue, transactions = yesterdayTransactions },
                        revenueChange = yesterdayRevenue > 0 ? ((todayRevenue - yesterdayRevenue) / yesterdayRevenue) * 100 : 0,
                        transactionChange = yesterdayTransactions > 0 ? ((decimal)(todayTransactions - yesterdayTransactions) / yesterdayTransactions) * 100 : 0
                    },
                    monthly = new
                    {
                        thisMonth = new { revenue = thisMonthRevenue, transactions = thisMonthTransactions },
                        lastMonth = new { revenue = lastMonthRevenue, transactions = lastMonthTransactions },
                        revenueChange = lastMonthRevenue > 0 ? ((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 : 0,
                        transactionChange = lastMonthTransactions > 0 ? ((decimal)(thisMonthTransactions - lastMonthTransactions) / lastMonthTransactions) * 100 : 0
                    },
                    generatedAt = DateTime.UtcNow
                };

                return Ok(comparison);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sales comparison");
                return StatusCode(500, new { message = "Error retrieving sales comparison" });
            }
        }

        /// <summary>
        /// Get inventory health overview
        /// </summary>
        [HttpGet("inventory-health")]
        public async Task<ActionResult> GetInventoryHealth()
        {
            try
            {
                var allProducts = await _productService.GetAllProductsAsync();
                var productsList = allProducts.ToList();
                
                var lowStockProducts = productsList.Where(p => p.IsLowStock && p.StockQuantity > 0).ToList();
                var outOfStockProducts = productsList.Where(p => p.StockQuantity == 0).ToList();
                var healthyProducts = productsList.Where(p => !p.IsLowStock && p.StockQuantity > 0).ToList();

                var totalInventoryValue = productsList.Sum(p => p.StockQuantity * p.CostPrice);
                var lowStockValue = lowStockProducts.Sum(p => p.StockQuantity * p.CostPrice);
                var healthyStockValue = healthyProducts.Sum(p => p.StockQuantity * p.CostPrice);

                var inventoryHealth = new
                {
                    overview = new
                    {
                        totalProducts = productsList.Count,
                        totalInventoryValue = totalInventoryValue,
                        averageProductValue = productsList.Any() ? totalInventoryValue / productsList.Count : 0
                    },
                    stockLevels = new
                    {
                        healthy = new
                        {
                            count = healthyProducts.Count,
                            percentage = productsList.Any() ? (decimal)healthyProducts.Count / productsList.Count * 100 : 0,
                            value = healthyStockValue
                        },
                        lowStock = new
                        {
                            count = lowStockProducts.Count,
                            percentage = productsList.Any() ? (decimal)lowStockProducts.Count / productsList.Count * 100 : 0,
                            value = lowStockValue
                        },
                        outOfStock = new
                        {
                            count = outOfStockProducts.Count,
                            percentage = productsList.Any() ? (decimal)outOfStockProducts.Count / productsList.Count * 100 : 0,
                            value = 0m
                        }
                    },
                    alerts = new
                    {
                        critical = outOfStockProducts.Take(5).Select(p => new
                        {
                            productId = p.Id,
                            name = p.Name,
                            sku = p.SKU,
                            category = p.CategoryName
                        }),
                        warning = lowStockProducts.Take(5).Select(p => new
                        {
                            productId = p.Id,
                            name = p.Name,
                            sku = p.SKU,
                            currentStock = p.StockQuantity,
                            minStockLevel = p.MinStockLevel,
                            category = p.CategoryName
                        })
                    },
                    recommendations = GenerateInventoryRecommendations(productsList),
                    lastUpdated = DateTime.UtcNow
                };

                return Ok(inventoryHealth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory health");
                return StatusCode(500, new { message = "Error retrieving inventory health" });
            }
        }

        #region Private Helper Methods

        private static object GenerateInventoryRecommendations(List<ProductDto> products)
        {
            var outOfStock = products.Count(p => p.StockQuantity == 0);
            var lowStock = products.Count(p => p.IsLowStock && p.StockQuantity > 0);
            var recommendations = new List<string>();

            if (outOfStock > 0)
                recommendations.Add($"Immediate reorder needed for {outOfStock} out-of-stock products");
            
            if (lowStock > 0)
                recommendations.Add($"Consider restocking {lowStock} low-stock products");
            
            if (outOfStock == 0 && lowStock == 0)
                recommendations.Add("Inventory levels are healthy");
            
            var totalValue = products.Sum(p => p.StockQuantity * p.CostPrice);
            if (totalValue > 0)
            {
                var averageValue = totalValue / products.Count;
                if (averageValue > 1000)
                    recommendations.Add("Consider reviewing high-value inventory turnover");
            }

            return new { items = recommendations, count = recommendations.Count };
        }

        #endregion
    }
}