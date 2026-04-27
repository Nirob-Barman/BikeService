using BikeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BikeService.Infrastructure.Persistence.Configurations;

public class ServiceTicketConfiguration : IEntityTypeConfiguration<ServiceTicket>
{
    public void Configure(EntityTypeBuilder<ServiceTicket> builder)
    {
        builder.HasOne(t => t.Bike)
            .WithMany(b => b.ServiceTickets)
            .HasForeignKey(t => t.BikeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Mechanic)
            .WithMany(m => m.ServiceTickets)
            .HasForeignKey(t => t.MechanicId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Appointment)
            .WithMany(a => a.ServiceTickets)
            .HasForeignKey(t => t.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
