using Microsoft.Extensions.Options;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts.ConnectionValidation.Response;
using Veracity.DataWorkbench.ConnectorSdk.ExternalApiDemo.Utils;

namespace Veracity.DataWorkbench.ConnectorSdk.ExternalApiDemo.Application;

/// <summary>
/// This class is used for validation of connection when that is being established 
/// and also for authorization of data queries
/// </summary>
public class QueryValidator
{
    private static readonly string DataWorkbenchApiKey = "DataWorkbenchApiKey";
    private static readonly string TenantAccessToken = "TenantAccessToken";

    private readonly Config _config;

    public QueryValidator(IOptions<Config> config)
    {
        _config = config.Value;
    }

    /// <summary>
    /// Validates connections settings which are supposed to be attached to every query
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    public ConnectionValidationResultDto ValidateConnection(IReadOnlyDictionary<string, string> settings)
    {
        var failures = new List<InvalidSettingDto>();

        if (!settings.GetValue(DataWorkbenchApiKey, out var dwApiKey))
            failures.Add(new InvalidSettingDto(DataWorkbenchApiKey, new[] { "Not found" }));
        else if (dwApiKey != _config.DataWorkbenchApiKey)
            failures.Add(new InvalidSettingDto(DataWorkbenchApiKey, new[] { "Wrong key" }));

        if (!settings.GetValue(TenantAccessToken, out var tenantToken))
            failures.Add(new InvalidSettingDto(TenantAccessToken, new[] { "Not found" }));
        else
        {
            var userId = JwtUtils.ValidateToken(tenantToken, _config.JwtSecret);
            if (userId == null)
                failures.Add(new InvalidSettingDto(DataWorkbenchApiKey, new[] { "Invalid access token" }));
        }

        return new ConnectionValidationResultDto(!failures.Any(), failures);
    }
}
