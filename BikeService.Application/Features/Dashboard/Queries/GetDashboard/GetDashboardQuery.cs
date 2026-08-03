using BikeService.Application.DTOs.Dashboard;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Dashboard.Queries.GetDashboard;

public record GetDashboardQuery : IRequest<Result<DashboardDto>>;
