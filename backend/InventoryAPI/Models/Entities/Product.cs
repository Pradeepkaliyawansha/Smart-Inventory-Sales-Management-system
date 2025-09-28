using System.ComponentModel.DataAnnotations;

namespace InventoryAPI.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        public string? Description { get; set; }
        
        [Required]
        [StringLength(50)]
        public string SKU { get; set; }
        
        [Required]
        public string Barcode { get; set; }
        
        [Required]
        public decimal Price { get; set; }
        
        [Required]
        public decimal CostPrice { get; set; }
        
        [Required]
        public int StockQuantity { get; set; }
        
        [Required]
        public int MinStockLevel { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        
        public Category Category { get; set; }
        
        [Required]
        public int SupplierId { get; set; }
        
        public Supplier Supplier { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
        
        public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    }
}