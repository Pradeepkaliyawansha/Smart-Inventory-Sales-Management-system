using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryAPI.Models.Entities;

namespace InventoryAPI.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // Primary Key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(p => p.SKU)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Barcode)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.CostPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.StockQuantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.MinStockLevel)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.CategoryId)
                .IsRequired();

            builder.Property(p => p.SupplierId)
                .IsRequired();

            builder.Property(p => p.ImageUrl)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            builder.Property(p => p.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            // Indexes
            builder.HasIndex(p => p.SKU)
                .IsUnique()
                .HasDatabaseName("IX_Products_SKU");

            builder.HasIndex(p => p.Barcode)
                .IsUnique()
                .HasDatabaseName("IX_Products_Barcode");

            builder.HasIndex(p => p.Name)
                .HasDatabaseName("IX_Products_Name");

            builder.HasIndex(p => new { p.CategoryId, p.IsActive })
                .HasDatabaseName("IX_Products_Category_Active");

            builder.HasIndex(p => new { p.SupplierId, p.IsActive })
                .HasDatabaseName("IX_Products_Supplier_Active");

            builder.HasIndex(p => new { p.StockQuantity, p.MinStockLevel })
                .HasDatabaseName("IX_Products_Stock_Levels");

            builder.HasIndex(p => p.IsActive)
                .HasDatabaseName("IX_Products_IsActive");

            builder.HasIndex(p => p.UpdatedAt)
                .HasDatabaseName("IX_Products_UpdatedAt");

            // Relationships
            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Products_Categories");

            builder.HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Products_Suppliers");

            builder.HasMany(p => p.SaleItems)
                .WithOne(si => si.Product)
                .HasForeignKey(si => si.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_SaleItems_Products");

            builder.HasMany(p => p.StockMovements)
                .WithOne(sm => sm.Product)
                .HasForeignKey(sm => sm.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_StockMovements_Products");

            // Table Configuration
            builder.ToTable("Products", schema: "dbo");

            // Soft Delete Filter
            builder.HasQueryFilter(p => p.IsActive);

            // Check Constraints
            builder.HasCheckConstraint("CK_Products_Name_NotEmpty", "LEN([Name]) > 0");
            builder.HasCheckConstraint("CK_Products_SKU_NotEmpty", "LEN([SKU]) > 0");
            builder.HasCheckConstraint("CK_Products_Barcode_NotEmpty", "LEN([Barcode]) > 0");
            builder.HasCheckConstraint("CK_Products_Price_Positive", "[Price] > 0");
            builder.HasCheckConstraint("CK_Products_CostPrice_Positive", "[CostPrice] > 0");
            builder.HasCheckConstraint("CK_Products_StockQuantity_NonNegative", "[StockQuantity] >= 0");
            builder.HasCheckConstraint("CK_Products_MinStockLevel_NonNegative", "[MinStockLevel] >= 0");
        }
    }
}