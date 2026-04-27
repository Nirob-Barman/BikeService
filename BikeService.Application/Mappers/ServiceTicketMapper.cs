using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Domain.Entities;

namespace BikeService.Application.Mappers
{
    public static class ServiceTicketMapper
    {
        public static ServiceTicketDto ToDto(ServiceTicket e) => new()
        {
            Id = e.Id,
            Status = e.Status,
            DiagnosisNotes = e.DiagnosisNotes,
            EstimatedCompletionDate = e.EstimatedCompletionDate,
            BikeId = e.BikeId,
            BikeSummary = e.Bike != null
                ? $"{e.Bike.Year} {e.Bike.Make} {e.Bike.Model}"
                : string.Empty,
            MechanicId = e.MechanicId,
            MechanicName = e.Mechanic?.FullName,
            CustomerId = e.Bike?.CustomerId ?? string.Empty,
            AppointmentId = e.AppointmentId,
            CreatedAt = e.CreatedAt,
            Items = e.Items?.Select(ItemToDto).ToList() ?? new(),
            HasInvoice = e.Invoices?.Any() ?? false,
            InvoiceId = e.Invoices?.FirstOrDefault()?.Id
        };

        public static ServiceTicketItemDto ItemToDto(ServiceTicketItem i) => new()
        {
            Id = i.Id,
            ServiceTicketId = i.ServiceTicketId,
            ServiceTypeId = i.ServiceTypeId,
            ServiceTypeName = i.ServiceType?.Name,
            PartId = i.PartId,
            PartName = i.Part?.Name,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        };

        public static ServiceTicket ToEntity(ServiceTicketFormDto dto) => new()
        {
            BikeId = dto.BikeId,
            MechanicId = dto.MechanicId,
            AppointmentId = dto.AppointmentId,
            DiagnosisNotes = dto.DiagnosisNotes,
            EstimatedCompletionDate = dto.EstimatedCompletionDate
        };

        public static ServiceTicketItem ItemToEntity(ServiceTicketItemFormDto dto, int ticketId) => new()
        {
            ServiceTicketId = ticketId,
            ServiceTypeId = dto.ServiceTypeId,
            PartId = dto.PartId,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice
        };
    }
}
