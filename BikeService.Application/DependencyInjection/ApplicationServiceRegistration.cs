using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BikeService.Application.DependencyInjection;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Identity
        services.AddScoped<IUserService, UserService>();

        // Phase 1
        services.AddScoped<IServiceTypeService, ServiceTypeService>();
        services.AddScoped<IMechanicService, MechanicService>();
        services.AddScoped<IPromoCodeService, PromoCodeService>();

        // Phase 2
        services.AddScoped<IPartService, PartService>();
        services.AddScoped<IBulkImportService, BulkImportService>();

        // Phase 3
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICustomerBikeService, CustomerBikeService>();
        services.AddScoped<IAppointmentService, AppointmentService>();

        // Phase 4
        services.AddScoped<IServiceTicketService, ServiceTicketService>();

        // Phase 5
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();

        // Phase 6
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Phase 10
        services.AddScoped<IPaymentService, PaymentService>();

        // Phase 12
        services.AddScoped<IReviewService, ReviewService>();

        // Phase 15
        services.AddScoped<ITicketNoteService, TicketNoteService>();

        // Phase 17
        services.AddScoped<IReportService, ReportService>();

        // Phase 18
        services.AddScoped<ILeaveRequestService, LeaveRequestService>();

        // Phase 19
        services.AddScoped<IPayrollService, PayrollService>();

        return services;
    }
}
