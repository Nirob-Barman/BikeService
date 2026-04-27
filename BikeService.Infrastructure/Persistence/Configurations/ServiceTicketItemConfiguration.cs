using BikeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BikeService.Infrastructure.Persistence.Configurations;

public class ServiceTicketItemConfiguration : IEntityTypeConfiguration<ServiceTicketItem>
{
    public void Configure(EntityTypeBuilder<ServiceTicketItem> builder)
    {
        builder.HasOne(i => i.ServiceTicket)
            .WithMany(t => t.Items)
            .HasForeignKey(i => i.ServiceTicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.ServiceType)
            .WithMany(s => s.ServiceTicketItems)
            .HasForeignKey(i => i.ServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Part)
            .WithMany(p => p.ServiceTicketItems)
            .HasForeignKey(i => i.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
    }
}
