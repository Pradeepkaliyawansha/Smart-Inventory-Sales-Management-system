using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryAPI.Models.DTOs;
using InventoryAPI.Services.Interfaces;
using InventoryAPI.Exceptions;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        
        [HttpPost("login")]
        [AllowAnonymous] // Add this attribute
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
        
        [HttpPost("register")]
        [AllowAnonymous] 
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during registration", error = ex.Message });
            }
        }
        
        [HttpPost("refresh-token")]
        [AllowAnonymous] // Add this attribute
        public async Task<ActionResult<AuthResponseDto>> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(refreshTokenDto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
        
        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] string refreshToken)
        {
            var result = await _authService.RevokeTokenAsync(refreshToken);
            return Ok(new { success = result });
        }
        
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirst("id")?.Value ?? "0");
            var user = await _authService.GetUserByIdAsync(userId);
            return Ok(user);
        }
    }
}