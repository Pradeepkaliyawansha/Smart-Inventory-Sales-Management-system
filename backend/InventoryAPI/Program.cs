using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using InventoryAPI.Data;
using InventoryAPI.Services;
using InventoryAPI.Services.Interfaces;
using InventoryAPI.Helpers;
using InventoryAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Database
builder.Services.AddDbContext<InventoryContext>(options =>
    options.UseMySql(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
));

// Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Helpers
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<PdfGenerator>();

// CORS - Updated with more permissive settings for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
    
    // Add a more permissive policy for development
    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Inventory Management API",
        Version = "v1",
        Description = "API for managing inventory, sales, and customer data"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure URLs - NO CHANGE HERE, HTTP 5000 and HTTPS 7000 is correct
builder.WebHost.UseUrls("http://localhost:5000", "https://localhost:7000");

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory Management API V1");
        c.RoutePrefix = string.Empty;
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Force HTTPS redirect only in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

// IMPORTANT: CORS must come before Authentication and Authorization
if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");
}
else
{
    app.UseCors("AllowAngularApp");
}

// Custom middleware - put after CORS but before Auth
app.UseMiddleware<ExceptionMiddleware>();

// Authentication and Authorization must come AFTER CORS
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
    context.Database.EnsureCreated();
}

app.Run();