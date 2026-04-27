namespace BikeService.Application.DTOs.ServiceTicket
{
    public class ServiceTicketItemFormDto
    {
        public int? ServiceTypeId { get; set; }
        public int? PartId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
    }
}
