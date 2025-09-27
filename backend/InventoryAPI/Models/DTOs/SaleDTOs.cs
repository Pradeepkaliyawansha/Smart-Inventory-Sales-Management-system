using System.ComponentModel.DataAnnotations;
using InventoryAPI.Models.Enums;

namespace InventoryAPI.Models.DTOs
{
    public class SaleDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int UserId { get; set; }
        public string SalesPersonName { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string Notes { get; set; }
        public bool IsCompleted { get; set; }
        public List<SaleItemDto> SaleItems { get; set; } = new List<SaleItemDto>();
    }

    public class CreateSaleDto
    {
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        public List<CreateSaleItemDto> SaleItems { get; set; } = new List<CreateSaleItemDto>();
        
        public decimal DiscountAmount { get; set; } = 0;
        
        public decimal TaxAmount { get; set; } = 0;
        
        [Required]
        public decimal PaidAmount { get; set; }
        
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        
        public string Notes { get; set; }
    }

    public class SaleItemDto
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CreateSaleItemDto
    {
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        public decimal UnitPrice { get; set; }
        
        [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100")]
        public decimal DiscountPercentage { get; set; } = 0;
    }

    public class InvoiceDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime SaleDate { get; set; }
        public CustomerDto Customer { get; set; }
        public UserDto SalesPerson { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string Notes { get; set; }
        public List<SaleItemDto> SaleItems { get; set; } = new List<SaleItemDto>();
    }

    public class SalesSummaryDto
    {
        public decimal TotalSales { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AverageTransactionValue { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
