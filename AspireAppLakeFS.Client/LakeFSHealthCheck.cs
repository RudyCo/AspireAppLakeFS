using AspireAppLakeFS.Client.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspireAppLakeFS.Client;

internal sealed class LakeFSHealthCheck(LakeFSClientFactory factory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // The factory connects (and authenticates).
            var client = await factory.GetHttpClientAsync(cancellationToken);
            var response = await client.GetAsync("/api/v1/healthcheck", cancellationToken);
            return response.IsSuccessStatusCode ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    }
}