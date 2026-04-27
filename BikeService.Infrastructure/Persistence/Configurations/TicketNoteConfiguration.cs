using BikeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BikeService.Infrastructure.Persistence.Configurations;

public class TicketNoteConfiguration : IEntityTypeConfiguration<TicketNote>
{
    public void Configure(EntityTypeBuilder<TicketNote> builder)
    {
        builder.HasOne(n => n.ServiceTicket)
               .WithMany(t => t.Notes)
               .HasForeignKey(n => n.ServiceTicketId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(n => n.AuthorId).HasMaxLength(450).IsRequired();
        builder.Property(n => n.AuthorName).HasMaxLength(100).IsRequired();
        builder.Property(n => n.AuthorRole).HasMaxLength(20).IsRequired();
        builder.Property(n => n.Message).HasMaxLength(1000).IsRequired();
    }
}
