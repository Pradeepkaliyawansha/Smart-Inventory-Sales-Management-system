using System.ComponentModel.DataAnnotations;

namespace InventoryAPI.Models.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [EmailAddress]
        public string? Email { get; set; }
        
        [Phone]
        public string? Phone { get; set; }
        
        public string? Address { get; set; }
        
        public decimal LoyaltyPoints { get; set; } = 0;
        
        public decimal CreditBalance { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastPurchaseDate { get; set; }
        
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}