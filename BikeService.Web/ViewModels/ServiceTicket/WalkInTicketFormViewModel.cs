using System.ComponentModel.DataAnnotations;

namespace BikeService.Web.ViewModels.ServiceTicket
{
    public class WalkInTicketFormViewModel
    {
        [Required(ErrorMessage = "Please select a customer bike.")]
        [Display(Name = "Customer Bike")]
        public int BikeId { get; set; }

        [Display(Name = "Assign Mechanic")]
        public int? MechanicId { get; set; }

        [Display(Name = "Diagnosis Notes")]
        [MaxLength(1000)]
        public string? DiagnosisNotes { get; set; }

        [Display(Name = "Estimated Completion")]
        public DateTime? EstimatedCompletionDate { get; set; }
    }
}
