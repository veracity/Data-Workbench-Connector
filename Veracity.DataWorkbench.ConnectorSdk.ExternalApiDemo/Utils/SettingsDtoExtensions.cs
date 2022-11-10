using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts;

namespace Veracity.DataWorkbench.Connector.ExternalApiDemo.Utils;

public static class SettingsDtoExtensions
{
    public static bool GetValue(this SettingsDto settingsDto, string key, out string? value)
    {
        var settings = new Dictionary<string, string>(settingsDto.Settings, StringComparer.InvariantCultureIgnoreCase);

        return settings.TryGetValue(key, out value);
    }
}
