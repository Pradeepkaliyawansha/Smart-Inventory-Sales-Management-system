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
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;
        private readonly ILogger<SuppliersController> _logger;
        
        public SuppliersController(
            ISupplierService supplierService, 
            IProductService productService,
            ILogger<SuppliersController> logger)
        {
            _supplierService = supplierService;
            _productService = productService;
            _logger = logger;
        }
        
        /// <summary>
        /// Get all suppliers
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliers()
        {
            try
            {
                var suppliers = await _supplierService.GetAllSuppliersAsync();
                _logger.LogInformation("Retrieved {Count} suppliers", suppliers.Count());
                return Ok(suppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving suppliers");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }
        
        /// <summary>
        /// Get supplier by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<SupplierDto>> GetSupplier(int id)
        {
            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(id);
                return Ok(supplier);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Supplier with ID {SupplierId} not found", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier with ID {SupplierId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }
        
        /// <summary>
        /// Create new supplier (Admin/Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<SupplierDto>> CreateSupplier(CreateSupplierDto createSupplierDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var supplier = await _supplierService.CreateSupplierAsync(createSupplierDto);
                _logger.LogInformation("Created new supplier: {SupplierName} with ID {SupplierId}", 
                    supplier.Name, supplier.Id);
                
                return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, supplier);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Failed to create supplier: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier: {SupplierName}", createSupplierDto.Name);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }
        
        /// <summary>
        /// Update supplier (Admin/Manager only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<SupplierDto>> UpdateSupplier(int id, UpdateSupplierDto updateSupplierDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var supplier = await _supplierService.UpdateSupplierAsync(id, updateSupplierDto);
                _logger.LogInformation("Updated supplier with ID {SupplierId}", id);
                
                return Ok(supplier);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Supplier with ID {SupplierId} not found for update", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Failed to update supplier {SupplierId}: {Error}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier with ID {SupplierId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }
        
        /// <summary>
        /// Delete supplier (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            try
            {
                var result = await _supplierService.DeleteSupplierAsync(id);
                if (!result) 
                {
                    _logger.LogWarning("Supplier with ID {SupplierId} not found for deletion", id);
                    return NotFound(new { message = "Supplier not found" });
                }
                
                _logger.LogInformation("Deleted supplier with ID {SupplierId}", id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete supplier {SupplierId}: {Error}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier with ID {SupplierId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Get products for a specific supplier
        /// </summary>
        [HttpGet("{id}/products")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetSupplierProducts(int id)
        {
            try
            {
                // First verify supplier exists
                await _supplierService.GetSupplierByIdAsync(id);
                
                var products = await _productService.GetProductsBySupplierAsync(id);
                _logger.LogInformation("Retrieved {Count} products for supplier {SupplierId}", 
                    products.Count(), id);
                
                return Ok(products);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Supplier with ID {SupplierId} not found", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for supplier {SupplierId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Get only active suppliers
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<SupplierDto>>> GetActiveSuppliers()
        {
            try
            {
                var suppliers = await _supplierService.GetAllSuppliersAsync();
                var activeSuppliers = suppliers.Where(s => s.IsActive);
                
                _logger.LogInformation("Retrieved {Count} active suppliers", activeSuppliers.Count());
                return Ok(activeSuppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active suppliers");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Search suppliers by name, contact person, or email
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<SupplierDto>>> SearchSuppliers([FromQuery] string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                    return BadRequest(new { message = "Search term is required and cannot be empty" });

                if (term.Length < 2)
                    return BadRequest(new { message = "Search term must be at least 2 characters long" });

                var suppliers = await _supplierService.GetAllSuppliersAsync();
                var filtered = suppliers.Where(s => 
                    s.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(s.ContactPerson) && s.ContactPerson.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(s.Email) && s.Email.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(s.Phone) && s.Phone.Contains(term, StringComparison.OrdinalIgnoreCase)));
                
                _logger.LogInformation("Search for '{SearchTerm}' returned {Count} suppliers", term, filtered.Count());
                return Ok(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching suppliers with term: {SearchTerm}", term);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Get supplier statistics and summary
        /// </summary>
        [HttpGet("{id}/statistics")]
        public async Task<ActionResult> GetSupplierStatistics(int id)
        {
            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(id);
                var products = await _productService.GetProductsBySupplierAsync(id);
                var productsList = products.ToList();

                var statistics = new
                {
                    supplierId = id,
                    supplierName = supplier.Name,
                    totalProducts = productsList.Count,
                    activeProducts = productsList.Count(p => p.IsActive),
                    inactiveProducts = productsList.Count(p => !p.IsActive),
                    totalInventoryValue = productsList.Sum(p => p.StockQuantity * p.CostPrice),
                    averageProductPrice = productsList.Any() ? productsList.Average(p => p.Price) : 0,
                    lowStockProducts = productsList.Count(p => p.IsLowStock),
                    outOfStockProducts = productsList.Count(p => p.StockQuantity == 0),
                    categories = productsList.GroupBy(p => p.CategoryName)
                        .Select(g => new { category = g.Key, count = g.Count() })
                        .OrderByDescending(c => c.count),
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
                _logger.LogError(ex, "Error retrieving statistics for supplier {SupplierId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Get suppliers with low stock products
        /// </summary>
        [HttpGet("with-low-stock")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> GetSuppliersWithLowStock()
        {
            try
            {
                var suppliers = await _supplierService.GetAllSuppliersAsync();
                var suppliersWithLowStock = new List<object>();

                foreach (var supplier in suppliers.Where(s => s.IsActive))
                {
                    var products = await _productService.GetProductsBySupplierAsync(supplier.Id);
                    var lowStockProducts = products.Where(p => p.IsLowStock).ToList();

                    if (lowStockProducts.Any())
                    {
                        suppliersWithLowStock.Add(new
                        {
                            supplierId = supplier.Id,
                            supplierName = supplier.Name,
                            contactPerson = supplier.ContactPerson,
                            email = supplier.Email,
                            phone = supplier.Phone,
                            lowStockProductCount = lowStockProducts.Count,
                            outOfStockProductCount = lowStockProducts.Count(p => p.StockQuantity == 0),
                            lowStockProducts = lowStockProducts.Select(p => new
                            {
                                productId = p.Id,
                                productName = p.Name,
                                sku = p.SKU,
                                currentStock = p.StockQuantity,
                                minStockLevel = p.MinStockLevel
                            })
                        });
                    }
                }

                _logger.LogInformation("Found {Count} suppliers with low stock products", suppliersWithLowStock.Count);
                return Ok(suppliersWithLowStock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving suppliers with low stock");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Get supplier contact information only
        /// </summary>
        [HttpGet("{id}/contact")]
        public async Task<ActionResult> GetSupplierContact(int id)
        {
            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(id);
                
                var contact = new
                {
                    supplierId = supplier.Id,
                    name = supplier.Name,
                    contactPerson = supplier.ContactPerson,
                    email = supplier.Email,
                    phone = supplier.Phone,
                    address = supplier.Address,
                    isActive = supplier.IsActive
                };

                return Ok(contact);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contact for supplier {SupplierId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Bulk update supplier status (Admin only)
        /// </summary>
        [HttpPost("bulk-status-update")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> BulkUpdateSupplierStatus([FromBody] BulkSupplierStatusUpdateDto bulkUpdate)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var results = new List<object>();

                foreach (var supplierId in bulkUpdate.SupplierIds)
                {
                    try
                    {
                        var supplier = await _supplierService.GetSupplierByIdAsync(supplierId);
                        var updateDto = new UpdateSupplierDto
                        {
                            Name = supplier.Name,
                            ContactPerson = supplier.ContactPerson,
                            Email = supplier.Email,
                            Phone = supplier.Phone,
                            Address = supplier.Address,
                            IsActive = bulkUpdate.IsActive
                        };

                        var updatedSupplier = await _supplierService.UpdateSupplierAsync(supplierId, updateDto);
                        results.Add(new { supplierId, success = true, newStatus = bulkUpdate.IsActive });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to update supplier {SupplierId}: {Error}", supplierId, ex.Message);
                        results.Add(new { supplierId, success = false, error = ex.Message });
                    }
                }

                _logger.LogInformation("Bulk updated {Count} suppliers status to {Status}", 
                    bulkUpdate.SupplierIds.Count, bulkUpdate.IsActive);

                return Ok(new { 
                    results, 
                    totalProcessed = bulkUpdate.SupplierIds.Count,
                    successful = results.Count(r => ((dynamic)r).success == true),
                    failed = results.Count(r => ((dynamic)r).success == false)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk supplier status update");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }
    }
}

// // Additional DTO for bulk operations
// namespace InventoryAPI.Models.DTOs
// {
//     public class BulkSupplierStatusUpdateDto
//     {
//         [Required]
//         public List<int> SupplierIds { get; set; } = new List<int>();
        
//         [Required]
//         public bool IsActive { get; set; }
        
//         public string Reason { get; set; } = "";
//     }
// }