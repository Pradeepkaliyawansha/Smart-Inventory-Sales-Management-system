namespace InventoryAPI.Models.Entities
{
    public class Sale
    {
        public int Id { get; set; }
        
        [Required]
        public string InvoiceNumber { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        public Customer Customer { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        public User User { get; set; }
        
        [Required]
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        public decimal SubTotal { get; set; }
        
        public decimal DiscountAmount { get; set; } = 0;
        
        public decimal TaxAmount { get; set; } = 0;
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        public decimal PaidAmount { get; set; } = 0;
        
        public PaymentMethod PaymentMethod { get; set; }
        
        public string? Notes { get; set; }
        
        public bool IsCompleted { get; set; } = true;
        
        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}