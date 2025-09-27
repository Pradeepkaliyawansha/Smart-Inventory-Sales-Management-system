using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using InventoryAPI.Models.Entities;
using InventoryAPI.Models.Enums;

namespace InventoryAPI.Helpers
{
    /// <summary>
    /// Provides comprehensive JWT token management including generation, validation, 
    /// refresh token handling, and claims extraction
    /// </summary>
    public class JwtHelper
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtHelper> _logger;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenExpiryMinutes;
        private readonly int _refreshTokenExpiryDays;
        private readonly SymmetricSecurityKey _signingKey;
        private readonly SigningCredentials _signingCredentials;

        public JwtHelper(IConfiguration configuration, ILogger<JwtHelper> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _secretKey = _configuration["Jwt:Key"] ?? 
                throw new InvalidOperationException("JWT Key not configured");
            _issuer = _configuration["Jwt:Issuer"] ?? 
                throw new InvalidOperationException("JWT Issuer not configured");
            _audience = _configuration["Jwt:Audience"] ?? 
                throw new InvalidOperationException("JWT Audience not configured");
            
            if (!int.TryParse(_configuration["Jwt:ExpiryMinutes"], out _accessTokenExpiryMinutes))
                _accessTokenExpiryMinutes = 60; // Default 1 hour

            if (!int.TryParse(_configuration["Jwt:RefreshTokenExpiryDays"], out _refreshTokenExpiryDays))
                _refreshTokenExpiryDays = 7; // Default 7 days

            // Validate key length (minimum 256 bits for HS256)
            if (Encoding.UTF8.GetBytes(_secretKey).Length < 32)
                throw new InvalidOperationException("JWT key must be at least 256 bits (32 characters) long");

            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            _signingCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        }

        /// <summary>
        /// Generate JWT access token for user authentication
        /// </summary>
        /// <param name="user">User entity</param>
        /// <param name="customClaims">Additional custom claims (optional)</param>
        /// <returns>JWT token string</returns>
        public string GenerateAccessToken(User user, Dictionary<string, string> customClaims = null)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                var claims = CreateUserClaims(user);
                
                // Add custom claims if provided
                if (customClaims != null)
                {
                    foreach (var claim in customClaims)
                    {
                        claims.Add(new Claim(claim.Key, claim.Value));
                    }
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
                    Issuer = _issuer,
                    Audience = _audience,
                    SigningCredentials = _signingCredentials,
                    NotBefore = DateTime.UtcNow, // Token valid from now
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogDebug("Generated access token for user {UserId} ({Username})", user.Id, user.Username);
                
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating access token for user {UserId}", user.Id);
                throw new InvalidOperationException("Failed to generate access token", ex);
            }
        }

        /// <summary>
        /// Generate secure refresh token
        /// </summary>
        /// <returns>Base64 encoded refresh token</returns>
        public string GenerateRefreshToken()
        {
            try
            {
                var randomBytes = new byte[64];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomBytes);
                
                var refreshToken = Convert.ToBase64String(randomBytes);
                _logger.LogDebug("Generated new refresh token");
                
                return refreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating refresh token");
                throw new InvalidOperationException("Failed to generate refresh token", ex);
            }
        }

        /// <summary>
        /// Validate JWT token and return claims principal
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <param name="validateLifetime">Whether to validate token expiration (default: true)</param>
        /// <returns>ClaimsPrincipal if valid, null if invalid</returns>
        public ClaimsPrincipal ValidateToken(string token, bool validateLifetime = true)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetTokenValidationParameters(validateLifetime);

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                
                // Verify it's a JWT token with the correct algorithm
                if (validatedToken is not JwtSecurityToken jwtToken || 
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("Invalid token algorithm or token type");
                    return null;
                }

                _logger.LogDebug("Token validation successful");
                return principal;
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogDebug("Token has expired: {Error}", ex.Message);
                return null;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning("Token validation failed: {Error}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token validation");
                return null;
            }
        }

        /// <summary>
        /// Extract claims principal from expired token (useful for refresh operations)
        /// </summary>
        /// <param name="token">Expired JWT token</param>
        /// <returns>ClaimsPrincipal from expired token</returns>
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetTokenValidationParameters(validateLifetime: false);

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("Invalid token algorithm in expired token");
                    return null;
                }

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting principal from expired token");
                return null;
            }
        }

        /// <summary>
        /// Check if token is expired without full validation
        /// </summary>
        /// <param name="token">JWT token to check</param>
        /// <returns>True if token is expired</returns>
        public bool IsTokenExpired(string token)
        {
            if (string.IsNullOrEmpty(token))
                return true;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(token);
                
                return jwt.ValidTo <= DateTime.UtcNow;
            }
            catch (Exception)
            {
                return true;
            }
        }

        /// <summary>
        /// Get token expiration date
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Token expiration date or DateTime.MinValue if invalid</returns>
        public DateTime GetTokenExpirationDate(string token)
        {
            if (string.IsNullOrEmpty(token))
                return DateTime.MinValue;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(token);
                
                return jwt.ValidTo;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error reading token expiration date");
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Extract user ID from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>User ID or null if not found</returns>
        public int? GetUserIdFromToken(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null)
                return null;

            var userIdClaim = principal.FindFirst("id") ?? principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            return null;
        }

        /// <summary>
        /// Extract username from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Username or null if not found</returns>
        public string GetUsernameFromToken(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null)
                return null;

            return principal.FindFirst(ClaimTypes.Name)?.Value;
        }

        /// <summary>
        /// Extract user role from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>User role or null if not found</returns>
        public UserRole? GetUserRoleFromToken(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null)
                return null;

            var roleClaim = principal.FindFirst(ClaimTypes.Role);
            if (roleClaim != null && Enum.TryParse<UserRole>(roleClaim.Value, out UserRole role))
            {
                return role;
            }

            return null;
        }

        /// <summary>
        /// Extract all claims from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Dictionary of claims or empty dictionary if invalid</returns>
        public Dictionary<string, string> GetClaimsFromToken(string token)
        {
            var claims = new Dictionary<string, string>();
            
            var principal = ValidateToken(token);
            if (principal == null)
                return claims;

            foreach (var claim in principal.Claims)
            {
                if (!claims.ContainsKey(claim.Type))
                {
                    claims[claim.Type] = claim.Value;
                }
            }

            return claims;
        }

        /// <summary>
        /// Check if user has specific role based on token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <param name="requiredRole">Required role</param>
        /// <returns>True if user has the required role</returns>
        public bool HasRole(string token, UserRole requiredRole)
        {
            var userRole = GetUserRoleFromToken(token);
            return userRole.HasValue && userRole.Value == requiredRole;
        }

        /// <summary>
        /// Check if user has any of the specified roles
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <param name="roles">Required roles</param>
        /// <returns>True if user has any of the required roles</returns>
        public bool HasAnyRole(string token, params UserRole[] roles)
        {
            var userRole = GetUserRoleFromToken(token);
            return userRole.HasValue && roles.Contains(userRole.Value);
        }

        /// <summary>
        /// Generate a new access token from refresh token claims
        /// </summary>
        /// <param name="refreshToken">Valid refresh token</param>
        /// <param name="user">Current user entity</param>
        /// <returns>New access token</returns>
        public string RefreshAccessToken(string refreshToken, User user)
        {
            if (string.IsNullOrEmpty(refreshToken) || user == null)
                throw new ArgumentException("Invalid refresh token or user");

            try
            {
                // In a real implementation, you would validate the refresh token against your database
                // For now, we'll generate a new access token
                var newAccessToken = GenerateAccessToken(user);
                
                _logger.LogDebug("Refreshed access token for user {UserId}", user.Id);
                return newAccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing access token for user {UserId}", user.Id);
                throw new InvalidOperationException("Failed to refresh access token", ex);
            }
        }

        /// <summary>
        /// Revoke refresh token (mark as invalid)
        /// </summary>
        /// <param name="refreshToken">Refresh token to revoke</param>
        /// <returns>True if successfully revoked</returns>
        public bool RevokeRefreshToken(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            try
            {
                // In a real implementation, you would mark this token as revoked in your database
                // This is a placeholder for the actual revocation logic
                _logger.LogDebug("Refresh token revoked");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token");
                return false;
            }
        }

        /// <summary>
        /// Generate token for password reset
        /// </summary>
        /// <param name="user">User requesting password reset</param>
        /// <param name="expiryMinutes">Token expiry in minutes (default: 15)</param>
        /// <returns>Password reset token</returns>
        public string GeneratePasswordResetToken(User user, int expiryMinutes = 15)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                var claims = new List<Claim>
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim("email", user.Email),
                    new Claim("purpose", "password_reset"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, 
                        new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                        ClaimValueTypes.Integer64)
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                    Issuer = _issuer,
                    Audience = _audience,
                    SigningCredentials = _signingCredentials
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                _logger.LogDebug("Generated password reset token for user {UserId}", user.Id);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating password reset token for user {UserId}", user.Id);
                throw new InvalidOperationException("Failed to generate password reset token", ex);
            }
        }

        /// <summary>
        /// Validate password reset token
        /// </summary>
        /// <param name="token">Password reset token</param>
        /// <returns>User ID if valid, null otherwise</returns>
        public int? ValidatePasswordResetToken(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null)
                return null;

            var purposeClaim = principal.FindFirst("purpose");
            if (purposeClaim?.Value != "password_reset")
                return null;

            var userIdClaim = principal.FindFirst("id");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            return null;
        }

        /// <summary>
        /// Generate email verification token
        /// </summary>
        /// <param name="user">User for email verification</param>
        /// <param name="expiryHours">Token expiry in hours (default: 24)</param>
        /// <returns>Email verification token</returns>
        public string GenerateEmailVerificationToken(User user, int expiryHours = 24)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                var claims = new List<Claim>
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim("email", user.Email),
                    new Claim("purpose", "email_verification"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, 
                        new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                        ClaimValueTypes.Integer64)
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(expiryHours),
                    Issuer = _issuer,
                    Audience = _audience,
                    SigningCredentials = _signingCredentials
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                _logger.LogDebug("Generated email verification token for user {UserId}", user.Id);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating email verification token for user {UserId}", user.Id);
                throw new InvalidOperationException("Failed to generate email verification token", ex);
            }
        }

        /// <summary>
        /// Validate email verification token
        /// </summary>
        /// <param name="token">Email verification token</param>
        /// <returns>Tuple with user ID and email if valid</returns>
        public (int? userId, string email) ValidateEmailVerificationToken(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null)
                return (null, null);

            var purposeClaim = principal.FindFirst("purpose");
            if (purposeClaim?.Value != "email_verification")
                return (null, null);

            var userIdClaim = principal.FindFirst("id");
            var emailClaim = principal.FindFirst("email");

            int? userId = null;
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int id))
            {
                userId = id;
            }

            return (userId, emailClaim?.Value);
        }

        /// <summary>
        /// Get remaining time until token expires
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>TimeSpan until expiry or TimeSpan.Zero if expired/invalid</returns>
        public TimeSpan GetRemainingTokenTime(string token)
        {
            var expiryDate = GetTokenExpirationDate(token);
            if (expiryDate == DateTime.MinValue)
                return TimeSpan.Zero;

            var remaining = expiryDate - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        /// <summary>
        /// Create token metadata for logging and tracking
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Token metadata</returns>
        public TokenMetadata GetTokenMetadata(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(token);
                var principal = ValidateToken(token, validateLifetime: false);

                return new TokenMetadata
                {
                    TokenId = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value,
                    UserId = principal?.FindFirst("id")?.Value,
                    Username = principal?.FindFirst(ClaimTypes.Name)?.Value,
                    Role = principal?.FindFirst(ClaimTypes.Role)?.Value,
                    IssuedAt = jwt.IssuedAt,
                    ExpiresAt = jwt.ValidTo,
                    Issuer = jwt.Issuer,
                    Audience = jwt.Audiences?.FirstOrDefault(),
                    IsExpired = jwt.ValidTo <= DateTime.UtcNow,
                    Algorithm = jwt.Header.Alg
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting token metadata");
                return null;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Create standard claims for user authentication
        /// </summary>
        /// <param name="user">User entity</param>
        /// <returns>List of claims</returns>
        private List<Claim> CreateUserClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("fullName", user.FullName),
                new Claim("isActive", user.IsActive.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, 
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                    ClaimValueTypes.Integer64)
            };

            // Add last login if available
            if (user.LastLogin.HasValue)
            {
                claims.Add(new Claim("lastLogin", user.LastLogin.Value.ToString("O")));
            }

            return claims;
        }

        /// <summary>
        /// Get token validation parameters
        /// </summary>
        /// <param name="validateLifetime">Whether to validate token expiration</param>
        /// <returns>Token validation parameters</returns>
        private TokenValidationParameters GetTokenValidationParameters(bool validateLifetime = true)
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = validateLifetime,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = _signingKey,
                ClockSkew = TimeSpan.Zero, // No tolerance for clock skew
                RequireExpirationTime = true,
                RequireSignedTokens = true
            };
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Token metadata for tracking and logging
    /// </summary>
    public class TokenMetadata
    {
        public string TokenId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public bool IsExpired { get; set; }
        public string Algorithm { get; set; }
        public TimeSpan RemainingTime => IsExpired ? TimeSpan.Zero : ExpiresAt - DateTime.UtcNow;
    }

    /// <summary>
    /// JWT configuration options
    /// </summary>
    public class JwtOptions
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessTokenExpiryMinutes { get; set; } = 60;
        public int RefreshTokenExpiryDays { get; set; } = 7;
        public int PasswordResetTokenExpiryMinutes { get; set; } = 15;
        public int EmailVerificationTokenExpiryHours { get; set; } = 24;
    }

    #endregion
}