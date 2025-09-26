using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryAPI.Models.DTOs;
using InventoryAPI.Services.Interfaces;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _saleService;
        
        public SalesController(ISaleService saleService)
        {
            _saleService = saleService;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleDto>>> GetSales()
        {
            var sales = await _saleService.GetAllSalesAsync();
            return Ok(sales);
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<SaleDto>> GetSale(int id)
        {
            try
            {
                var sale = await _saleService.GetSaleByIdAsync(id);
                return Ok(sale);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<ActionResult<SaleDto>> CreateSale(CreateSaleDto createSaleDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("id")?.Value ?? "0");
                var sale = await _saleService.CreateSaleAsync(createSaleDto, userId);
                return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, sale);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        [HttpGet("{id}/invoice")]
        public async Task<ActionResult<InvoiceDto>> GetInvoice(int id)
        {
            try
            {
                var invoice = await _saleService.GenerateInvoiceAsync(id);
                return Ok(invoice);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        
        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<SaleDto>>> GetSalesByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var sales = await _saleService.GetSalesByDateRangeAsync(startDate, endDate);
            return Ok(sales);
        }
        
        [HttpGet("total-amount")]
        public async Task<ActionResult<decimal>> GetTotalSalesAmount([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var total = await _saleService.GetTotalSalesAmountAsync(startDate, endDate);
            return Ok(new { totalAmount = total });
        }
    }
}