using BikeService.Domain.Entities;
using BikeService.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BikeService.Infrastructure.Persistence.Configurations;

public class CustomerBikeConfiguration : IEntityTypeConfiguration<CustomerBike>
{
    public void Configure(EntityTypeBuilder<CustomerBike> builder)
    {
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
