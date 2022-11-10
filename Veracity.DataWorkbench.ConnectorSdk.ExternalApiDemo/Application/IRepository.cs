using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts.DataDiscovery.Response;

namespace Veracity.DataWorkbench.Connector.ExternalApiDemo.Application;

public interface IRepository
{
    /// <summary>
    /// This is used for lookup of the service in DI container
    /// </summary>
    public string DataSource { get; }

    /// <summary>
    /// Returns a list of columns from a given table which values can be returned in a data query
    /// Its result depends on if there were any filter columns in a request
    /// </summary>
    /// <param name="requiredColumns">Represents filter columns from a 'POST /query' request. Optional.</param>
    /// <returns></returns>
    IReadOnlyList<string> GetColumns(IEnumerable<string>? requiredColumns);

    /// <summary>
    /// Returns a data from a given table
    /// </summary>
    /// <param name="requiredColumns">Represents filter columns from a 'POST /query' request. Optional.</param>
    /// <param name="filters">Query filters. Optional.</param>
    /// <returns></returns>
    Task<IReadOnlyList<IReadOnlyList<dynamic>>> GetData(IEnumerable<string>? requiredColumns, IEnumerable<QueryFilterDto>? filters);

    /// <summary>
    /// Returns a table summary
    /// </summary>
    /// <param name="filters">Query filters. Optional.</param>
    /// <returns></returns>
    Task<DataAggregatedDto> GetDataSummary(IEnumerable<QueryFilterDto>? filters);

    /// <summary>
    /// Returns all available filtering options: columns which can be filtered together with assets summary per each such column
    /// </summary>
    /// <param name="requiredColumns">Represents filter columns from a 'POST /discovery' request. Optional.</param>
    /// <param name="filters">Query filters. Optional.</param>
    /// <returns></returns>
    Task<IReadOnlyList<FilterOptionDto>> DiscoverFilteringOptions(IEnumerable<string>? requiredColumns, IEnumerable<QueryFilterDto>? filters);
}
