using BikeService.Application.DTOs.Email;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.FileStorage;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Infrastructure.Identity;
using BikeService.Infrastructure.Payments;
using BikeService.Infrastructure.Persistence;
using BikeService.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BikeService.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IUserManager, IdentityUserManager>();
        services.AddScoped<ISignInManager, IdentitySignInManager>();
        services.AddScoped<IRoleManager, RoleManager>();

        services.AddScoped<IUserContextService, UserContextService>();

        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, EmailService>();

        services.AddDataProtection();
        services.AddScoped<IConfigEncryptor, ConfigEncryptor>();
        services.AddScoped<IFileStorage, LocalFileStorage>();

        services.AddScoped<IPdfService, InvoicePdfService>();

        // Payment processors
        services.AddScoped<IPaymentProcessor, MockPaymentProcessor>();
        services.AddScoped<IPaymentProcessorFactory, PaymentProcessorFactory>();

        return services;
    }
}
