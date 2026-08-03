using BikeService.Application.Features.Appointments.Commands.CompleteAppointment;
using BikeService.Application.Features.Appointments.Queries.GetAppointmentById;
using BikeService.Application.Features.ServiceTickets.Commands.CreateServiceTicket;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Appointments.Commands.ConvertAppointmentToTicket;

public class ConvertAppointmentToTicketCommandHandler(
    IUnitOfWork unitOfWork,
    IMediator mediator) : IRequestHandler<ConvertAppointmentToTicketCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ConvertAppointmentToTicketCommand request, CancellationToken cancellationToken)
    {
        var appointmentResult = await mediator.Send(new GetAppointmentByIdQuery(request.AppointmentId), cancellationToken);
        if (!appointmentResult.Success)
            return Result<int>.Fail(appointmentResult.Errors ?? new List<string> { "Appointment not found." });

        var appointment = appointmentResult.Data!;

        await unitOfWork.BeginTransaction();
        try
        {
            var ticketResult = await mediator.Send(
                new CreateServiceTicketCommand(appointment.BikeId, null, appointment.Id, null, null),
                cancellationToken);
            if (!ticketResult.Success)
            {
                await unitOfWork.RollbackAsync();
                return Result<int>.Fail(ticketResult.Errors ?? new List<string> { "Failed to create service ticket." });
            }

            var completeResult = await mediator.Send(new CompleteAppointmentCommand(request.AppointmentId), cancellationToken);
            if (!completeResult.Success)
            {
                await unitOfWork.RollbackAsync();
                return Result<int>.Fail(completeResult.Errors ?? new List<string> { "Failed to complete appointment." });
            }

            await unitOfWork.CommitAsync();
            return Result<int>.Ok(ticketResult.Data, "Service ticket created.");
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
}
