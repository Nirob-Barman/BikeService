using System.ComponentModel.DataAnnotations;
using BikeService.Application.DTOs.Payment;

namespace BikeService.Web.ViewModels.Payment
{
    public class CheckoutViewModel
    {
        public CheckoutInfoDto Info { get; set; } = new();

        [Required(ErrorMessage = "Please select a payment method.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a payment method.")]
        public int GatewayId { get; set; }

        public string? PromoCode { get; set; }
    }
}
