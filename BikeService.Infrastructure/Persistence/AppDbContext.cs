using BikeService.Domain.Entities;
using BikeService.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BikeService.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<CustomerBike> CustomerBikes => Set<CustomerBike>();
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<Mechanic> Mechanics => Set<Mechanic>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<ServiceTicket> ServiceTickets => Set<ServiceTicket>();
    public DbSet<ServiceTicketItem> ServiceTicketItems => Set<ServiceTicketItem>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<PartStockAlert> PartStockAlerts => Set<PartStockAlert>();
    public DbSet<AppNotification> AppNotifications => Set<AppNotification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<PaymentGateway> PaymentGateways => Set<PaymentGateway>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<IntegrationSetting> IntegrationSettings => Set<IntegrationSetting>();
    public DbSet<TicketNote> TicketNotes => Set<TicketNote>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
