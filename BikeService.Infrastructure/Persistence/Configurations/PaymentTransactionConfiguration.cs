using BikeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BikeService.Infrastructure.Persistence.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.HasOne(t => t.Invoice)
            .WithMany(i => i.PaymentTransactions)
            .HasForeignKey(t => t.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Gateway)
            .WithMany(g => g.Transactions)
            .HasForeignKey(t => t.GatewayId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.Amount).HasPrecision(18, 2);
    }
}
