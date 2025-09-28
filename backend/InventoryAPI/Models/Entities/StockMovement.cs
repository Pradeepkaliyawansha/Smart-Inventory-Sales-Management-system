using System.ComponentModel.DataAnnotations;
using InventoryAPI.Models.Enums;

namespace InventoryAPI.Models.Entities
{
    public class StockMovement
    {
        public int Id { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        public Product Product { get; set; }
        
        [Required]
        public StockMovementType MovementType { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        public string? Reference { get; set; }
        
        public string? Notes { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public int CreatedBy { get; set; }
        
        public User CreatedByUser { get; set; }
    }
}
