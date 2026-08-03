using BikeService.Application.DTOs.Review;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Reviews.Queries.GetRecentReviews;

public record GetRecentReviewsQuery(int Count = 10) : IRequest<Result<List<ReviewDto>>>;
