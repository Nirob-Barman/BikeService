namespace BikeService.Application.DTOs.ServiceTicket
{
    public class ServiceTicketItemDto
    {
        public int Id { get; set; }
        public int ServiceTicketId { get; set; }

        public int? ServiceTypeId { get; set; }
        public string? ServiceTypeName { get; set; }

        public int? PartId { get; set; }
        public string? PartName { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
    }
}
