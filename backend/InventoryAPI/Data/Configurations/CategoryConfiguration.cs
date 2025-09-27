using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryAPI.Models.Entities;

namespace InventoryAPI.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            // Primary Key
            builder.HasKey(c => c.Id);

            // Properties
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            // Indexes
            builder.HasIndex(c => c.Name)
                .IsUnique()
                .HasFilter("[IsActive] = 1")
                .HasDatabaseName("IX_Categories_Name_Active");

            builder.HasIndex(c => c.IsActive)
                .HasDatabaseName("IX_Categories_IsActive");

            builder.HasIndex(c => c.CreatedAt)
                .HasDatabaseName("IX_Categories_CreatedAt");

            // Relationships
            builder.HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Products_Categories");

            // Table Configuration
            builder.ToTable("Categories", schema: "dbo");

            // Soft Delete Filter
            builder.HasQueryFilter(c => c.IsActive);

            // Check Constraints
            builder.HasCheckConstraint("CK_Categories_Name_NotEmpty", "LEN([Name]) > 0");
        }
    }
}