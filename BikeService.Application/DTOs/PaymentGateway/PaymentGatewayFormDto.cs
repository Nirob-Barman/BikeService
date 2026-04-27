namespace BikeService.Application.DTOs.PaymentGateway
{
    public class PaymentGatewayFormDto
    {
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Config { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsSandbox { get; set; }
    }
}
