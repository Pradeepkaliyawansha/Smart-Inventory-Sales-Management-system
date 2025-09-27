using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryAPI.Models.Entities;

namespace InventoryAPI.Data.Configurations
{
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            // Primary Key
            builder.HasKey(s => s.Id);

            // Properties
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.ContactPerson)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(s => s.Email)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(s => s.Phone)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(s => s.Address)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(s => s.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(s => s.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            // Indexes
            builder.HasIndex(s => s.Name)
                .IsUnique()
                .HasFilter("[IsActive] = 1")
                .HasDatabaseName("IX_Suppliers_Name_Active");

            builder.HasIndex(s => s.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL AND [IsActive] = 1")
                .HasDatabaseName("IX_Suppliers_Email_Active");

            builder.HasIndex(s => s.Phone)
                .HasFilter("[Phone] IS NOT NULL")
                .HasDatabaseName("IX_Suppliers_Phone");

            builder.HasIndex(s => s.IsActive)
                .HasDatabaseName("IX_Suppliers_IsActive");

            builder.HasIndex(s => s.CreatedAt)
                .HasDatabaseName("IX_Suppliers_CreatedAt");

            // Relationships
            builder.HasMany(s => s.Products)
                .WithOne(p => p.Supplier)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Products_Suppliers");

            // Table Configuration
            builder.ToTable("Suppliers", schema: "dbo");

            // Soft Delete Filter
            builder.HasQueryFilter(s => s.IsActive);

            // Check Constraints
            builder.HasCheckConstraint("CK_Suppliers_Name_NotEmpty", "LEN([Name]) > 0");
            builder.HasCheckConstraint("CK_Suppliers_Email_Format", "[Email] IS NULL OR [Email] LIKE '%@%.%'");
        }
    }
}
