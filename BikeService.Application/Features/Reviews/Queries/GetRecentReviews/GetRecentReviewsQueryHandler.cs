using BikeService.Application.DTOs.Review;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Reviews.Queries.GetRecentReviews;

public class GetRecentReviewsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetRecentReviewsQuery, Result<List<ReviewDto>>>
{
    public async Task<Result<List<ReviewDto>>> Handle(GetRecentReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await unitOfWork.Repository<Review>()
            .GetAllWithIncludesAsync<Review>(r => r, r => r.ServiceTicket);

        var result = new List<ReviewDto>();
        foreach (var r in reviews.OrderByDescending(r => r.CreatedAt).Take(request.Count))
        {
            CustomerBike? bike = null;
            if (r.ServiceTicket != null)
                bike = await unitOfWork.Repository<CustomerBike>().GetByIdAsync(r.ServiceTicket.BikeId);

            // Customer name resolved via IUserService to keep Application layer clean
            var customerName = "Verified Customer";

            var bikeSummary = bike != null ? $"{bike.Year} {bike.Make} {bike.Model}" : "Bike Service";
            result.Add(ReviewMapper.ToDto(r, bikeSummary, customerName));
        }

        return Result<List<ReviewDto>>.Ok(result);
    }
}
