using BikeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BikeService.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasOne(i => i.ServiceTicket)
            .WithMany(t => t.Invoices)
            .HasForeignKey(i => i.ServiceTicketId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.PromoCode)
            .WithMany(p => p.Invoices)
            .HasForeignKey(i => i.PromoCodeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(i => i.TotalAmount).HasPrecision(18, 2);
        builder.Property(i => i.TaxAmount).HasPrecision(18, 2);
        builder.Property(i => i.DiscountAmount).HasPrecision(18, 2);
        builder.Property(i => i.FinalAmount).HasPrecision(18, 2);
    }
}
