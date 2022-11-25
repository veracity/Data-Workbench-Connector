using Microsoft.AspNetCore.Mvc;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.API;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts.ConnectionValidation.Response;
using Veracity.DataWorkbench.ConnectorSdk.ExternalApiDemo.Application;

namespace Veracity.DataWorkbench.ConnectorSdk.ExternalApiDemo.Controllers;

/// <summary>
/// Vaildates a connection. This endpoint is called once when a new connection is established (or when it's updated/patched)
/// </summary>
[Route("api/validate")]
[ApiController]
public class ValidateConnectionController : ControllerBase, IConnectionValidation
{
    private readonly QueryValidator _queryValidator;

    public ValidateConnectionController(QueryValidator queryValidator)
    {
        _queryValidator = queryValidator;
    }

    /// <summary>
    /// Returns if provided connection settings are valid and authorized. If not, returns a list of failures.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    public Task<ConnectionValidationResultDto> ValidateConnection(SettingsDto settings, CancellationToken cancellationToken)
    {
        return Task.FromResult(_queryValidator.ValidateConnection(settings.Settings));
    }
}
