using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using InventoryAPI.Data;
using InventoryAPI.Helpers;
using InventoryAPI.Models.Enums;

namespace InventoryAPI.Middleware
{
    /// <summary>
    /// JWT Middleware for custom token validation, user context setting, 
    /// and advanced authentication features
    /// </summary>
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtMiddleware> _logger;
        private readonly JwtHelper _jwtHelper;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public JwtMiddleware(
            RequestDelegate next,
            ILogger<JwtMiddleware> logger,
            JwtHelper jwtHelper,
            IServiceScopeFactory serviceScopeFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jwtHelper = jwtHelper ?? throw new ArgumentNullException(nameof(jwtHelper));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        /// <summary>
        /// Process HTTP request and handle JWT authentication
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Task</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await ProcessTokenAsync(context);
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in JWT middleware processing");
                await HandleErrorAsync(context, ex);
            }
        }

        /// <summary>
        /// Process JWT token from request and set user context
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Task</returns>
        private async Task ProcessTokenAsync(HttpContext context)
        {
            // Skip processing for certain endpoints
            if (ShouldSkipTokenProcessing(context))
            {
                return;
            }

            var token = ExtractTokenFromRequest(context);
            
            if (string.IsNullOrEmpty(token))
            {
                // No token provided - this is okay for public endpoints
                _logger.LogDebug("No JWT token provided for {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
                return;
            }

            await ValidateAndSetUserContextAsync(context, token);
        }

        /// <summary>
        /// Extract JWT token from various sources in the request
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>JWT token string or null</returns>
        private string ExtractTokenFromRequest(HttpContext context)
        {
            // 1. Check Authorization header (Bearer token)
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring(7).Trim();
                if (!string.IsNullOrEmpty(token))
                {
                    _logger.LogDebug("Token extracted from Authorization header");
                    return token;
                }
            }

            // 2. Check query parameter (for URLs that can't use headers)
            var queryToken = context.Request.Query["token"].FirstOrDefault();
            if (!string.IsNullOrEmpty(queryToken))
            {
                _logger.LogDebug("Token extracted from query parameter");
                return queryToken;
            }

            // 3. Check custom header (X-Access-Token)
            var customHeaderToken = context.Request.Headers["X-Access-Token"].FirstOrDefault();
            if (!string.IsNullOrEmpty(customHeaderToken))
            {
                _logger.LogDebug("Token extracted from custom header");
                return customHeaderToken;
            }

            // 4. Check cookie (for browser-based applications)
            if (context.Request.Cookies.TryGetValue("access_token", out string cookieToken) && 
                !string.IsNullOrEmpty(cookieToken))
            {
                _logger.LogDebug("Token extracted from cookie");
                return cookieToken;
            }

            return null;
        }

        /// <summary>
        /// Validate JWT token and set user context
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="token">JWT token</param>
        /// <returns>Task</returns>
        private async Task ValidateAndSetUserContextAsync(HttpContext context, string token)
        {
            try
            {
                // Validate token using JWT helper
                var principal = _jwtHelper.ValidateToken(token);
                if (principal == null)
                {
                    _logger.LogWarning("Invalid JWT token provided from {IP}", 
                        context.Connection.RemoteIpAddress);
                    await HandleUnauthorizedAsync(context, "Invalid or expired token");
                    return;
                }

                // Extract user information from token
                var userIdClaim = principal.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Token missing valid user ID claim");
                    await HandleUnauthorizedAsync(context, "Invalid token claims");
                    return;
                }

                // Validate user exists and is active
                var user = await ValidateUserAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Token contains non-existent or inactive user ID: {UserId}", userId);
                    await HandleUnauthorizedAsync(context, "User not found or inactive");
                    return;
                }

                // Set user principal in context
                context.User = principal;

                // Add additional user context information
                context.Items["UserId"] = userId;
                context.Items["Username"] = user.Username;
                context.Items["UserRole"] = user.Role;
                context.Items["UserFullName"] = user.FullName;
                context.Items["TokenMetadata"] = _jwtHelper.GetTokenMetadata(token);

                // Log successful authentication
                _logger.LogDebug("User {UserId} ({Username}) authenticated successfully for {Method} {Path}", 
                    userId, user.Username, context.Request.Method, context.Request.Path);

                // Check for token expiry warning (if expires within 5 minutes)
                var remainingTime = _jwtHelper.GetRemainingTokenTime(token);
                if (remainingTime <= TimeSpan.FromMinutes(5) && remainingTime > TimeSpan.Zero)
                {
                    context.Response.Headers.Add("X-Token-Expires-Soon", "true");
                    context.Response.Headers.Add("X-Token-Expires-In", ((int)remainingTime.TotalSeconds).ToString());
                    _logger.LogDebug("Token for user {UserId} expires in {Minutes} minutes", 
                        userId, remainingTime.TotalMinutes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating JWT token");
                await HandleUnauthorizedAsync(context, "Token validation failed");
            }
        }

        /// <summary>
        /// Validate that user exists and is active in database
        /// </summary>
        /// <param name="userId">User ID to validate</param>
        /// <returns>User entity or null if not found/inactive</returns>
        private async Task<dynamic> ValidateUserAsync(int userId)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();

                var user = await context.Users
                    .Where(u => u.Id == userId && u.IsActive)
                    .Select(u => new { u.Id, u.Username, u.Role, u.FullName, u.IsActive })
                    .FirstOrDefaultAsync();

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user {UserId} in database", userId);
                return null;
            }
        }

        /// <summary>
        /// Check if token processing should be skipped for this request
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>True if should skip processing</returns>
        private bool ShouldSkipTokenProcessing(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant();
            
            // Skip for public endpoints
            var publicEndpoints = new[]
            {
                "/",
                "/health",
                "/api/auth/login",
                "/api/auth/register",
                "/api/auth/refresh-token",
                "/swagger",
                "/favicon.ico"
            };

            if (publicEndpoints.Any(endpoint => path?.StartsWith(endpoint) == true))
            {
                return true;
            }

            // Skip for static files
            var staticExtensions = new[] { ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".svg", ".woff", ".woff2" };
            if (staticExtensions.Any(ext => path?.EndsWith(ext) == true))
            {
                return true;
            }

            // Skip for development endpoints in development environment
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handle unauthorized access
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="message">Error message</param>
        /// <returns>Task</returns>
        private async Task HandleUnauthorizedAsync(HttpContext context, string message)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Unauthorized",
                message = message,
                statusCode = 401,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path.Value
            };

            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }

        /// <summary>
        /// Handle errors that occur during middleware processing
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="exception">Exception that occurred</param>
        /// <returns>Task</returns>
        private async Task HandleErrorAsync(HttpContext context, Exception exception)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response has already started, cannot send error response");
                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Internal Server Error",
                message = "An error occurred while processing your request",
                statusCode = 500,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path.Value
            };

            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// Advanced JWT Middleware with additional security features
    /// </summary>
    public class AdvancedJwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AdvancedJwtMiddleware> _logger;
        private readonly JwtHelper _jwtHelper;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _configuration;

        // Rate limiting and security settings
        private readonly int _maxRequestsPerMinute;
        private readonly bool _enableIpWhitelist;
        private readonly List<string> _whitelistedIPs;
        private readonly Dictionary<string, TokenUsageInfo> _tokenUsageTracker;
        private readonly object _lockObject = new object();

        public AdvancedJwtMiddleware(
            RequestDelegate next,
            ILogger<AdvancedJwtMiddleware> logger,
            JwtHelper jwtHelper,
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _jwtHelper = jwtHelper;
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;

            _maxRequestsPerMinute = _configuration.GetValue<int>("Security:MaxRequestsPerMinute", 100);
            _enableIpWhitelist = _configuration.GetValue<bool>("Security:EnableIpWhitelist", false);
            _whitelistedIPs = _configuration.GetSection("Security:WhitelistedIPs").Get<List<string>>() ?? new List<string>();
            _tokenUsageTracker = new Dictionary<string, TokenUsageInfo>();

            // Clean up token usage tracker every 5 minutes
            var timer = new Timer(CleanupTokenUsageTracker, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // IP whitelist check
                if (_enableIpWhitelist && !IsIpWhitelisted(context))
                {
                    await HandleForbiddenAsync(context, "IP address not whitelisted");
                    return;
                }

                var token = ExtractTokenFromRequest(context);
                
                if (!string.IsNullOrEmpty(token))
                {
                    // Rate limiting per token
                    if (!CheckRateLimit(token))
                    {
                        await HandleTooManyRequestsAsync(context);
                        return;
                    }

                    // Token blacklist check (for revoked tokens)
                    if (await IsTokenBlacklistedAsync(token))
                    {
                        await HandleUnauthorizedAsync(context, "Token has been revoked");
                        return;
                    }

                    // Validate and set user context
                    await ValidateAndSetUserContextAsync(context, token);
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in advanced JWT middleware");
                await HandleErrorAsync(context, ex);
            }
        }

        private string ExtractTokenFromRequest(HttpContext context)
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader.Substring(7).Trim();
            }
            return null;
        }

        private async Task ValidateAndSetUserContextAsync(HttpContext context, string token)
        {
            var principal = _jwtHelper.ValidateToken(token);
            if (principal == null)
            {
                await HandleUnauthorizedAsync(context, "Invalid or expired token");
                return;
            }

            var userIdClaim = principal.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                await HandleUnauthorizedAsync(context, "Invalid token claims");
                return;
            }

            // Additional security checks
            var tokenMetadata = _jwtHelper.GetTokenMetadata(token);
            if (tokenMetadata != null)
            {
                // Check for suspicious activity (e.g., token used from different IP than usual)
                await LogTokenUsageAsync(token, context.Connection.RemoteIpAddress?.ToString(), userId);
                
                // Add security headers
                context.Response.Headers.Add("X-Token-ID", tokenMetadata.TokenId);
                context.Response.Headers.Add("X-User-Context", $"User:{userId}");
            }

            context.User = principal;
            context.Items["UserId"] = userId;
            context.Items["TokenMetadata"] = tokenMetadata;
        }

        private bool IsIpWhitelisted(HttpContext context)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            return string.IsNullOrEmpty(clientIp) || _whitelistedIPs.Contains(clientIp);
        }

        private bool CheckRateLimit(string token)
        {
            lock (_lockObject)
            {
                var tokenHash = token.GetHashCode().ToString(); // Use hash for privacy
                var now = DateTime.UtcNow;

                if (_tokenUsageTracker.TryGetValue(tokenHash, out var usage))
                {
                    // Reset counter if minute has passed
                    if ((now - usage.WindowStart).TotalMinutes >= 1)
                    {
                        usage.RequestCount = 1;
                        usage.WindowStart = now;
                    }
                    else
                    {
                        usage.RequestCount++;
                        if (usage.RequestCount > _maxRequestsPerMinute)
                        {
                            _logger.LogWarning("Rate limit exceeded for token hash: {TokenHash}", tokenHash);
                            return false;
                        }
                    }
                }
                else
                {
                    _tokenUsageTracker[tokenHash] = new TokenUsageInfo
                    {
                        RequestCount = 1,
                        WindowStart = now
                    };
                }

                return true;
            }
        }

        private async Task<bool> IsTokenBlacklistedAsync(string token)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();

                var tokenMetadata = _jwtHelper.GetTokenMetadata(token);
                if (tokenMetadata?.TokenId == null)
                    return false;

                // Check if token is in blacklist table (you would need to create this table)
                // For now, we'll just check if the user is still active
                if (int.TryParse(tokenMetadata.UserId, out int userId))
                {
                    var user = await context.Users.FindAsync(userId);
                    return user == null || !user.IsActive;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking token blacklist");
                return false; // Fail open for availability
            }
        }

        private async Task LogTokenUsageAsync(string token, string ipAddress, int userId)
        {
            try
            {
                // Log token usage for security monitoring
                var tokenMetadata = _jwtHelper.GetTokenMetadata(token);
                _logger.LogInformation("Token usage: User {UserId}, Token {TokenId}, IP {IP}, Time {Time}",
                    userId, tokenMetadata?.TokenId, ipAddress, DateTime.UtcNow);
                
                // Here you could store usage patterns in database for security analytics
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging token usage");
            }
        }

        private void CleanupTokenUsageTracker(object state)
        {
            lock (_lockObject)
            {
                var cutoff = DateTime.UtcNow.AddMinutes(-5);
                var keysToRemove = _tokenUsageTracker
                    .Where(kvp => kvp.Value.WindowStart < cutoff)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _tokenUsageTracker.Remove(key);
                }

                _logger.LogDebug("Cleaned up {Count} expired token usage entries", keysToRemove.Count);
            }
        }

        #region Error Handling Methods

        private async Task HandleUnauthorizedAsync(HttpContext context, string message)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Unauthorized",
                message = message,
                statusCode = 401,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }

        private async Task HandleForbiddenAsync(HttpContext context, string message)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Forbidden",
                message = message,
                statusCode = 403,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }

        private async Task HandleTooManyRequestsAsync(HttpContext context)
        {
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.ContentType = "application/json";
            context.Response.Headers.Add("Retry-After", "60");

            var response = new
            {
                error = "Too Many Requests",
                message = "Rate limit exceeded. Please try again later.",
                statusCode = 429,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }

        private async Task HandleErrorAsync(HttpContext context, Exception exception)
        {
            if (context.Response.HasStarted)
                return;

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Internal Server Error",
                message = "An error occurred while processing your request",
                statusCode = 500,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Token usage information for rate limiting
    /// </summary>
    public class TokenUsageInfo
    {
        public int RequestCount { get; set; }
        public DateTime WindowStart { get; set; }
        public string LastIpAddress { get; set; }
        public List<DateTime> RecentRequests { get; set; } = new List<DateTime>();
    }

    /// <summary>
    /// JWT middleware configuration extensions
    /// </summary>
    public static class JwtMiddlewareExtensions
    {
        /// <summary>
        /// Add JWT middleware to the pipeline
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtMiddleware>();
        }

        /// <summary>
        /// Add advanced JWT middleware to the pipeline
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseAdvancedJwtMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AdvancedJwtMiddleware>();
        }
    }

    #endregion
}