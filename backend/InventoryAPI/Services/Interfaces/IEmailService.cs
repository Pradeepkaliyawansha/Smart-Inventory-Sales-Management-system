using InventoryAPI.Models.DTOs;

namespace InventoryAPI.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendStockAlertEmailAsync(List<StockAlertDto> stockAlerts);
        Task<bool> SendInvoiceEmailAsync(string customerEmail, InvoiceDto invoice);
        Task<bool> SendPasswordResetEmailAsync(string email, string resetToken);
        Task<bool> SendWelcomeEmailAsync(string email, string username, string temporaryPassword);
    }
}