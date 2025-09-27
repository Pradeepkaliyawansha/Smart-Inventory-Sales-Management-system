using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryAPI.Models.Entities;

namespace InventoryAPI.Data.Configurations
{
    public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
    {
        public void Configure(EntityTypeBuilder<SaleItem> builder)
        {
            // Primary Key
            builder.HasKey(si => si.Id);

            // Properties
            builder.Property(si => si.SaleId)
                .IsRequired();

            builder.Property(si => si.ProductId)
                .IsRequired();

            builder.Property(si => si.Quantity)
                .IsRequired();

            builder.Property(si => si.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(si => si.DiscountPercentage)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(si => si.TotalPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Indexes
            builder.HasIndex(si => si.SaleId)
                .HasDatabaseName("IX_SaleItems_SaleId");

            builder.HasIndex(si => si.ProductId)
                .HasDatabaseName("IX_SaleItems_ProductId");

            builder.HasIndex(si => new { si.SaleId, si.ProductId })
                .IsUnique()
                .HasDatabaseName("IX_SaleItems_Sale_Product");

            // Relationships
            builder.HasOne(si => si.Sale)
                .WithMany(s => s.SaleItems)
                .HasForeignKey(si => si.SaleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SaleItems_Sales");

            builder.HasOne(si => si.Product)
                .WithMany(p => p.SaleItems)
                .HasForeignKey(si => si.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_SaleItems_Products");

            // Table Configuration
            builder.ToTable("SaleItems", schema: "dbo");

            // Check Constraints
            builder.HasCheckConstraint("CK_SaleItems_Quantity_Positive", "[Quantity] > 0");
            builder.HasCheckConstraint("CK_SaleItems_UnitPrice_Positive", "[UnitPrice] > 0");
            builder.HasCheckConstraint("CK_SaleItems_DiscountPercentage_Valid", "[DiscountPercentage] >= 0 AND [DiscountPercentage] <= 100");
            builder.HasCheckConstraint("CK_SaleItems_TotalPrice_NonNegative", "[TotalPrice] >= 0");
        }
    }
}