﻿using Microsoft.AspNetCore.Mvc;
using Veracity.DataWorkbench.Connector.ExternalApiDemo.Application;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.API;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts.ConnectionValidation.Response;

namespace Veracity.DataWorkbench.Connector.ExternalApiDemo.Controllers;

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
    /// <returns></returns>
    [HttpPost]
    public Task<ConnectionValidationResultDto> ValidateConnection(SettingsDto settings)
    {
        return Task.FromResult(_queryValidator.ValidateConnection(settings));
    }
}
