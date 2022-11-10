using Microsoft.Extensions.Options;
using Veracity.DataWorkbench.Connector.ExternalApiDemo.Utils;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts.ConnectionValidation.Response;

namespace Veracity.DataWorkbench.Connector.ExternalApiDemo.Application;

/// <summary>
/// This class is used for validation of connection when that is being established 
/// and also for authorization of data queries
/// </summary>
public class QueryValidator
{
    private static readonly string ApiKey = "ApiKey";
    private static readonly string TenantAccessToken = "TenantAccessToken";

    private readonly Config _config;

    public QueryValidator(IOptions<Config> config)
    {
        _config = config.Value;
    }

    /// <summary>
    /// Validates connections settings which are supposed to be attached to every query
    /// </summary>
    /// <param name="settingsDto"></param>
    /// <returns></returns>
    public ConnectionValidationResultDto ValidateConnection(SettingsDto settingsDto)
    {
        var failures = new List<InvalidSettingDto>();

        if (!settingsDto.GetValue(ApiKey, out var dwApiKey))
            failures.Add(new InvalidSettingDto(ApiKey, new[] { "Not found" }));
        else if (dwApiKey != _config.ApiKey)
            failures.Add(new InvalidSettingDto(ApiKey, new[] { "Wrong key" }));

        if (!settingsDto.GetValue(TenantAccessToken, out var tenantToken))
            failures.Add(new InvalidSettingDto(TenantAccessToken, new[] { "Not found" }));
        else
        {
            var userId = JwtUtils.ValidateToken(tenantToken, _config.JwtSecret);
            if (userId == null)
                failures.Add(new InvalidSettingDto(ApiKey, new[] { "Invalid access token" }));
        }

        return new ConnectionValidationResultDto(!failures.Any(), failures);
    }
}
