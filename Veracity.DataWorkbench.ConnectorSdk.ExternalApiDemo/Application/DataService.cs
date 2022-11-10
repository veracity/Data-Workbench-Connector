using System.Net;
using Veracity.DataWorkbench.Connector.ExternalApiDemo.Utils;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts.DataDiscovery.Request;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts.DataDiscovery.Response;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts.DataQuery.Request;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts.DataQuery.Response;

namespace Veracity.DataWorkbench.Connector.ExternalApiDemo.Application;

/// <summary>
/// This class encapsulates handling of data queries. It validates a query (together with access authorization) and delivers the query to a specific data source handler.
/// </summary>
public class DataService
{
    private static readonly string SourceTableSettingsKey = "SourceTable";

    private readonly QueryValidator _queryValidator;
    private readonly RepositoryResolver _repositoryResolver;

    public DataService(QueryValidator queryValidator, RepositoryResolver repositoryResolver)
    {
        _queryValidator = queryValidator;
        _repositoryResolver = repositoryResolver;
    }

    /// <summary>
    /// Returns data from a required data source
    /// </summary>
    /// <param name="queryDto">Query parameters which are coming from the Query endpoint</param>
    /// <returns></returns>
    public async Task<DataQueryResultDto> QueryData(DataQueryDto queryDto)
    {
        var repository = ValidateAndGetRepository(queryDto.Settings!);

        var columns = repository.GetColumns(queryDto.Columns);
        var rows = await repository.GetData(queryDto.Columns, queryDto.Filters);

        //All data is wrapped into a DTO and returned.
        //Here a pagination is very simplistic and only for demo purposes. In real application you would want to handle pagination on DB queries level.
        return new DataQueryResultDto(new DataDto(columns, rows), new Provider.Abstractions.Contracts.PaginationDto(1, rows.Count, 1, rows.Count));
    }

    /// <summary>
    /// Returns summary and filtering options for data from a required source
    /// </summary>
    /// <param name="queryDto"></param>
    /// <returns></returns>
    public async Task<DataDiscoveryResultDto> DiscoveryData(DataDiscoveryQueryDto queryDto)
    {
        var repository = ValidateAndGetRepository(queryDto.Settings!);

        var dataSummary = await repository.GetDataSummary(queryDto.Filters);
        var filteringOptions = await repository.DiscoverFilteringOptions(queryDto.Columns, queryDto.Filters);

        return new DataDiscoveryResultDto(dataSummary, filteringOptions, new PaginationDto(1, filteringOptions.Count, 1, filteringOptions.Count));
    }

    /// <summary>
    /// Here validation (~authorization) happens and also a data source is resolved
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    /// <exception cref="QueryException"></exception>
    private IRepository ValidateAndGetRepository(SettingsDto settings)
    {
        var connectionValidationStatus = _queryValidator.ValidateConnection(settings);
        if (!connectionValidationStatus.IsValid)
            throw new QueryException(HttpStatusCode.Unauthorized, "Invalid connection");

        if (!settings.GetValue(SourceTableSettingsKey, out var sourceTable) || string.IsNullOrEmpty(sourceTable))
            throw new QueryException(HttpStatusCode.BadRequest, "An internal data source is not specified");

        var repository = _repositoryResolver.GetRepository(sourceTable);

        if (repository == null)
            throw new QueryException(HttpStatusCode.NotFound, "An internal data source is not found");
        return repository;
    }    
}
