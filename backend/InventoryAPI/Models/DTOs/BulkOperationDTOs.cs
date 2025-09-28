using System.ComponentModel.DataAnnotations;

namespace InventoryAPI.Models.DTOs
{
    public class BulkSupplierStatusUpdateDto
    {
        [Required]
        public List<int> SupplierIds { get; set; } = new List<int>();
        
        [Required]
        public bool IsActive { get; set; }
        
        public string Reason { get; set; } = "";
    }

    public class BulkCategoryStatusUpdateDto
    {
        [Required]
        public List<int> CategoryIds { get; set; } = new List<int>();
        
        [Required]
        public bool IsActive { get; set; }
        
        public string Reason { get; set; } = "";
    }
}