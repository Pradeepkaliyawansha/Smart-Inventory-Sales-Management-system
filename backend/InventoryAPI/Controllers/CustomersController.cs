using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryAPI.Models.DTOs;
using InventoryAPI.Services.Interfaces;
using InventoryAPI.Exceptions;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                return Ok(customer);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto createCustomerDto)
        {
            try
            {
                var customer = await _customerService.CreateCustomerAsync(createCustomerDto);
                return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CustomerDto>> UpdateCustomer(int id, UpdateCustomerDto updateCustomerDto)
        {
            try
            {
                var customer = await _customerService.UpdateCustomerAsync(id, updateCustomerDto);
                return Ok(customer);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var result = await _customerService.DeleteCustomerAsync(id);
            if (!result) return NotFound();

            return NoContent();
        }

        [HttpGet("{id}/purchase-history")]
        public async Task<ActionResult<CustomerPurchaseHistoryDto>> GetCustomerPurchaseHistory(int id)
        {
            try
            {
                var history = await _customerService.GetCustomerPurchaseHistoryAsync(id);
                return Ok(history);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("top")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetTopCustomers([FromQuery] int count = 10)
        {
            var topCustomers = await _customerService.GetTopCustomersAsync(count);
            return Ok(topCustomers);
        }

        [HttpGet("with-credit")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomersWithCredit()
        {
            var customers = await _customerService.GetCustomersWithCreditBalanceAsync();
            return Ok(customers);
        }

        [HttpPost("{id}/loyalty-points")]
        public async Task<IActionResult> UpdateLoyaltyPoints(int id, [FromBody] decimal points)
        {
            var result = await _customerService.UpdateLoyaltyPointsAsync(id, points);
            if (!result) return NotFound();

            return Ok(new { success = true });
        }

        [HttpPost("{id}/credit-balance")]
        public async Task<IActionResult> UpdateCreditBalance(int id, [FromBody] decimal amount)
        {
            var result = await _customerService.UpdateCreditBalanceAsync(id, amount);
            if (!result) return NotFound();

            return Ok(new { success = true });
        }

        [HttpGet("search")]
        public async Task<ActionResult<CustomerDto>> SearchCustomer([FromQuery] string? email, [FromQuery] string? phone)
        {
            try
            {
                CustomerDto customer;

                if (!string.IsNullOrEmpty(email))
                {
                    customer = await _customerService.GetCustomerByEmailAsync(email);
                }
                else if (!string.IsNullOrEmpty(phone))
                {
                    customer = await _customerService.GetCustomerByPhoneAsync(phone);
                }
                else
                {
                    return BadRequest(new { message = "Either email or phone must be provided" });
                }

                return Ok(customer);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpGet("{id}/loyalty-summary")]
        public async Task<ActionResult> GetCustomerLoyaltySummary(int id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                return Ok(new 
                { 
                    customerId = customer.Id,
                    customerName = customer.Name,
                    loyaltyPoints = customer.LoyaltyPoints,
                    creditBalance = customer.CreditBalance,
                    totalSpent = customer.TotalSpent,
                    totalPurchases = customer.TotalPurchases,
                    lastPurchaseDate = customer.LastPurchaseDate
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/redeem-points")]
        public async Task<IActionResult> RedeemLoyaltyPoints(int id, [FromBody] decimal pointsToRedeem)
        {
            try
            {
                if (pointsToRedeem <= 0)
                    return BadRequest(new { message = "Points to redeem must be greater than 0" });

                var customer = await _customerService.GetCustomerByIdAsync(id);
                
                if (customer.LoyaltyPoints < pointsToRedeem)
                    return BadRequest(new { message = "Insufficient loyalty points" });

                var result = await _customerService.UpdateLoyaltyPointsAsync(id, -pointsToRedeem);
                if (!result) return NotFound();
                
                return Ok(new { success = true, pointsRedeemed = pointsToRedeem });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}