using BikeService.Domain.Constants;
using BikeService.Domain.Entities;
using BikeService.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BikeService.Infrastructure.Persistence;

public static class AppDbContextSeed
{
    public static async Task SeedAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        AppDbContext db)
    {
        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager);
        await SeedMechanicUsersAsync(userManager, db);
        await SeedCustomerUsersAsync(userManager, db);
        await SeedMechanicsAsync(db, userManager);
        await SeedServiceTypesAsync(db);
        await SeedPartsAsync(db);
        await SeedPromoCodesAsync(db);
    }

    // ─── Roles ───────────────────────────────────────────────────────────────

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = [AppRoles.Admin, AppRoles.Mechanic, AppRoles.Customer];
        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
    }

    // ─── Admin user ──────────────────────────────────────────────────────────

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        const string email = "admin@bikeservice.com";
        if (await userManager.FindByEmailAsync(email) is not null) return;

        var user = new ApplicationUser
        {
            UserName = email, Email = email,
            FirstName = "System", LastName = "Admin",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, "Admin@123");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, AppRoles.Admin);
    }

    // ─── Mechanic accounts ───────────────────────────────────────────────────

    private static async Task SeedMechanicUsersAsync(
        UserManager<ApplicationUser> userManager, AppDbContext db)
    {
        var mechanics = new[]
        {
            ("james.wright@bikeservice.com",  "James",  "Wright"),
            ("sarah.malik@bikeservice.com",   "Sarah",  "Malik"),
            ("tom.nguyen@bikeservice.com",    "Tom",    "Nguyen"),
        };

        foreach (var (email, first, last) in mechanics)
        {
            if (await userManager.FindByEmailAsync(email) is not null) continue;

            var user = new ApplicationUser
            {
                UserName = email, Email = email,
                FirstName = first, LastName = last,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, "Mechanic@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, AppRoles.Mechanic);
        }
    }

    // ─── Customer accounts ───────────────────────────────────────────────────

    private static async Task SeedCustomerUsersAsync(
        UserManager<ApplicationUser> userManager, AppDbContext db)
    {
        var customers = new[]
        {
            ("alice.johnson@example.com", "Alice",   "Johnson"),
            ("bob.smith@example.com",     "Bob",     "Smith"),
            ("carol.white@example.com",   "Carol",   "White"),
            ("david.brown@example.com",   "David",   "Brown"),
            ("eva.green@example.com",     "Eva",     "Green"),
        };

        foreach (var (email, first, last) in customers)
        {
            if (await userManager.FindByEmailAsync(email) is not null) continue;

            var user = new ApplicationUser
            {
                UserName = email, Email = email,
                FirstName = first, LastName = last,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, "Customer@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, AppRoles.Customer);
        }
    }

    // ─── Mechanics (staff records) ───────────────────────────────────────────

    private static async Task SeedMechanicsAsync(
        AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        if (await db.Mechanics.AnyAsync()) return;

        var definitions = new[]
        {
            ("james.wright@bikeservice.com", "James Wright",  "Engine & Transmission"),
            ("sarah.malik@bikeservice.com",  "Sarah Malik",   "Electrical Systems"),
            ("tom.nguyen@bikeservice.com",   "Tom Nguyen",    "Suspension & Brakes"),
        };

        var mechanics = new List<Mechanic>();
        foreach (var (email, fullName, specialty) in definitions)
        {
            var appUser = await userManager.FindByEmailAsync(email);
            mechanics.Add(new Mechanic
            {
                FullName    = fullName,
                Specialty   = specialty,
                IsAvailable = true,
                UserId      = appUser?.Id,
                CreatedAt   = DateTime.UtcNow,
                CreatedBy   = "seed"
            });
        }

        await db.Mechanics.AddRangeAsync(mechanics);
        await db.SaveChangesAsync();
    }

    // ─── Service Types ───────────────────────────────────────────────────────

    private static async Task SeedServiceTypesAsync(AppDbContext db)
    {
        if (await db.ServiceTypes.AnyAsync()) return;

        var serviceTypes = new List<ServiceType>
        {
            new() { Name = "Full Service",            Description = "Complete inspection, oil change, filter replacement, and safety check.",       BasePrice = 79.99m,  EstimatedHours = 3.0, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Oil Change",              Description = "Engine oil and oil filter replacement with multi-point inspection.",            BasePrice = 29.99m,  EstimatedHours = 0.5, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Tire Replacement",        Description = "Front or rear tire removal, new tire fitting, and balancing.",                  BasePrice = 39.99m,  EstimatedHours = 1.0, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Brake Service",           Description = "Brake pad/disc inspection, replacement, and fluid top-up.",                    BasePrice = 49.99m,  EstimatedHours = 1.5, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Chain & Sprocket Service",Description = "Drive chain cleaning, lubrication or replacement, and sprocket inspection.",   BasePrice = 34.99m,  EstimatedHours = 1.0, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Electrical Diagnostics",  Description = "Full electrical system scan, fault code reading, and wiring inspection.",      BasePrice = 44.99m,  EstimatedHours = 1.5, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Suspension Service",      Description = "Fork seal inspection/replacement, rear shock check, and alignment.",           BasePrice = 89.99m,  EstimatedHours = 2.5, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Battery Replacement",     Description = "Battery test, removal, and new battery installation.",                         BasePrice = 24.99m,  EstimatedHours = 0.5, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Carburetor Cleaning",     Description = "Carburettor disassembly, ultrasonic cleaning, and re-jetting if required.",   BasePrice = 54.99m,  EstimatedHours = 2.0, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Tune-Up",                 Description = "Spark plug replacement, air filter check, idle adjustment, and throttle sync.",BasePrice = 59.99m,  EstimatedHours = 2.0, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
        };

        await db.ServiceTypes.AddRangeAsync(serviceTypes);
        await db.SaveChangesAsync();
    }

    // ─── Parts inventory ─────────────────────────────────────────────────────

    private static async Task SeedPartsAsync(AppDbContext db)
    {
        if (await db.Parts.AnyAsync()) return;

        var parts = new List<Part>
        {
            new() { Name = "Engine Oil Filter",         SKU = "FILT-001", UnitPrice = 8.50m,   StockQuantity = 50, LowStockThreshold = 10, CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Air Filter",                SKU = "FILT-002", UnitPrice = 12.00m,  StockQuantity = 40, LowStockThreshold = 8,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Spark Plug (Standard)",     SKU = "PLUG-001", UnitPrice = 4.75m,   StockQuantity = 80, LowStockThreshold = 15, CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Spark Plug (Iridium)",      SKU = "PLUG-002", UnitPrice = 11.50m,  StockQuantity = 30, LowStockThreshold = 8,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Brake Pads (Front)",        SKU = "BRAK-001", UnitPrice = 24.00m,  StockQuantity = 25, LowStockThreshold = 5,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Brake Pads (Rear)",         SKU = "BRAK-002", UnitPrice = 20.00m,  StockQuantity = 25, LowStockThreshold = 5,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Brake Disc (Front)",        SKU = "BRAK-003", UnitPrice = 55.00m,  StockQuantity = 12, LowStockThreshold = 3,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Drive Chain (520)",         SKU = "CHAN-001", UnitPrice = 32.00m,  StockQuantity = 20, LowStockThreshold = 4,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Drive Sprocket (Front)",    SKU = "SPKT-001", UnitPrice = 18.00m,  StockQuantity = 15, LowStockThreshold = 3,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Drive Sprocket (Rear)",     SKU = "SPKT-002", UnitPrice = 28.00m,  StockQuantity = 15, LowStockThreshold = 3,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Engine Oil 10W-40 (1L)",    SKU = "OIL-001",  UnitPrice = 9.00m,   StockQuantity = 60, LowStockThreshold = 12, CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Coolant (1L)",              SKU = "COOL-001", UnitPrice = 7.50m,   StockQuantity = 30, LowStockThreshold = 8,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Tire (Front 110/70-17)",    SKU = "TIRE-001", UnitPrice = 68.00m,  StockQuantity = 8,  LowStockThreshold = 2,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Tire (Rear 140/70-17)",     SKU = "TIRE-002", UnitPrice = 82.00m,  StockQuantity = 8,  LowStockThreshold = 2,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Fork Seal Set",             SKU = "FORK-001", UnitPrice = 22.00m,  StockQuantity = 10, LowStockThreshold = 3,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Clutch Cable",              SKU = "CABL-001", UnitPrice = 14.00m,  StockQuantity = 18, LowStockThreshold = 4,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Throttle Cable",            SKU = "CABL-002", UnitPrice = 13.00m,  StockQuantity = 18, LowStockThreshold = 4,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Battery 12V 9Ah",           SKU = "BATT-001", UnitPrice = 45.00m,  StockQuantity = 10, LowStockThreshold = 2,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Headlight Bulb (H4)",       SKU = "BULB-001", UnitPrice = 6.00m,   StockQuantity = 35, LowStockThreshold = 8,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Name = "Fuel Filter",               SKU = "FILT-003", UnitPrice = 10.50m,  StockQuantity = 22, LowStockThreshold = 5,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
        };

        await db.Parts.AddRangeAsync(parts);
        await db.SaveChangesAsync();
    }

    // ─── Promo Codes ─────────────────────────────────────────────────────────

    private static async Task SeedPromoCodesAsync(AppDbContext db)
    {
        if (await db.PromoCodes.AnyAsync()) return;

        var codes = new List<PromoCode>
        {
            new() { Code = "WELCOME10", DiscountPercent = 10m, MaxUsages = 100, UsageCount = 0, ExpiresAt = DateTime.UtcNow.AddMonths(6),  IsActive = true,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Code = "SUMMER20",  DiscountPercent = 20m, MaxUsages = 50,  UsageCount = 0, ExpiresAt = DateTime.UtcNow.AddMonths(3),  IsActive = true,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Code = "LOYAL15",   DiscountPercent = 15m, MaxUsages = 200, UsageCount = 0, ExpiresAt = DateTime.UtcNow.AddYears(1),   IsActive = true,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Code = "FLASH30",   DiscountPercent = 30m, MaxUsages = 20,  UsageCount = 0, ExpiresAt = DateTime.UtcNow.AddDays(7),    IsActive = true,  CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
            new() { Code = "VIP25",     DiscountPercent = 25m, MaxUsages = 30,  UsageCount = 0, ExpiresAt = null,                          IsActive = false, CreatedAt = DateTime.UtcNow, CreatedBy = "seed" },
        };

        await db.PromoCodes.AddRangeAsync(codes);
        await db.SaveChangesAsync();
    }
}
