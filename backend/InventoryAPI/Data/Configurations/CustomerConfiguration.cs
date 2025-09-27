using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryAPI.Models.Entities;

namespace InventoryAPI.Data.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            // Primary Key
            builder.HasKey(c => c.Id);

            // Properties
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Email)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(c => c.Phone)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(c => c.Address)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(c => c.LoyaltyPoints)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(c => c.CreditBalance)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            builder.Property(c => c.LastPurchaseDate)
                .IsRequired(false);

            // Indexes
            builder.HasIndex(c => c.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL AND [IsActive] = 1")
                .HasDatabaseName("IX_Customers_Email_Active");

            builder.HasIndex(c => c.Phone)
                .HasFilter("[Phone] IS NOT NULL")
                .HasDatabaseName("IX_Customers_Phone");

            builder.HasIndex(c => c.Name)
                .HasDatabaseName("IX_Customers_Name");

            builder.HasIndex(c => c.IsActive)
                .HasDatabaseName("IX_Customers_IsActive");

            builder.HasIndex(c => c.LastPurchaseDate)
                .HasDatabaseName("IX_Customers_LastPurchaseDate");

            builder.HasIndex(c => c.LoyaltyPoints)
                .HasFilter("[LoyaltyPoints] > 0")
                .HasDatabaseName("IX_Customers_LoyaltyPoints");

            builder.HasIndex(c => c.CreditBalance)
                .HasFilter("[CreditBalance] > 0")
                .HasDatabaseName("IX_Customers_CreditBalance");

            builder.HasIndex(c => c.CreatedAt)
                .HasDatabaseName("IX_Customers_CreatedAt");

            // Relationships
            builder.HasMany(c => c.Sales)
                .WithOne(s => s.Customer)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Sales_Customers");

            // Table Configuration
            builder.ToTable("Customers", schema: "dbo");

            // Soft Delete Filter
            builder.HasQueryFilter(c => c.IsActive);

            // Check Constraints
            builder.HasCheckConstraint("CK_Customers_Name_NotEmpty", "LEN([Name]) > 0");
            builder.HasCheckConstraint("CK_Customers_Email_Format", "[Email] IS NULL OR [Email] LIKE '%@%.%'");
            builder.HasCheckConstraint("CK_Customers_LoyaltyPoints_NonNegative", "[LoyaltyPoints] >= 0");
            builder.HasCheckConstraint("CK_Customers_CreditBalance_NonNegative", "[CreditBalance] >= 0");
        }
    }
}
