using System.ComponentModel.DataAnnotations;
using InventoryAPI.Models.Enums;

namespace InventoryAPI.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        
        public UserRole Role { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLogin { get; set; }
        
        public string? RefreshToken { get; set; }
        
        public DateTime? RefreshTokenExpiry { get; set; }
    }
}