using BikeService.Application.DTOs.Dashboard;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;

namespace BikeService.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DashboardDto>> GetDashboardAsync()
        {
            var now = DateTime.UtcNow;
            var startOfToday = now.Date;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var twelveMonthsAgo = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-11);

            // Revenue from paid invoices
            var paidInvoices = await _unitOfWork.Repository<Invoice>()
                .Where(i => i.Status == InvoiceStatus.Paid);

            var totalRevenue = paidInvoices.Sum(i => i.FinalAmount);
            var revenueThisMonth = paidInvoices
                .Where(i => i.CreatedAt >= startOfMonth)
                .Sum(i => i.FinalAmount);

            // Ticket stats
            var allTickets = await _unitOfWork.Repository<ServiceTicket>().GetAllAsync();
            var ticketList = allTickets.ToList();

            var ticketsToday = ticketList.Count(t => t.CreatedAt.Date == startOfToday);
            var pendingTickets = ticketList.Count(t => t.Status == ServiceTicketStatus.Pending);
            var activeTickets = ticketList.Count(t =>
                t.Status != ServiceTicketStatus.Delivered &&
                t.Status != ServiceTicketStatus.Cancelled);

            // Customer/Bike counts
            var allBikes = await _unitOfWork.Repository<CustomerBike>().GetAllAsync();
            var bikeList = allBikes.ToList();
            var totalBikes = bikeList.Count;
            var totalCustomers = bikeList.Select(b => b.CustomerId).Distinct().Count();

            // Top services
            var allItems = await _unitOfWork.Repository<ServiceTicketItem>()
                .GetAllWithIncludesAsync<ServiceTicketItem>(
                    i => i.ServiceTypeId.HasValue,
                    i => i,
                    i => i.ServiceType!);

            var topServices = allItems
                .GroupBy(i => new { i.ServiceTypeId, Name = i.ServiceType?.Name ?? "Unknown" })
                .Select(g => new ServiceStatDto
                {
                    ServiceName = g.Key.Name,
                    Count = g.Count(),
                    Revenue = g.Sum(i => i.Quantity * i.UnitPrice)
                })
                .OrderByDescending(s => s.Count)
                .Take(5)
                .ToList();

            // Top mechanics
            var deliveredTickets = await _unitOfWork.Repository<ServiceTicket>()
                .GetAllWithIncludesAsync<ServiceTicket>(
                    t => t.Status == ServiceTicketStatus.Delivered && t.MechanicId.HasValue,
                    t => t,
                    t => t.Mechanic!);

            var topMechanics = deliveredTickets
                .GroupBy(t => new { t.MechanicId, Name = t.Mechanic?.FullName ?? "Unknown" })
                .Select(g => new MechanicStatDto
                {
                    MechanicName = g.Key.Name,
                    TicketsCompleted = g.Count()
                })
                .OrderByDescending(m => m.TicketsCompleted)
                .Take(5)
                .ToList();

            // Monthly revenue — last 12 months
            var recentPaidInvoices = paidInvoices
                .Where(i => i.CreatedAt >= twelveMonthsAgo)
                .ToList();

            var monthlyRevenue = recentPaidInvoices
                .GroupBy(i => new { i.CreatedAt.Year, i.CreatedAt.Month })
                .Select(g => new MonthlyRevenueDto
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Revenue = g.Sum(i => i.FinalAmount)
                })
                .OrderBy(m => m.Month)
                .ToList();

            // Fill in missing months with zero
            var allMonths = Enumerable.Range(0, 12)
                .Select(offset => now.AddMonths(-11 + offset))
                .Select(d => new DateTime(d.Year, d.Month, 1))
                .ToList();

            var monthlyRevenueComplete = allMonths.Select(month =>
            {
                var label = month.ToString("MMM yyyy");
                var existing = monthlyRevenue.FirstOrDefault(m => m.Month == label);
                return existing ?? new MonthlyRevenueDto { Month = label, Revenue = 0 };
            }).ToList();

            var dto = new DashboardDto
            {
                TotalRevenue = totalRevenue,
                RevenueThisMonth = revenueThisMonth,
                TicketsToday = ticketsToday,
                PendingTickets = pendingTickets,
                ActiveTickets = activeTickets,
                TotalCustomers = totalCustomers,
                TotalBikes = totalBikes,
                TopServices = topServices,
                TopMechanics = topMechanics,
                MonthlyRevenue = monthlyRevenueComplete
            };

            return Result<DashboardDto>.Ok(dto);
        }
    }
}
