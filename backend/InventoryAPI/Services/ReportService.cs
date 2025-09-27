using InventoryAPI.Models.DTOs;

namespace InventoryAPI.Services.Interfaces
{
    public interface ISaleService
    {
        Task<IEnumerable<SaleDto>> GetAllSalesAsync();
        Task<SaleDto> GetSaleByIdAsync(int id);
        Task<SaleDto> CreateSaleAsync(CreateSaleDto createSaleDto, int userId);
        Task<bool> CancelSaleAsync(int id, int userId);
        Task<InvoiceDto> GenerateInvoiceAsync(int saleId);
        Task<IEnumerable<SaleDto>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<SaleDto>> GetSalesByCustomerAsync(int customerId);
        Task<IEnumerable<SaleDto>> GetSalesByUserAsync(int userId);
        Task<decimal> GetTotalSalesAmountAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<SalesSummaryDto> GetSalesSummaryAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<SaleDto>> GetTodaysSalesAsync();
        Task<string> GenerateInvoiceNumberAsync();
    }
}