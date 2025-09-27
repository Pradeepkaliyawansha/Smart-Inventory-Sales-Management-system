using Microsoft.EntityFrameworkCore;
using AutoMapper;
using InventoryAPI.Data;
using InventoryAPI.Models.Entities;
using InventoryAPI.Models.DTOs;
using InventoryAPI.Models.Enums;
using InventoryAPI.Services.Interfaces;

namespace InventoryAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly InventoryContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(InventoryContext context, IMapper mapper, ILogger<ProductService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .Where(p => p.IsActive)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<ProductDto>>(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                throw;
            }
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

                if (product == null)
                    throw new NotFoundException("Product not found");

                return _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by ID: {ProductId}", id);
                throw;
            }
        }

        public async Task<ProductDto> GetProductBySKUAsync(string sku)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .FirstOrDefaultAsync(p => p.SKU == sku && p.IsActive);

                if (product == null)
                    throw new NotFoundException("Product not found");

                return _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by SKU: {SKU}", sku);
                throw;
            }
        }

        public async Task<ProductDto> GetProductByBarcodeAsync(string barcode)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .FirstOrDefaultAsync(p => p.Barcode == barcode && p.IsActive);

                if (product == null)
                    throw new NotFoundException("Product not found");

                return _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by barcode: {Barcode}", barcode);
                throw;
            }
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            try
            {
                if (await _context.Products.AnyAsync(p => p.SKU == createProductDto.SKU))
                    throw new ArgumentException("SKU already exists");

                if (await _context.Products.AnyAsync(p => p.Barcode == createProductDto.Barcode))
                    throw new ArgumentException("Barcode already exists");

                var product = _mapper.Map<Product>(createProductDto);
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return await GetProductByIdAsync(product.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product: {ProductName}", createProductDto.Name);
                throw;
            }
        }

        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    throw new NotFoundException("Product not found");

                _mapper.Map(updateProductDto, product);
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return await GetProductByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return false;

                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {ProductId}", id);
                return false;
            }
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .Where(p => p.IsActive && p.StockQuantity <= p.MinStockLevel)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<ProductDto>>(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock products");
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> GetOutOfStockProductsAsync()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .Where(p => p.IsActive && p.StockQuantity == 0)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<ProductDto>>(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting out of stock products");
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .Where(p => p.CategoryId == categoryId && p.IsActive)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<ProductDto>>(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category: {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> GetProductsBySupplierAsync(int supplierId)
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .Where(p => p.SupplierId == supplierId && p.IsActive)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<ProductDto>>(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by supplier: {SupplierId}", supplierId);
                throw;
            }
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity, StockMovementType movementType, string reference, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                    return false;

                int previousStock = product.StockQuantity;
                int newStock = movementType switch
                {
                    StockMovementType.Purchase => previousStock + quantity,
                    StockMovementType.Sale => previousStock - quantity,
                    StockMovementType.Return => previousStock + quantity,
                    StockMovementType.Adjustment => quantity,
                    StockMovementType.Transfer => previousStock - quantity,
                    StockMovementType.Damage => previousStock - quantity,
                    _ => previousStock
                };

                if (newStock < 0)
                    throw new InvalidOperationException("Insufficient stock available");

                product.StockQuantity = newStock;
                product.UpdatedAt = DateTime.UtcNow;

                var stockMovement = new StockMovement
                {
                    ProductId = productId,
                    MovementType = movementType,
                    Quantity = quantity,
                    Reference = reference,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                _context.StockMovements.Add(stockMovement);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating stock for product: {ProductId}", productId);
                throw;
            }
        }

        public async Task<IEnumerable<StockMovementDto>> GetStockMovementsAsync(int productId)
        {
            try
            {
                var movements = await _context.StockMovements
                    .Include(sm => sm.Product)
                    .Include(sm => sm.CreatedByUser)
                    .Where(sm => sm.ProductId == productId)
                    .OrderByDescending(sm => sm.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<StockMovementDto>>(movements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock movements for product: {ProductId}", productId);
                throw;
            }
        }

        public async Task<bool> CheckStockAvailabilityAsync(int productId, int quantity)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                return product != null && product.StockQuantity >= quantity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock availability for product: {ProductId}", productId);
                return false;
            }
        }
    }

    public class CategoryService : ICategoryService
    {
        private readonly InventoryContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(InventoryContext context, IMapper mapper, ILogger<CategoryService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        ProductCount = c.Products.Count(p => p.IsActive)
                    })
                    .ToListAsync();

                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                throw;
            }
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

                if (category == null)
                    throw new NotFoundException("Category not found");

                var categoryDto = _mapper.Map<CategoryDto>(category);
                categoryDto.ProductCount = category.Products.Count(p => p.IsActive);

                return categoryDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by ID: {CategoryId}", id);
                throw;
            }
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            try
            {
                if (await _context.Categories.AnyAsync(c => c.Name == createCategoryDto.Name && c.IsActive))
                    throw new ArgumentException("Category name already exists");

                var category = _mapper.Map<Category>(createCategoryDto);
                category.CreatedAt = DateTime.UtcNow;

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return await GetCategoryByIdAsync(category.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {CategoryName}", createCategoryDto.Name);
                throw;
            }
        }

        public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    throw new NotFoundException("Category not found");

                if (await _context.Categories.AnyAsync(c => c.Name == updateCategoryDto.Name && c.Id != id && c.IsActive))
                    throw new ArgumentException("Category name already exists");

                _mapper.Map(updateCategoryDto, category);
                await _context.SaveChangesAsync();

                return await GetCategoryByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                    return false;

                if (category.Products.Any(p => p.IsActive))
                    throw new InvalidOperationException("Cannot delete category that contains products");

                category.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                throw;
            }
        }
    }

    public class SupplierService : ISupplierService
    {
        private readonly InventoryContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(InventoryContext context, IMapper mapper, ILogger<SupplierService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync()
        {
            try
            {
                var suppliers = await _context.Suppliers
                    .Where(s => s.IsActive)
                    .Select(s => new SupplierDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        ContactPerson = s.ContactPerson,
                        Email = s.Email,
                        Phone = s.Phone,
                        Address = s.Address,
                        IsActive = s.IsActive,
                        CreatedAt = s.CreatedAt,
                        ProductCount = s.Products.Count(p => p.IsActive)
                    })
                    .ToListAsync();

                return suppliers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all suppliers");
                throw;
            }
        }

        public async Task<SupplierDto> GetSupplierByIdAsync(int id)
        {
            try
            {
                var supplier = await _context.Suppliers
                    .Include(s => s.Products)
                    .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

                if (supplier == null)
                    throw new NotFoundException("Supplier not found");

                var supplierDto = _mapper.Map<SupplierDto>(supplier);
                supplierDto.ProductCount = supplier.Products.Count(p => p.IsActive);

                return supplierDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplier by ID: {SupplierId}", id);
                throw;
            }
        }

        public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createSupplierDto)
        {
            try
            {
                if (await _context.Suppliers.AnyAsync(s => s.Name == createSupplierDto.Name && s.IsActive))
                    throw new ArgumentException("Supplier name already exists");

                if (!string.IsNullOrEmpty(createSupplierDto.Email) && 
                    await _context.Suppliers.AnyAsync(s => s.Email == createSupplierDto.Email && s.IsActive))
                    throw new ArgumentException("Supplier email already exists");

                var supplier = _mapper.Map<Supplier>(createSupplierDto);
                supplier.CreatedAt = DateTime.UtcNow;

                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();

                return await GetSupplierByIdAsync(supplier.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier: {SupplierName}", createSupplierDto.Name);
                throw;
            }
        }

        public async Task<SupplierDto> UpdateSupplierAsync(int id, UpdateSupplierDto updateSupplierDto)
        {
            try
            {
                var supplier = await _context.Suppliers.FindAsync(id);
                if (supplier == null)
                    throw new NotFoundException("Supplier not found");

                if (await _context.Suppliers.AnyAsync(s => s.Name == updateSupplierDto.Name && s.Id != id && s.IsActive))
                    throw new ArgumentException("Supplier name already exists");

                if (!string.IsNullOrEmpty(updateSupplierDto.Email) && 
                    await _context.Suppliers.AnyAsync(s => s.Email == updateSupplierDto.Email && s.Id != id && s.IsActive))
                    throw new ArgumentException("Supplier email already exists");

                _mapper.Map(updateSupplierDto, supplier);
                await _context.SaveChangesAsync();

                return await GetSupplierByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier: {SupplierId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteSupplierAsync(int id)
        {
            try
            {
                var supplier = await _context.Suppliers
                    .Include(s => s.Products)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (supplier == null)
                    return false;

                if (supplier.Products.Any(p => p.IsActive))
                    throw new InvalidOperationException("Cannot delete supplier that has products");

                supplier.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier: {SupplierId}", id);
                throw;
            }
        }
    }
}