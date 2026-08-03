using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Reviews.Commands.CreateReview;

public record CreateReviewCommand(
    int ServiceTicketId,
    int Rating,
    string? Comment) : IRequest<Result<int>>;
