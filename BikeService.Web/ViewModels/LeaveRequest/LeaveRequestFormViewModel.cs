using BikeService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BikeService.Web.ViewModels.LeaveRequest
{
    public class LeaveRequestFormViewModel
    {
        [Required(ErrorMessage = "Start date is required.")]
        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "End date is required.")]
        [Display(Name = "To Date")]
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Leave type is required.")]
        [Display(Name = "Leave Type")]
        public LeaveType Type { get; set; }

        [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        [Display(Name = "Reason (optional)")]
        public string? Reason { get; set; }
    }
}
