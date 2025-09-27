using InventoryAPI.Models.DTOs;

namespace InventoryAPI.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto> GetCustomerByIdAsync(int id);
        Task<CustomerDto> GetCustomerByEmailAsync(string email);
        Task<CustomerDto> GetCustomerByPhoneAsync(string phone);
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto);
        Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerDto updateCustomerDto);
        Task<bool> DeleteCustomerAsync(int id);
        Task<CustomerPurchaseHistoryDto> GetCustomerPurchaseHistoryAsync(int customerId);
        Task<bool> UpdateLoyaltyPointsAsync(int customerId, decimal points);
        Task<bool> UpdateCreditBalanceAsync(int customerId, decimal amount);
        Task<IEnumerable<CustomerDto>> GetTopCustomersAsync(int count = 10);
        Task<IEnumerable<CustomerDto>> GetCustomersWithCreditBalanceAsync();
        Task<bool> MarkCustomerInactiveAsync(int id);
    }
}