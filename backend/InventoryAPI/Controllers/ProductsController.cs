using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryAPI.Models.DTOs;
using InventoryAPI.Models.Enums;
using InventoryAPI.Services.Interfaces;
using InventoryAPI.Exceptions;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                return Ok(product);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
        {
            try
            {
                var product = await _productService.CreateProductAsync(createProductDto);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(int id, UpdateProductDto updateProductDto)
        {
            try
            {
                var product = await _productService.UpdateProductAsync(id, updateProductDto);
                return Ok(product);
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
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result) return NotFound();

            return NoContent();
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStockProducts()
        {
            var products = await _productService.GetLowStockProductsAsync();
            return Ok(products);
        }

        [HttpPost("{id}/update-stock")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateStock(int id, UpdateStockDto updateStockDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("id")?.Value ?? "0");
                var result = await _productService.UpdateStockAsync(
                    id,
                    updateStockDto.Quantity,
                    updateStockDto.MovementType,
                    updateStockDto.Reference,
                    userId
                );

                if (!result) return NotFound();
                return Ok(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts([FromQuery] string term)
        {
            if (string.IsNullOrEmpty(term))
                return BadRequest(new { message = "Search term is required" });

            var products = await _productService.GetAllProductsAsync();
            var filtered = products.Where(p => 
                p.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                p.SKU.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (p.Barcode != null && p.Barcode.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (p.Description != null && p.Description.Contains(term, StringComparison.OrdinalIgnoreCase)));
            
            return Ok(filtered);
        }

        [HttpGet("barcode/{barcode}")]
        public async Task<ActionResult<ProductDto>> GetProductByBarcode(string barcode)
        {
            try
            {
                var product = await _productService.GetProductByBarcodeAsync(barcode);
                return Ok(product);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("sku/{sku}")]
        public async Task<ActionResult<ProductDto>> GetProductBySKU(string sku)
        {
            try
            {
                var product = await _productService.GetProductBySKUAsync(sku);
                return Ok(product);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/stock-movements")]
        public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetProductStockMovements(int id)
        {
            try
            {
                var movements = await _productService.GetStockMovementsAsync(id);
                return Ok(movements);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        
    }
}
