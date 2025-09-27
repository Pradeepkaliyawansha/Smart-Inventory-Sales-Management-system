using Microsoft.EntityFrameworkCore;
using AutoMapper;
using InventoryAPI.Data;
using InventoryAPI.Models.Entities;
using InventoryAPI.Models.DTOs;
using InventoryAPI.Services.Interfaces;

namespace InventoryAPI.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly InventoryContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(InventoryContext context, IMapper mapper, ILogger<CustomerService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            try
            {
                var customers = await _context.Customers
                    .Where(c => c.IsActive)
                    .Select(c => new CustomerDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Email = c.Email,
                        Phone = c.Phone,
                        Address = c.Address,
                        LoyaltyPoints = c.LoyaltyPoints,
                        CreditBalance = c.CreditBalance,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        LastPurchaseDate = c.LastPurchaseDate,
                        TotalPurchases = c.Sales.Count(),
                        TotalSpent = c.Sales.Sum(s => s.TotalAmount)
                    })
                    .ToListAsync();

                return customers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all customers");
                throw;
            }
        }

        public async Task<CustomerDto> GetCustomerByIdAsync(int id)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.Sales)
                    .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

                if (customer == null)
                    throw new NotFoundException("Customer not found");

                var customerDto = _mapper.Map<CustomerDto>(customer);
                customerDto.TotalPurchases = customer.Sales.Count;
                customerDto.TotalSpent = customer.Sales.Sum(s => s.TotalAmount);

                return customerDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by ID: {CustomerId}", id);
                throw;
            }
        }

        public async Task<CustomerDto> GetCustomerByEmailAsync(string email)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.Sales)
                    .FirstOrDefaultAsync(c => c.Email == email && c.IsActive);

                if (customer == null)
                    throw new NotFoundException("Customer not found");

                var customerDto = _mapper.Map<CustomerDto>(customer);
                customerDto.TotalPurchases = customer.Sales.Count;
                customerDto.TotalSpent = customer.Sales.Sum(s => s.TotalAmount);

                return customerDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by email: {Email}", email);
                throw;
            }
        }

        public async Task<CustomerDto> GetCustomerByPhoneAsync(string phone)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.Sales)
                    .FirstOrDefaultAsync(c => c.Phone == phone && c.IsActive);

                if (customer == null)
                    throw new NotFoundException("Customer not found");

                var customerDto = _mapper.Map<CustomerDto>(customer);
                customerDto.TotalPurchases = customer.Sales.Count;
                customerDto.TotalSpent = customer.Sales.Sum(s => s.TotalAmount);

                return customerDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by phone: {Phone}", phone);
                throw;
            }
        }

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto)
        {
            try
            {
                if (!string.IsNullOrEmpty(createCustomerDto.Email) && 
                    await _context.Customers.AnyAsync(c => c.Email == createCustomerDto.Email && c.IsActive))
                {
                    throw new ArgumentException("A customer with this email already exists");
                }

                var customer = _mapper.Map<Customer>(createCustomerDto);
                customer.CreatedAt = DateTime.UtcNow;

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                return await GetCustomerByIdAsync(customer.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer: {CustomerName}", createCustomerDto.Name);
                throw;
            }
        }

        public async Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerDto updateCustomerDto)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                    throw new NotFoundException("Customer not found");

                if (!string.IsNullOrEmpty(updateCustomerDto.Email) && 
                    await _context.Customers.AnyAsync(c => c.Email == updateCustomerDto.Email && c.Id != id && c.IsActive))
                {
                    throw new ArgumentException("A customer with this email already exists");
                }

                _mapper.Map(updateCustomerDto, customer);
                await _context.SaveChangesAsync();

                return await GetCustomerByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer: {CustomerId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                    return false;

                customer.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer: {CustomerId}", id);
                return false;
            }
        }

        public async Task<CustomerPurchaseHistoryDto> GetCustomerPurchaseHistoryAsync(int customerId)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.Sales)
                        .ThenInclude(s => s.SaleItems)
                            .ThenInclude(si => si.Product)
                    .Include(c => c.Sales)
                        .ThenInclude(s => s.User)
                    .FirstOrDefaultAsync(c => c.Id == customerId && c.IsActive);

                if (customer == null)
                    throw new NotFoundException("Customer not found");

                var recentPurchases = customer.Sales
                    .OrderByDescending(s => s.SaleDate)
                    .Take(20)
                    .ToList();

                return new CustomerPurchaseHistoryDto
                {
                    CustomerId = customer.Id,
                    CustomerName = customer.Name,
                    RecentPurchases = _mapper.Map<List<SaleDto>>(recentPurchases),
                    TotalSpent = customer.Sales.Sum(s => s.TotalAmount),
                    TotalTransactions = customer.Sales.Count,
                    LastPurchaseDate = customer.LastPurchaseDate,
                    LoyaltyPoints = customer.LoyaltyPoints,
                    CreditBalance = customer.CreditBalance
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer purchase history: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> UpdateLoyaltyPointsAsync(int customerId, decimal points)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                    return false;

                customer.LoyaltyPoints += points;
                if (customer.LoyaltyPoints < 0)
                    customer.LoyaltyPoints = 0;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loyalty points for customer: {CustomerId}", customerId);
                return false;
            }
        }

        public async Task<bool> UpdateCreditBalanceAsync(int customerId, decimal amount)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                    return false;

                customer.CreditBalance += amount;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating credit balance for customer: {CustomerId}", customerId);
                return false;
            }
        }

        public async Task<IEnumerable<CustomerDto>> GetTopCustomersAsync(int count = 10)
        {
            try
            {
                var topCustomers = await _context.Customers
                    .Where(c => c.IsActive)
                    .Include(c => c.Sales)
                    .Select(c => new CustomerDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Email = c.Email,
                        Phone = c.Phone,
                        Address = c.Address,
                        LoyaltyPoints = c.LoyaltyPoints,
                        CreditBalance = c.CreditBalance,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        LastPurchaseDate = c.LastPurchaseDate,
                        TotalPurchases = c.Sales.Count(),
                        TotalSpent = c.Sales.Sum(s => s.TotalAmount)
                    })
                    .OrderByDescending(c => c.TotalSpent)
                    .Take(count)
                    .ToListAsync();

                return topCustomers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top customers");
                throw;
            }
        }

        public async Task<IEnumerable<CustomerDto>> GetCustomersWithCreditBalanceAsync()
        {
            try
            {
                var customers = await _context.Customers
                    .Where(c => c.IsActive && c.CreditBalance > 0)
                    .Include(c => c.Sales)
                    .Select(c => new CustomerDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Email = c.Email,
                        Phone = c.Phone,
                        Address = c.Address,
                        LoyaltyPoints = c.LoyaltyPoints,
                        CreditBalance = c.CreditBalance,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        LastPurchaseDate = c.LastPurchaseDate,
                        TotalPurchases = c.Sales.Count(),
                        TotalSpent = c.Sales.Sum(s => s.TotalAmount)
                    })
                    .OrderByDescending(c => c.CreditBalance)
                    .ToListAsync();

                return customers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers with credit balance");
                throw;
            }
        }

        public async Task<bool> MarkCustomerInactiveAsync(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                    return false;

                customer.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking customer inactive: {CustomerId}", id);
                return false;
            }
        }
    }
}
