using MediatR;
using Microsoft.Extensions.Logging;

namespace BikeService.Application.Common.Behaviors;

public class ExceptionBehavior<TRequest, TResponse>(ILogger<ExceptionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception while processing {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}
