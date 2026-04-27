using BikeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BikeService.Infrastructure.Persistence.Configurations
{
    public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
    {
        public void Configure(EntityTypeBuilder<LeaveRequest> builder)
        {
            builder.HasOne(l => l.Mechanic)
                   .WithMany(m => m.LeaveRequests)
                   .HasForeignKey(l => l.MechanicId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(l => l.Reason).HasMaxLength(500);
            builder.Property(l => l.AdminNotes).HasMaxLength(500);
        }
    }
}
