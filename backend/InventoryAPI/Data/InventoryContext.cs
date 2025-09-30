using Microsoft.EntityFrameworkCore;
using InventoryAPI.Models.Entities;
using InventoryAPI.Models.Enums;
using BCrypt.Net;

namespace InventoryAPI.Data
{
    public class InventoryContext : DbContext
    {
        public InventoryContext(DbContextOptions<InventoryContext> options) : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasConversion<int>();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("UTC_TIMESTAMP(6)");
            });
            
            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SKU).IsUnique();
                entity.HasIndex(e => e.Barcode).IsUnique();
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CostPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("UTC_TIMESTAMP(6)");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("UTC_TIMESTAMP(6)");
                
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Supplier)
                    .WithMany(s => s.Products)
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("UTC_TIMESTAMP(6)");
            });
            
            // Configure Supplier entity
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("UTC_TIMESTAMP(6)");
            });
            
            // Configure Sale entity
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.InvoiceNumber).IsUnique();
                entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaymentMethod).HasConversion<int>();
                entity.Property(e => e.SaleDate).HasDefaultValueSql("UTC_TIMESTAMP(6)");
                
                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Sales)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Configure SaleItem entity
            modelBuilder.Entity<SaleItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountPercentage).HasColumnType("decimal(5,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
                
                entity.HasOne(e => e.Sale)
                    .WithMany(s => s.SaleItems)
                    .HasForeignKey(e => e.SaleId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.SaleItems)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Configure Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LoyaltyPoints).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreditBalance).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("UTC_TIMESTAMP(6)");
            });
            
            // Configure StockMovement entity
            modelBuilder.Entity<StockMovement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MovementType).HasConversion<int>();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("UTC_TIMESTAMP(6)");
                
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.StockMovements)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Seed data
            SeedData(modelBuilder);
        }
        
        private void SeedData(ModelBuilder modelBuilder)
        {
            // CRITICAL FIX: Use static, hardcoded dates instead of new DateTime()
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            
            // Seed default admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@inventory.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    FullName = "System Administrator",
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = seedDate
                }
            );
            
            // Seed default categories
            modelBuilder.Entity<Category>().HasData(
                new Category 
                { 
                    Id = 1, 
                    Name = "Electronics", 
                    Description = "Electronic devices and accessories", 
                    CreatedAt = seedDate,
                    IsActive = true
                },
                new Category 
                { 
                    Id = 2, 
                    Name = "Clothing", 
                    Description = "Apparel and fashion items", 
                    CreatedAt = seedDate,
                    IsActive = true
                },
                new Category 
                { 
                    Id = 3, 
                    Name = "Books", 
                    Description = "Books and educational materials", 
                    CreatedAt = seedDate,
                    IsActive = true
                },
                new Category 
                { 
                    Id = 4, 
                    Name = "Home & Garden", 
                    Description = "Home improvement and garden supplies", 
                    CreatedAt = seedDate,
                    IsActive = true
                }
            );
            
            // Seed default suppliers
            modelBuilder.Entity<Supplier>().HasData(
                new Supplier 
                { 
                    Id = 1, 
                    Name = "TechCorp Supply", 
                    ContactPerson = "John Smith", 
                    Email = "john@techcorp.com", 
                    Phone = "+1-555-0101", 
                    Address = "123 Tech Street, Silicon Valley, CA", 
                    CreatedAt = seedDate,
                    IsActive = true
                },
                new Supplier 
                { 
                    Id = 2, 
                    Name = "Fashion Wholesale", 
                    ContactPerson = "Jane Doe", 
                    Email = "jane@fashionwholesale.com", 
                    Phone = "+1-555-0102", 
                    Address = "456 Fashion Ave, New York, NY", 
                    CreatedAt = seedDate,
                    IsActive = true
                },
                new Supplier 
                { 
                    Id = 3, 
                    Name = "Book Distributors Inc", 
                    ContactPerson = "Bob Wilson", 
                    Email = "bob@bookdist.com", 
                    Phone = "+1-555-0103", 
                    Address = "789 Literature Lane, Boston, MA", 
                    CreatedAt = seedDate,
                    IsActive = true
                }
            );
            
            // Seed default customer
            modelBuilder.Entity<Customer>().HasData(
                new Customer 
                { 
                    Id = 1, 
                    Name = "Walk-in Customer", 
                    Email = "walkin@store.com", 
                    Phone = "N/A", 
                    Address = "Store Location", 
                    LoyaltyPoints = 0, 
                    CreditBalance = 0, 
                    CreatedAt = seedDate,
                    IsActive = true
                }
            );
        }
    }
}