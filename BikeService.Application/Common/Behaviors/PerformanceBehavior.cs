using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BikeService.Application.Common.Behaviors;

public class PerformanceBehavior<TRequest, TResponse>(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int SlowRequestThresholdMs = 500;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > SlowRequestThresholdMs)
        {
            logger.LogWarning(
                "Long running request: {RequestName} took {ElapsedMilliseconds}ms",
                typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
