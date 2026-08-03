using BikeService.Application.DTOs.Review;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Reviews.Queries.GetReviewByTicketId;

public class GetReviewByTicketIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetReviewByTicketIdQuery, Result<ReviewDto?>>
{
    public async Task<Result<ReviewDto?>> Handle(GetReviewByTicketIdQuery request, CancellationToken cancellationToken)
    {
        var review = await unitOfWork.Repository<Review>()
            .FirstOrDefaultAsync(r => r.ServiceTicketId == request.TicketId);

        if (review == null)
            return Result<ReviewDto?>.Ok(null);

        return Result<ReviewDto?>.Ok(ReviewMapper.ToDto(review));
    }
}
