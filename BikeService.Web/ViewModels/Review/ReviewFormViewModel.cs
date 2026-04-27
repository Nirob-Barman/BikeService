using System.ComponentModel.DataAnnotations;

namespace BikeService.Web.ViewModels.Review
{
    public class ReviewFormViewModel
    {
        [Required]
        public int ServiceTicketId { get; set; }
        public string BikeSummary { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a rating.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string? Comment { get; set; }
    }
}
