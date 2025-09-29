using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BCrypt.Net;
using InventoryAPI.Data;
using InventoryAPI.Models.Entities;
using InventoryAPI.Models.DTOs;
using InventoryAPI.Services.Interfaces;
using InventoryAPI.Helpers;

namespace InventoryAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly InventoryContext _context;
        private readonly IMapper _mapper;
        private readonly JwtHelper _jwtHelper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            InventoryContext context,
            IMapper mapper,
            JwtHelper jwtHelper,
            ILogger<AuthService> logger)
        {
            _context = context;
            _mapper = mapper;
            _jwtHelper = jwtHelper;
            _logger = logger;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Username == loginDto.Username && x.IsActive);

                if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    throw new UnauthorizedAccessException("Invalid username or password");
                }

                // FIX: Changed GenerateJwtToken to GenerateAccessToken
                var token = _jwtHelper.GenerateAccessToken(user);
                var refreshToken = _jwtHelper.GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                user.LastLogin = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(user);

                return new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Username}", loginDto.Username);
                throw;
            }
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if username already exists
                if (await _context.Users.AnyAsync(x => x.Username == registerDto.Username))
                {
                    throw new ArgumentException("Username already exists");
                }

                // Check if email already exists
                if (await _context.Users.AnyAsync(x => x.Email == registerDto.Email))
                {
                    throw new ArgumentException("Email already exists");
                }

                var user = _mapper.Map<User>(registerDto);
                
                // CRITICAL FIXES: Initialize properties and handle hashing
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true; 
                // Removed user.Id = 0; to let the database handle primary key generation.
                
                _context.Users.Add(user); // Add entity to context
                
                // Generate tokens and set user properties before the final save
                var token = _jwtHelper.GenerateAccessToken(user);
                var refreshToken = _jwtHelper.GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                user.LastLogin = DateTime.UtcNow; // Set initial LastLogin on registration

                // FINAL SAVE: Only one SaveChangesAsync is required for the entire transaction.
                await _context.SaveChangesAsync(); 

                var userDto = _mapper.Map<UserDto>(user);

                return new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {Username}", registerDto.Username);
                throw;
            }
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.RefreshToken == refreshTokenDto.RefreshToken && x.IsActive);

                if (user == null || user.RefreshTokenExpiry <= DateTime.UtcNow)
                {
                    throw new UnauthorizedAccessException("Invalid or expired refresh token");
                }

                // FIX: Changed GenerateJwtToken to GenerateAccessToken
                var newToken = _jwtHelper.GenerateAccessToken(user);
                var newRefreshToken = _jwtHelper.GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

                await _context.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(user);

                return new AuthResponseDto
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                throw;
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);

                if (user == null)
                    return false;

                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token revocation");
                return false;
            }
        }

        public async Task<UserDto> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);

                if (user == null)
                    throw new NotFoundException("User not found");

                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last login for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                    throw new UnauthorizedAccessException("Current password is incorrect");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Email == email && x.IsActive);

                if (user == null)
                    return false;

                // Generate temporary password
                var tempPassword = Guid.NewGuid().ToString()[..8];
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);

                await _context.SaveChangesAsync();

                // Here you would send an email with the temporary password
                // Implementation depends on your email service

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for email: {Email}", email);
                return false;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                // FIX: Changed ValidateJwtToken to ValidateToken
                return _jwtHelper.ValidateToken(token) != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return false;
            }
        }
    }

    // Custom exception classes
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }
}