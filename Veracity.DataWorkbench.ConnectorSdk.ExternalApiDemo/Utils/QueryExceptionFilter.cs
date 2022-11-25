using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Veracity.DataWorkbench.ConnectorSdk.ExternalApiDemo.Utils;

/// <summary>
/// This filter is embedded into http pipeline. It handles application exceptions and converts them to responses with correct http status codes
/// </summary>
public class QueryExceptionFilter : IActionFilter, IOrderedFilter
{
    public int Order => int.MaxValue - 10;

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is QueryException queryException)
        {
            context.Result = new ObjectResult(queryException.Message)
            {
                StatusCode = (int)queryException.StatusCode
            };

            context.ExceptionHandled = true;
        }
    }
}
