using BikeService.Application.DTOs.Review;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces.Services
{
    public interface IReviewService
    {
        Task<Result<List<ReviewDto>>> GetRecentAsync(int count = 10);
        Task<Result<ReviewDto?>> GetByTicketIdAsync(int ticketId);
        Task<Result<int>> CreateAsync(ReviewFormDto dto);
    }
}
