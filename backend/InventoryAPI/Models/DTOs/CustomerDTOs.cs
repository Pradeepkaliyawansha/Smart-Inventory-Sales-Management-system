using System.ComponentModel.DataAnnotations;

namespace InventoryAPI.Models.DTOs
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public decimal LoyaltyPoints { get; set; }
        public decimal CreditBalance { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
        public int TotalPurchases { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class CreateCustomerDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        
        [Phone]
        public string Phone { get; set; }
        
        public string Address { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Loyalty points cannot be negative")]
        public decimal LoyaltyPoints { get; set; } = 0;
        
        [Range(0, double.MaxValue, ErrorMessage = "Credit balance cannot be negative")]
        public decimal CreditBalance { get; set; } = 0;
    }

    public class UpdateCustomerDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        
        [Phone]
        public string Phone { get; set; }
        
        public string Address { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Loyalty points cannot be negative")]
        public decimal LoyaltyPoints { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Credit balance cannot be negative")]
        public decimal CreditBalance { get; set; }
        
        public bool IsActive { get; set; } = true;
    }

    public class CustomerPurchaseHistoryDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public List<SaleDto> RecentPurchases { get; set; } = new List<SaleDto>();
        public decimal TotalSpent { get; set; }
        public int TotalTransactions { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
        public decimal LoyaltyPoints { get; set; }
        public decimal CreditBalance { get; set; }
    }
}
