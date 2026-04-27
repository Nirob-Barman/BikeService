using BikeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BikeService.Infrastructure.Persistence.Configurations
{
    public class PayrollRecordConfiguration : IEntityTypeConfiguration<PayrollRecord>
    {
        public void Configure(EntityTypeBuilder<PayrollRecord> builder)
        {
            builder.HasOne(p => p.Mechanic)
                   .WithMany()
                   .HasForeignKey(p => p.MechanicId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.BaseSalary).HasPrecision(18, 2);
            builder.Property(p => p.Bonus).HasPrecision(18, 2);
            builder.Property(p => p.Deductions).HasPrecision(18, 2);
            builder.Property(p => p.Notes).HasMaxLength(500);

            // Ignore computed property — not stored in DB
            builder.Ignore(p => p.NetPay);

            builder.HasIndex(p => new { p.MechanicId, p.Year, p.Month }).IsUnique();
        }
    }
}
