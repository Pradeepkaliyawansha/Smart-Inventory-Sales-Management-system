using System.ComponentModel.DataAnnotations;

namespace InventoryAPI.Models.Entities
{
    public class Supplier
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(100)]
        public string? ContactPerson { get; set; }
        
        [EmailAddress]
        public string? Email { get; set; }
        
        [Phone]
        public string? Phone { get; set; }
        
        public string? Address { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
