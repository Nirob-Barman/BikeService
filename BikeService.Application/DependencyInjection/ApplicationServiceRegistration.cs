using BikeService.Application.Common.Behaviors;
using BikeService.Application.Interfaces;
using BikeService.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BikeService.Application.DependencyInjection;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var applicationAssembly = typeof(ApplicationServiceRegistration).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        // Pipeline behaviors run in registration order (outermost first)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
