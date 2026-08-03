using BikeService.Application.DTOs.Review;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Reviews.Queries.GetReviewByTicketId;

public record GetReviewByTicketIdQuery(int TicketId) : IRequest<Result<ReviewDto?>>;
