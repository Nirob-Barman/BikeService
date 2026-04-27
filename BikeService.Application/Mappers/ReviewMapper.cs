using BikeService.Application.DTOs.Review;
using BikeService.Domain.Entities;

namespace BikeService.Application.Mappers
{
    public static class ReviewMapper
    {
        public static ReviewDto ToDto(Review r, string bikeSummary = "", string customerName = "") => new()
        {
            Id = r.Id,
            Rating = r.Rating,
            Comment = r.Comment,
            ServiceTicketId = r.ServiceTicketId,
            BikeSummary = bikeSummary,
            CustomerName = customerName,
            CreatedAt = r.CreatedAt
        };
    }
}
