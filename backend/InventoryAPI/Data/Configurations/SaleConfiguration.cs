using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryAPI.Models.Entities;

namespace InventoryAPI.Data.Configurations
{
    public class SaleConfiguration : IEntityTypeConfiguration<Sale>
    {
        public void Configure(EntityTypeBuilder<Sale> builder)
        {
            // Primary Key
            builder.HasKey(s => s.Id);

            // Properties
            builder.Property(s => s.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.CustomerId)
                .IsRequired();

            builder.Property(s => s.UserId)
                .IsRequired();

            builder.Property(s => s.SaleDate)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            builder.Property(s => s.SubTotal)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(s => s.DiscountAmount)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(s => s.TaxAmount)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(s => s.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(s => s.PaidAmount)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(s => s.PaymentMethod)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(s => s.Notes)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(s => s.IsCompleted)
                .HasDefaultValue(true)
                .IsRequired();

            // Indexes
            builder.HasIndex(s => s.InvoiceNumber)
                .IsUnique()
                .HasDatabaseName("IX_Sales_InvoiceNumber");

            builder.HasIndex(s => s.SaleDate)
                .HasDatabaseName("IX_Sales_SaleDate");

            builder.HasIndex(s => new { s.CustomerId, s.SaleDate })
                .HasDatabaseName("IX_Sales_Customer_Date");

            builder.HasIndex(s => new { s.UserId, s.SaleDate })
                .HasDatabaseName("IX_Sales_User_Date");

            builder.HasIndex(s => s.PaymentMethod)
                .HasDatabaseName("IX_Sales_PaymentMethod");

            builder.HasIndex(s => s.IsCompleted)
                .HasDatabaseName("IX_Sales_IsCompleted");

            builder.HasIndex(s => s.TotalAmount)
                .HasDatabaseName("IX_Sales_TotalAmount");

            // Relationships
            builder.HasOne(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Sales_Customers");

            builder.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Sales_Users");

            builder.HasMany(s => s.SaleItems)
                .WithOne(si => si.Sale)
                .HasForeignKey(si => si.SaleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SaleItems_Sales");

            // Table Configuration
            builder.ToTable("Sales", schema: "dbo");

            // Check Constraints
            builder.HasCheckConstraint("CK_Sales_InvoiceNumber_NotEmpty", "LEN([InvoiceNumber]) > 0");
            builder.HasCheckConstraint("CK_Sales_SubTotal_NonNegative", "[SubTotal] >= 0");
            builder.HasCheckConstraint("CK_Sales_DiscountAmount_NonNegative", "[DiscountAmount] >= 0");
            builder.HasCheckConstraint("CK_Sales_TaxAmount_NonNegative", "[TaxAmount] >= 0");
            builder.HasCheckConstraint("CK_Sales_TotalAmount_Positive", "[TotalAmount] > 0");
            builder.HasCheckConstraint("CK_Sales_PaidAmount_NonNegative", "[PaidAmount] >= 0");
        }
    }
}