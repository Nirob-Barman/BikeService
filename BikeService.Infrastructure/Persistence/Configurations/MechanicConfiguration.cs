using BikeService.Domain.Entities;
using BikeService.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BikeService.Infrastructure.Persistence.Configurations;

public class MechanicConfiguration : IEntityTypeConfiguration<Mechanic>
{
    public void Configure(EntityTypeBuilder<Mechanic> builder)
    {
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
