using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Wrappers;
using BikeService.Domain.Enums;

namespace BikeService.Application.Interfaces.Services
{
    public interface IServiceTicketService
    {
        Task<Result<List<ServiceTicketDto>>> GetAllAsync(TicketFilterDto? filter = null);
        Task<Result<ServiceTicketDto>> GetByIdAsync(int id);
        Task<Result<List<ServiceTicketDto>>> GetMyTicketsAsync();
        Task<Result<List<ServiceTicketDto>>> GetAssignedTicketsAsync();
        Task<Result<int>> CreateAsync(ServiceTicketFormDto dto);
        Task<Result<bool>> UpdateStatusAsync(int id, ServiceTicketStatus newStatus);
        Task<Result<bool>> AssignMechanicAsync(int id, int mechanicId);
        Task<Result<bool>> UpdateDiagnosisAsync(int id, string? notes, DateTime? estimatedCompletion);
        Task<Result<bool>> AddItemAsync(int ticketId, ServiceTicketItemFormDto dto);
        Task<Result<bool>> RemoveItemAsync(int itemId);
        Task<Result<bool>> CancelAsync(int id);
    }
}
