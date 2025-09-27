using System.ComponentModel.DataAnnotations;
using InventoryAPI.Models.Enums;

namespace InventoryAPI.Models.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SKU { get; set; }
        public string Barcode { get; set; }
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public int StockQuantity { get; set; }
        public int MinStockLevel { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsLowStock { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateProductDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        [StringLength(50)]
        public string SKU { get; set; }
        
        [Required]
        public string Barcode { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cost price must be greater than 0")]
        public decimal CostPrice { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int StockQuantity { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Minimum stock level cannot be negative")]
        public int MinStockLevel { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        
        [Required]
        public int SupplierId { get; set; }
        
        public string ImageUrl { get; set; }
    }

    public class UpdateProductDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cost price must be greater than 0")]
        public decimal CostPrice { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Minimum stock level cannot be negative")]
        public int MinStockLevel { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        
        [Required]
        public int SupplierId { get; set; }
        
        public string ImageUrl { get; set; }
        
        public bool IsActive { get; set; } = true;
    }

    public class UpdateStockDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
        
        [Required]
        public StockMovementType MovementType { get; set; }
        
        public string Reference { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductCount { get; set; }
    }

    public class CreateCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        public string Description { get; set; }
    }

    public class UpdateCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public bool IsActive { get; set; } = true;
    }

    public class SupplierDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductCount { get; set; }
    }

    public class CreateSupplierDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(100)]
        public string ContactPerson { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        
        [Phone]
        public string Phone { get; set; }
        
        public string Address { get; set; }
    }

    public class UpdateSupplierDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(100)]
        public string ContactPerson { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        
        [Phone]
        public string Phone { get; set; }
        
        public string Address { get; set; }
        
        public bool IsActive { get; set; } = true;
    }

    public class StockMovementDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public StockMovementType MovementType { get; set; }
        public int Quantity { get; set; }
        public string Reference { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
    }
}