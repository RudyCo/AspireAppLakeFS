using AspireAppLakeFS.Client.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspireAppLakeFS.Client;

internal sealed class LakeFSHealthCheck(LakeFSClientFactory factory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // The factory connects (and authenticates).
            _ = await factory.GetHttpClientAsync(cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    }
}
