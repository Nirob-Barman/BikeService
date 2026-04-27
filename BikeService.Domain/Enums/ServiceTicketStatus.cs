namespace BikeService.Domain.Enums;

public enum ServiceTicketStatus
{
    Pending,
    Diagnosed,
    InProgress,
    QualityCheck,
    ReadyForPickup,
    Delivered,
    Cancelled
}
