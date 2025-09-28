using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using InventoryAPI.Models.DTOs;
using InventoryAPI.Services.Interfaces;
using InventoryAPI.Exceptions;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            ICategoryService categoryService,
            IProductService productService,
            ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                _logger.LogInformation("Retrieved {Count} categories", categories.Count());
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return Ok(category);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with ID {CategoryId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Create new category (Admin/Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createCategoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var category = await _categoryService.CreateCategoryAsync(createCategoryDto);
                _logger.LogInformation("Created new category: {CategoryName} with ID {CategoryId}",
                    category.Name, category.Id);

                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Failed to create category: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {CategoryName}", createCategoryDto.Name);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Update category (Admin/Manager only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var category = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);
                _logger.LogInformation("Updated category with ID {CategoryId}", id);

                return Ok(category);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found for update", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Failed to update category {CategoryId}: {Error}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category with ID {CategoryId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Delete category (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found for deletion", id);
                    return NotFound(new { message = "Category not found" });
                }

                _logger.LogInformation("Deleted category with ID {CategoryId}", id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete category {CategoryId}: {Error}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID {CategoryId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Get products for a specific category
        /// </summary>
        [HttpGet("{id}/products")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetCategoryProducts(int id)
        {
            try
            {
                // First verify category exists
                await _categoryService.GetCategoryByIdAsync(id);

                var products = await _productService.GetProductsByCategoryAsync(id);
                _logger.LogInformation("Retrieved {Count} products for category {CategoryId}",
                    products.Count(), id);

                return Ok(products);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for category {CategoryId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Get only active categories
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetActiveCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                var activeCategories = categories.Where(c => c.IsActive);

                _logger.LogInformation("Retrieved {Count} active categories", activeCategories.Count());
                return Ok(activeCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active categories");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Search categories by name or description
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> SearchCategories([FromQuery] string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                    return BadRequest(new { message = "Search term is required and cannot be empty" });

                if (term.Length < 2)
                    return BadRequest(new { message = "Search term must be at least 2 characters long" });

                var categories = await _categoryService.GetAllCategoriesAsync();
                var filtered = categories.Where(c =>
                    c.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(c.Description) && c.Description.Contains(term, StringComparison.OrdinalIgnoreCase)));

                _logger.LogInformation("Search for '{SearchTerm}' returned {Count} categories", term, filtered.Count());
                return Ok(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching categories with term: {SearchTerm}", term);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Get category statistics and summary
        /// </summary>
        [HttpGet("{id}/statistics")]
        public async Task<ActionResult> GetCategoryStatistics(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                var products = await _productService.GetProductsByCategoryAsync(id);
                var productsList = products.ToList();

                var statistics = new
                {
                    categoryId = id,
                    categoryName = category.Name,
                    description = category.Description,
                    totalProducts = productsList.Count,
                    activeProducts = productsList.Count(p => p.IsActive),
                    inactiveProducts = productsList.Count(p => !p.IsActive),
                    totalInventoryValue = productsList.Sum(p => p.StockQuantity * p.CostPrice),
                    totalStockQuantity = productsList.Sum(p => p.StockQuantity),
                    averageProductPrice = productsList.Any() ? productsList.Average(p => p.Price) : 0,
                    averageCostPrice = productsList.Any() ? productsList.Average(p => p.CostPrice) : 0,
                    lowStockProducts = productsList.Count(p => p.IsLowStock),
                    outOfStockProducts = productsList.Count(p => p.StockQuantity == 0),
                    priceRange = productsList.Any() ? new
                    {
                        min = productsList.Min(p => p.Price),
                        max = productsList.Max(p => p.Price)
                    } : null,
                    topSuppliers = productsList.GroupBy(p => new { p.SupplierId, p.SupplierName })
                        .Select(g => new
                        {
                            supplierId = g.Key.SupplierId,
                            supplierName = g.Key.SupplierName,
                            productCount = g.Count()
                        })
                        .OrderByDescending(s => s.productCount)
                        .Take(5),
                    lastUpdated = DateTime.UtcNow
                };

                return Ok(statistics);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics for category {CategoryId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Get categories with low stock products
        /// </summary>
        [HttpGet("with-low-stock")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> GetCategoriesWithLowStock()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                var categoriesWithLowStock = new List<object>();

                foreach (var category in categories.Where(c => c.IsActive))
                {
                    var products = await _productService.GetProductsByCategoryAsync(category.Id);
                    var lowStockProducts = products.Where(p => p.IsLowStock).ToList();

                    if (lowStockProducts.Any())
                    {
                        categoriesWithLowStock.Add(new
                        {
                            categoryId = category.Id,
                            categoryName = category.Name,
                            description = category.Description,
                            totalProducts = products.Count(),
                            lowStockProductCount = lowStockProducts.Count,
                            outOfStockProductCount = lowStockProducts.Count(p => p.StockQuantity == 0),
                            lowStockValue = lowStockProducts.Sum(p => p.StockQuantity * p.CostPrice),
                            lowStockProducts = lowStockProducts.Select(p => new
                            {
                                productId = p.Id,
                                productName = p.Name,
                                sku = p.SKU,
                                currentStock = p.StockQuantity,
                                minStockLevel = p.MinStockLevel,
                                supplierName = p.SupplierName
                            }).Take(10) // Limit to first 10 for performance
                        });
                    }
                }

                _logger.LogInformation("Found {Count} categories with low stock products", categoriesWithLowStock.Count);
                return Ok(categoriesWithLowStock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories with low stock");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Get category performance metrics
        /// </summary>
        [HttpGet("{id}/performance")]
        public async Task<ActionResult> GetCategoryPerformance(int id, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                var products = await _productService.GetProductsByCategoryAsync(id);

                // Set default date range if not provided
                fromDate ??= DateTime.UtcNow.AddDays(-30);
                toDate ??= DateTime.UtcNow;

                // This would require integration with sales service for actual sales data
                // For now, we'll provide inventory-based performance metrics
                var productsList = products.ToList();

                var performance = new
                {
                    categoryId = id,
                    categoryName = category.Name,
                    period = new { from = fromDate, to = toDate },
                    inventoryMetrics = new
                    {
                        totalProducts = productsList.Count,
                        totalInventoryValue = productsList.Sum(p => p.StockQuantity * p.CostPrice),
                        totalStockQuantity = productsList.Sum(p => p.StockQuantity),
                        averageStockTurnover = CalculateStockTurnover(productsList), // Simplified calculation
                        stockUtilization = CalculateStockUtilization(productsList)
                    },
                    topPerformingProducts = productsList
                        .Where(p => p.StockQuantity > 0)
                        .OrderByDescending(p => p.StockQuantity * p.Price)
                        .Take(5)
                        .Select(p => new
                        {
                            productId = p.Id,
                            productName = p.Name,
                            sku = p.SKU,
                            inventoryValue = p.StockQuantity * p.CostPrice,
                            stockLevel = p.StockQuantity,
                            isLowStock = p.IsLowStock
                        }),
                    stockAlerts = productsList
                        .Where(p => p.IsLowStock || p.StockQuantity == 0)
                        .Select(p => new
                        {
                            productId = p.Id,
                            productName = p.Name,
                            currentStock = p.StockQuantity,
                            minStockLevel = p.MinStockLevel,
                            alertLevel = p.StockQuantity == 0 ? "Out of Stock" : "Low Stock"
                        })
                };

                return Ok(performance);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving performance for category {CategoryId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Get categories summary for dashboard
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult> GetCategoriesSummary()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                var categoriesList = categories.ToList();

                var summary = new
                {
                    totalCategories = categoriesList.Count,
                    activeCategories = categoriesList.Count(c => c.IsActive),
                    inactiveCategories = categoriesList.Count(c => !c.IsActive),
                    categoriesWithProducts = 0, // Will be calculated below
                    emptyCategories = 0,
                    topCategories = new List<object>(),
                    lastUpdated = DateTime.UtcNow
                };

                var topCategories = new List<object>();
                int categoriesWithProducts = 0;
                int emptyCategories = 0;

                foreach (var category in categoriesList.Where(c => c.IsActive))
                {
                    var products = await _productService.GetProductsByCategoryAsync(category.Id);
                    var productsList = products.ToList();

                    if (productsList.Any())
                    {
                        categoriesWithProducts++;
                        topCategories.Add(new
                        {
                            categoryId = category.Id,
                            categoryName = category.Name,
                            productCount = productsList.Count,
                            inventoryValue = productsList.Sum(p => p.StockQuantity * p.CostPrice)
                        });
                    }
                    else
                    {
                        emptyCategories++;
                    }
                }

                var finalSummary = new
                {
                    totalCategories = categoriesList.Count,
                    activeCategories = categoriesList.Count(c => c.IsActive),
                    inactiveCategories = categoriesList.Count(c => !c.IsActive),
                    categoriesWithProducts,
                    emptyCategories,
                    topCategories = topCategories
                        .OrderByDescending(c => ((dynamic)c).inventoryValue)
                        .Take(5),
                    lastUpdated = DateTime.UtcNow
                };

                return Ok(finalSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories summary");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Bulk update category status (Admin only)
        /// </summary>
        [HttpPost("bulk-status-update")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> BulkUpdateCategoryStatus([FromBody] BulkCategoryStatusUpdateDto bulkUpdate)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var results = new List<object>();

                foreach (var categoryId in bulkUpdate.CategoryIds)
                {
                    try
                    {
                        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                        var updateDto = new UpdateCategoryDto
                        {
                            Name = category.Name,
                            Description = category.Description,
                            IsActive = bulkUpdate.IsActive
                        };

                        var updatedCategory = await _categoryService.UpdateCategoryAsync(categoryId, updateDto);
                        results.Add(new { categoryId, success = true, newStatus = bulkUpdate.IsActive });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to update category {CategoryId}: {Error}", categoryId, ex.Message);
                        results.Add(new { categoryId, success = false, error = ex.Message });
                    }
                }

                _logger.LogInformation("Bulk updated {Count} categories status to {Status}",
                    bulkUpdate.CategoryIds.Count, bulkUpdate.IsActive);

                return Ok(new
                {
                    results,
                    totalProcessed = bulkUpdate.CategoryIds.Count,
                    successful = results.Count(r => ((dynamic)r).success == true),
                    failed = results.Count(r => ((dynamic)r).success == false)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk category status update");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        #region Private Helper Methods

        private static decimal CalculateStockTurnover(List<ProductDto> products)
        {
            // Simplified stock turnover calculation
            // In a real scenario, this would use sales data
            if (!products.Any()) return 0;

            var totalCostOfGoods = products.Sum(p => p.StockQuantity * p.CostPrice);
            var averageInventory = totalCostOfGoods / products.Count;

            return averageInventory > 0 ? totalCostOfGoods / averageInventory : 0;
        }

        private static decimal CalculateStockUtilization(List<ProductDto> products)
        {
            // Stock utilization as percentage of products above minimum stock level
            if (!products.Any()) return 0;

            var productsAboveMinStock = products.Count(p => p.StockQuantity > p.MinStockLevel);
            return (decimal)productsAboveMinStock / products.Count * 100;
        }

        #endregion
    }
}

// // Additional DTO for bulk operations
// namespace InventoryAPI.Models.DTOs
// {
//     public class BulkCategoryStatusUpdateDto
//     {
//         [Required]
//         public List<int> CategoryIds { get; set; } = new List<int>();
        
//         [Required]
//         public bool IsActive { get; set; }
        
//         public string Reason { get; set; } = "";
//     }
// }