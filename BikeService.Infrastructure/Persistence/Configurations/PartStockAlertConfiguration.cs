using BikeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BikeService.Infrastructure.Persistence.Configurations;

public class PartStockAlertConfiguration : IEntityTypeConfiguration<PartStockAlert>
{
    public void Configure(EntityTypeBuilder<PartStockAlert> builder)
    {
        builder.HasOne(a => a.Part)
            .WithMany(p => p.StockAlerts)
            .HasForeignKey(a => a.PartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
