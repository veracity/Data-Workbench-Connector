using Microsoft.AspNetCore.Mvc;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.API;

namespace Veracity.DataWorkbench.ConnectorSdk.ExternalApiDemo.Controllers;

/// <summary>
/// This controller implements health check.
/// </summary>
[Route("api/healthcheck")]
[ApiController]
public class HealthCheckController : IHealthCheck
{
    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public Task HealthCheck()
    {
        return Task.CompletedTask;
    }
}
