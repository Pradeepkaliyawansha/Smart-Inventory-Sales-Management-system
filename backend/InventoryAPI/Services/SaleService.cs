using Microsoft.EntityFrameworkCore;
using AutoMapper;
using InventoryAPI.Data;
using InventoryAPI.Models.Entities;
using InventoryAPI.Models.DTOs;
using InventoryAPI.Models.Enums;
using InventoryAPI.Services.Interfaces;

namespace InventoryAPI.Services
{
    public class SaleService : ISaleService
    {
        private readonly InventoryContext _context;
        private readonly IMapper _mapper;
        private readonly IProductService _productService;
        private readonly ILogger<SaleService> _logger;

        public SaleService(
            InventoryContext context, 
            IMapper mapper, 
            IProductService productService,
            ILogger<SaleService> logger)
        {
            _context = context;
            _mapper = mapper;
            _productService = productService;
            _logger = logger;
        }

        public async Task<IEnumerable<SaleDto>> GetAllSalesAsync()
        {
            try
            {
                var sales = await _context.Sales
                    .Include(s => s.Customer)
                    .Include(s => s.User)
                    .Include(s => s.SaleItems)
                        .ThenInclude(si => si.Product)
                    .OrderByDescending(s => s.SaleDate)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<SaleDto>>(sales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all sales");
                throw;
            }
        }

        public async Task<SaleDto> GetSaleByIdAsync(int id)
        {
            try
            {
                var sale = await _context.Sales
                    .Include(s => s.Customer)
                    .Include(s => s.User)
                    .Include(s => s.SaleItems)
                        .ThenInclude(si => si.Product)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (sale == null)
                    throw new NotFoundException("Sale not found");

                return _mapper.Map<SaleDto>(sale);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sale by ID: {SaleId}", id);
                throw;
            }
        }

        public async Task<SaleDto> CreateSaleAsync(CreateSaleDto createSaleDto, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate stock availability for all items
                foreach (var item in createSaleDto.SaleItems)
                {
                    var hasStock = await _productService.CheckStockAvailabilityAsync(item.ProductId, item.Quantity);
                    if (!hasStock)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        throw new InvalidOperationException($"Insufficient stock for product: {product?.Name}");
                    }
                }

                // Create sale
                var sale = new Sale
                {
                    InvoiceNumber = await GenerateInvoiceNumberAsync(),
                    CustomerId = createSaleDto.CustomerId,
                    UserId = userId,
                    SaleDate = DateTime.UtcNow,
                    DiscountAmount = createSaleDto.DiscountAmount,
                    TaxAmount = createSaleDto.TaxAmount,
                    PaidAmount = createSaleDto.PaidAmount,
                    PaymentMethod = createSaleDto.PaymentMethod,
                    Notes = createSaleDto.Notes,
                    IsCompleted = true
                };

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                // Create sale items and calculate totals
                decimal subTotal = 0;
                foreach (var itemDto in createSaleDto.SaleItems)
                {
                    var product = await _context.Products.FindAsync(itemDto.ProductId);
                    var discountAmount = (itemDto.UnitPrice * itemDto.Quantity) * (itemDto.DiscountPercentage / 100);
                    var totalPrice = (itemDto.UnitPrice * itemDto.Quantity) - discountAmount;

                    var saleItem = new SaleItem
                    {
                        SaleId = sale.Id,
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice,
                        DiscountPercentage = itemDto.DiscountPercentage,
                        TotalPrice = totalPrice
                    };

                    _context.SaleItems.Add(saleItem);
                    subTotal += totalPrice;

                    // Update product stock
                    await _productService.UpdateStockAsync(
                        itemDto.ProductId,
                        itemDto.Quantity,
                        StockMovementType.Sale,
                        sale.InvoiceNumber,
                        userId);
                }

                // Update sale totals
                sale.SubTotal = subTotal;
                sale.TotalAmount = subTotal + createSaleDto.TaxAmount - createSaleDto.DiscountAmount;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetSaleByIdAsync(sale.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating sale");
                throw;
            }
        }

        public async Task<bool> CancelSaleAsync(int id, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var sale = await _context.Sales
                    .Include(s => s.SaleItems)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (sale == null)
                    return false;

                // Restore stock for each item
                foreach (var item in sale.SaleItems)
                {
                    await _productService.UpdateStockAsync(
                        item.ProductId,
                        item.Quantity,
                        StockMovementType.Return,
                        $"Sale cancellation - {sale.InvoiceNumber}",
                        userId);
                }

                // Mark sale as cancelled (you might want to add a status field)
                sale.IsCompleted = false;
                sale.Notes = $"{sale.Notes ?? ""} [CANCELLED]";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error cancelling sale: {SaleId}", id);
                return false;
            }
        }

        public async Task<InvoiceDto> GenerateInvoiceAsync(int saleId)
        {
            try
            {
                var sale = await _context.Sales
                    .Include(s => s.Customer)
                    .Include(s => s.User)
                    .Include(s => s.SaleItems)
                        .ThenInclude(si => si.Product)
                    .FirstOrDefaultAsync(s => s.Id == saleId);

                if (sale == null)
                    throw new NotFoundException("Sale not found");

                return _mapper.Map<InvoiceDto>(sale);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice for sale: {SaleId}", saleId);
                throw;
            }
        }

        public async Task<IEnumerable<SaleDto>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var sales = await _context.Sales
                    .Include(s => s.Customer)
                    .Include(s => s.User)
                    .Include(s => s.SaleItems)
                        .ThenInclude(si => si.Product)
                    .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
                    .OrderByDescending(s => s.SaleDate)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<SaleDto>>(sales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales by date range: {StartDate} - {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<IEnumerable<SaleDto>> GetSalesByCustomerAsync(int customerId)
        {
            try
            {
                var sales = await _context.Sales
                    .Include(s => s.Customer)
                    .Include(s => s.User)
                    .Include(s => s.SaleItems)
                        .ThenInclude(si => si.Product)
                    .Where(s => s.CustomerId == customerId)
                    .OrderByDescending(s => s.SaleDate)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<SaleDto>>(sales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales by customer: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<SaleDto>> GetSalesByUserAsync(int userId)
        {
            try
            {
                var sales = await _context.Sales
                    .Include(s => s.Customer)
                    .Include(s => s.User)
                    .Include(s => s.SaleItems)
                        .ThenInclude(si => si.Product)
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.SaleDate)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<SaleDto>>(sales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales by user: {UserId}", userId);
                throw;
            }
        }

        public async Task<decimal> GetTotalSalesAmountAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Sales.Where(s => s.IsCompleted);

                if (startDate.HasValue)
                    query = query.Where(s => s.SaleDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(s => s.SaleDate <= endDate.Value);

                return await query.SumAsync(s => s.TotalAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total sales amount");
                throw;
            }
        }

        public async Task<SalesSummaryDto> GetSalesSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Sales.Where(s => s.IsCompleted);

                if (startDate.HasValue)
                    query = query.Where(s => s.SaleDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(s => s.SaleDate <= endDate.Value);

                var sales = await query.ToListAsync();

                return new SalesSummaryDto
                {
                    TotalSales = sales.Sum(s => s.TotalAmount),
                    TotalTransactions = sales.Count,
                    AverageTransactionValue = sales.Any() ? sales.Average(s => s.TotalAmount) : 0,
                    FromDate = startDate,
                    ToDate = endDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales summary");
                throw;
            }
        }

        public async Task<IEnumerable<SaleDto>> GetTodaysSalesAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                var sales = await _context.Sales
                    .Include(s => s.Customer)
                    .Include(s => s.User)
                    .Include(s => s.SaleItems)
                        .ThenInclude(si => si.Product)
                    .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow)
                    .OrderByDescending(s => s.SaleDate)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<SaleDto>>(sales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today's sales");
                throw;
            }
        }

        public async Task<string> GenerateInvoiceNumberAsync()
        {
            try
            {
                var today = DateTime.UtcNow;
                var prefix = $"INV{today:yyyyMM}";
                
                var lastInvoice = await _context.Sales
                    .Where(s => s.InvoiceNumber.StartsWith(prefix))
                    .OrderByDescending(s => s.InvoiceNumber)
                    .Select(s => s.InvoiceNumber)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (!string.IsNullOrEmpty(lastInvoice))
                {
                    var numberPart = lastInvoice.Substring(prefix.Length);
                    if (int.TryParse(numberPart, out int currentNumber))
                    {
                        nextNumber = currentNumber + 1;
                    }
                }

                return $"{prefix}{nextNumber:D4}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice number");
                throw;
            }
        }
    }
}