using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Reviews.Commands.CreateReview;

public class CreateReviewCommandHandler(
    IUnitOfWork unitOfWork,
    IUserContextService userContextService) : IRequestHandler<CreateReviewCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = userContextService.UserId;
        if (string.IsNullOrEmpty(userId))
            return Result<int>.Fail("User not authenticated.");

        // Load ticket with bike for ownership check
        var tickets = await unitOfWork.Repository<ServiceTicket>()
            .GetAllWithIncludesAsync<ServiceTicket>(
                t => t.Id == request.ServiceTicketId,
                t => t,
                t => t.Bike);

        var ticket = tickets.FirstOrDefault();
        if (ticket == null)
            return Result<int>.Fail("Service ticket not found.");

        if (ticket.Bike?.CustomerId != userId)
            return Result<int>.Fail("Access denied.");

        if (ticket.Status != ServiceTicketStatus.Delivered)
            return Result<int>.Fail("You can only review a completed service.");

        var existing = await unitOfWork.Repository<Review>()
            .AnyAsync(r => r.ServiceTicketId == request.ServiceTicketId && r.CustomerId == userId);

        if (existing)
            return Result<int>.Fail("You have already submitted a review for this service.");

        var review = new Review
        {
            ServiceTicketId = request.ServiceTicketId,
            CustomerId = userId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await unitOfWork.Repository<Review>().AddAsync(review);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Ok(review.Id, "Review submitted successfully.");
    }
}
