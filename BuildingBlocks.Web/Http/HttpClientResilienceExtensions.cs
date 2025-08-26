using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly; 

namespace BuildingBlocks.Web.Http;

public static class HttpClientResilienceExtensions
{
    public static IHttpClientBuilder AddStandardResilience(
        this IHttpClientBuilder b,
        string pipelineName = "standard")
    {
        b.AddResilienceHandler(pipelineName, builder =>
        {
            // Retry
            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = args =>
                    ValueTask.FromResult(HttpClientResiliencePredicates.IsTransient(args.Outcome)),
            });
            builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 0.2,
                MinimumThroughput = 20,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(20)
            });

            // Per-attempt timeout
            builder.AddTimeout(TimeSpan.FromSeconds(3));
        });

        // Return the original IHttpClientBuilder (no casting!)
        return b;
    }
}
