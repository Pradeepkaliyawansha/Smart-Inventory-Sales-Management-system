using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryAPI.Models.Entities;

namespace InventoryAPI.Data.Configurations
{
    public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
    {
        public void Configure(EntityTypeBuilder<StockMovement> builder)
        {
            // Primary Key
            builder.HasKey(sm => sm.Id);

            // Properties
            builder.Property(sm => sm.ProductId)
                .IsRequired();

            builder.Property(sm => sm.MovementType)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(sm => sm.Quantity)
                .IsRequired();

            builder.Property(sm => sm.Reference)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(sm => sm.Notes)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(sm => sm.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            builder.Property(sm => sm.CreatedBy)
                .IsRequired();

            // Indexes
            builder.HasIndex(sm => sm.ProductId)
                .HasDatabaseName("IX_StockMovements_ProductId");

            builder.HasIndex(sm => sm.MovementType)
                .HasDatabaseName("IX_StockMovements_MovementType");

            builder.HasIndex(sm => sm.CreatedAt)
                .HasDatabaseName("IX_StockMovements_CreatedAt");

            builder.HasIndex(sm => sm.CreatedBy)
                .HasDatabaseName("IX_StockMovements_CreatedBy");

            builder.HasIndex(sm => new { sm.ProductId, sm.CreatedAt })
                .HasDatabaseName("IX_StockMovements_Product_Date");

            builder.HasIndex(sm => sm.Reference)
                .HasFilter("[Reference] IS NOT NULL")
                .HasDatabaseName("IX_StockMovements_Reference");

            // Relationships
            builder.HasOne(sm => sm.Product)
                .WithMany(p => p.StockMovements)
                .HasForeignKey(sm => sm.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_StockMovements_Products");

            builder.HasOne(sm => sm.CreatedByUser)
                .WithMany()
                .HasForeignKey(sm => sm.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_StockMovements_Users");

            // Table Configuration
            builder.ToTable("StockMovements", schema: "dbo");

            // Check Constraints
            builder.HasCheckConstraint("CK_StockMovements_Quantity_Positive", "[Quantity] > 0");
        }
    }
}