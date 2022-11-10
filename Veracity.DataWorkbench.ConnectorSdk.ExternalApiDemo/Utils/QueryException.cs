using System.Net;

namespace Veracity.DataWorkbench.Connector.ExternalApiDemo.Utils;

public class QueryException : Exception
{
    public QueryException(HttpStatusCode statusCode, string? message = null) : base(message) =>
        StatusCode = statusCode;

    public HttpStatusCode StatusCode { get; }
}
