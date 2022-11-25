namespace Veracity.DataWorkbench.ConnectorSdk.ExternalApiDemo.Utils;

public static class ReadOnlyDictionaryExtensions
{
    public static bool GetValue(this IReadOnlyDictionary<string, string> settingsIn, string key, out string? value)
    {
        var settings = new Dictionary<string, string>(settingsIn, StringComparer.InvariantCultureIgnoreCase);

        return settings.TryGetValue(key, out value);
    }
}
