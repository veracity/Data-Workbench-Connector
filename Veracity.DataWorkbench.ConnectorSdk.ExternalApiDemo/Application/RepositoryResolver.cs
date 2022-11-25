namespace Veracity.DataWorkbench.ConnectorSdk.ExternalApiDemo.Application;

/// <summary>
/// We assume that a data provider can have multiple internal data sources. Depending on a query, 
/// it should be able to resolve a data source (or multiple sources) needed for fulfilling this query.
/// This class implements a simple resolving policy
/// </summary>
public class RepositoryResolver
{
    private readonly IEnumerable<IRepository> _providers;

    public RepositoryResolver(IEnumerable<IRepository> providers)
    {
        _providers = providers;
    }

    /// <summary>
    /// Returns a data source abstraction by a parameter from a data query
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    public IRepository GetRepository(string dataSource) => 
        _providers.First(provider => provider.DataSource.Equals(dataSource, StringComparison.InvariantCultureIgnoreCase));
}
