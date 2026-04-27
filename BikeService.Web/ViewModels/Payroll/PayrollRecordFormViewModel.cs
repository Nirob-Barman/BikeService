using System.ComponentModel.DataAnnotations;

namespace BikeService.Web.ViewModels.Payroll
{
    public class PayrollRecordFormViewModel
    {
        [Required]
        [Display(Name = "Mechanic")]
        public int MechanicId { get; set; }

        [Required]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12.")]
        public int Month { get; set; }

        [Required]
        [Range(2000, 2100, ErrorMessage = "Please enter a valid year.")]
        public int Year { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Base salary cannot be negative.")]
        [Display(Name = "Base Salary")]
        public decimal BaseSalary { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Bonus cannot be negative.")]
        public decimal Bonus { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Deductions cannot be negative.")]
        public decimal Deductions { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
