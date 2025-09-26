namespace InventoryAPI.Models.Entities
{
    public class SaleItem
    {
        public int Id { get; set; }
        
        [Required]
        public int SaleId { get; set; }
        
        public Sale Sale { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        public Product Product { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        public decimal UnitPrice { get; set; }
        
        public decimal DiscountPercentage { get; set; } = 0;
        
        [Required]
        public decimal TotalPrice { get; set; }
    }
}