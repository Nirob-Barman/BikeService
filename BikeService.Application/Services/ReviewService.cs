using BikeService.Application.DTOs.Review;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;

namespace BikeService.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;

        public ReviewService(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
        }

        public async Task<Result<List<ReviewDto>>> GetRecentAsync(int count = 10)
        {
            var reviews = await _unitOfWork.Repository<Review>()
                .GetAllWithIncludesAsync<Review>(r => r, r => r.ServiceTicket);

            var result = new List<ReviewDto>();
            foreach (var r in reviews.OrderByDescending(r => r.CreatedAt).Take(count))
            {
                CustomerBike? bike = null;
                if (r.ServiceTicket != null)
                    bike = await _unitOfWork.Repository<CustomerBike>().GetByIdAsync(r.ServiceTicket.BikeId);

                // Customer name resolved via IUserService to keep Application layer clean
                var customerName = "Verified Customer";

                var bikeSummary = bike != null ? $"{bike.Year} {bike.Make} {bike.Model}" : "Bike Service";
                result.Add(ReviewMapper.ToDto(r, bikeSummary, customerName));
            }

            return Result<List<ReviewDto>>.Ok(result);
        }

        public async Task<Result<ReviewDto?>> GetByTicketIdAsync(int ticketId)
        {
            var review = await _unitOfWork.Repository<Review>()
                .FirstOrDefaultAsync(r => r.ServiceTicketId == ticketId);

            if (review == null)
                return Result<ReviewDto?>.Ok(null);

            return Result<ReviewDto?>.Ok(ReviewMapper.ToDto(review));
        }

        public async Task<Result<int>> CreateAsync(ReviewFormDto dto)
        {
            var userId = _userContextService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Result<int>.Fail("User not authenticated.");

            if (dto.Rating < 1 || dto.Rating > 5)
                return Result<int>.FailField("Rating", "Rating must be between 1 and 5.");

            // Load ticket with bike for ownership check
            var tickets = await _unitOfWork.Repository<ServiceTicket>()
                .GetAllWithIncludesAsync<ServiceTicket>(
                    t => t.Id == dto.ServiceTicketId,
                    t => t,
                    t => t.Bike);

            var ticket = tickets.FirstOrDefault();
            if (ticket == null)
                return Result<int>.Fail("Service ticket not found.");

            if (ticket.Bike?.CustomerId != userId)
                return Result<int>.Fail("Access denied.");

            if (ticket.Status != ServiceTicketStatus.Delivered)
                return Result<int>.Fail("You can only review a completed service.");

            var existing = await _unitOfWork.Repository<Review>()
                .AnyAsync(r => r.ServiceTicketId == dto.ServiceTicketId && r.CustomerId == userId);

            if (existing)
                return Result<int>.Fail("You have already submitted a review for this service.");

            var review = new Review
            {
                ServiceTicketId = dto.ServiceTicketId,
                CustomerId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _unitOfWork.Repository<Review>().AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            return Result<int>.Ok(review.Id, "Review submitted successfully.");
        }
    }
}
