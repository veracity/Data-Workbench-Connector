using Microsoft.AspNetCore.Mvc;
using Veracity.DataWorkbench.Connector.ExternalApiDemo.Application;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.API;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts.DataDiscovery.Request;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts.DataDiscovery.Response;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts.DataQuery.Request;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts.DataQuery.Response;

namespace Veracity.DataWorkbench.Connector.ExternalApiDemo.Controllers;

/// <summary>
/// This controller implements two endpoints: for querying data and for data discovery (aka filtering options)
/// Here you can see that API abstractions can be implemented in the same class when it fits your application design
/// </summary>
[ApiController]
public class DataQueryController : ControllerBase, IDataQuery, IDataDiscovery
{
    private readonly DataService _dataService;

    public DataQueryController(DataService dataService)
    {
        _dataService = dataService;
    }

    /// <summary>
    /// Queries summary (or filtering options).
    /// </summary>
    /// <param name="queryDto"></param>
    /// <returns></returns>
    [Route("api/discovery")]
    [HttpPost]
    public Task<DataDiscoveryResultDto> DiscoverFilteringOptions(DataDiscoveryQueryDto queryDto)
    {
        return _dataService.DiscoveryData(queryDto);
    }

    /// <summary>
    /// Queries data, with options for selecting columns and applying certain criteria to queried data
    /// </summary>
    /// <param name="queryDto"></param>
    /// <returns></returns>
    [Route("api/query")]
    [HttpPost]
    public Task<DataQueryResultDto> QueryData(DataQueryDto queryDto)
    {
        return _dataService.QueryData(queryDto);
    }
}
