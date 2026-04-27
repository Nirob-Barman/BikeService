namespace BikeService.Application.DTOs.PaymentGateway
{
    public class PaymentGatewayDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsSandbox { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TransactionCount { get; set; }
    }
}
