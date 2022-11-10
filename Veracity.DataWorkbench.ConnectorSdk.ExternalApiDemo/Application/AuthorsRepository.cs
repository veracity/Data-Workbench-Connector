using System.Diagnostics.Metrics;
using System.Linq;
using SqlKata;
using SqlKata.Execution;
using Veracity.DataWorkbench.Connector.ExternalApiDemo.Domain;
using Veracity.DataWorkbench.Connector.ExternalApiDemo.Infrastructure;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts.DataDiscovery.Response;

namespace Veracity.DataWorkbench.Connector.ExternalApiDemo.Application;

/// <summary>
/// This class represents an Authors table as a data source. It has a few columns: FirstName, LastName and Books (by ref). 
/// For the sake of simplicity we will assume that this data source supports filtering only by authors' LastName. This is implemented here.
/// Another filtering option could be, for example, authors' FirstName, books' Published Date (for finding authors who had publications before or after given date), etc.
/// But we leave this as en exercise to a reader. ;-)
/// </summary>
public class AuthorsRepository : IRepository
{
    private const string AuthorsTable = nameof(DemoContext.Authors);
    private const string BooksTable = nameof(DemoContext.Books);

    private readonly QueryFactory _queryFactory;

    public AuthorsRepository(QueryFactory queryFactory)
    {
        _queryFactory = queryFactory;
    }

    public string DataSource => AuthorsTable;

    public IReadOnlyList<string> GetColumns(IEnumerable<string>? requiredColumns) => 
        new[] { nameof(Author.FirstName), nameof(Author.LastName) }
        .Where(col => requiredColumns == null || !requiredColumns.Any() || requiredColumns.Contains(col, StringComparer.InvariantCultureIgnoreCase))
        .ToList();

    public async Task<IReadOnlyList<IReadOnlyList<dynamic>>> GetData(IEnumerable<string>? requiredColumns, IEnumerable<QueryFilterDto>? filters)
    {
        var columnsToUse = GetColumns(requiredColumns);

        var query = new Query(AuthorsTable)
                        .Select(columnsToUse);
        
        query = AttachFilters(query, filters);        

        var rawResult = await _queryFactory.GetAsync(query);
        return rawResult.Select(row => (row as IDictionary<string, dynamic>).Values.ToList()).ToList();
    }

    public async Task<DataAggregatedDto> GetDataSummary(IEnumerable<QueryFilterDto>? filters)
    {
        var booksCountKey = "bCount";
        var earliestPublishedKey = "bPublishMin";
        var latestPublishedKey = "bPublishMax";

        var query = new Query(BooksTable)
            .Join(AuthorsTable, $"{BooksTable}.{nameof(Book.AuthorId)}", $"{AuthorsTable}.{nameof(Author.Id)}")
            .SelectRaw($"count(*) as [{booksCountKey}], MIN([{nameof(Book.PublishedDate)}]) as [{earliestPublishedKey}], MAX([{nameof(Book.PublishedDate)}]) as [{latestPublishedKey}]");

        query = AttachFilters(query, filters);

        var result = await _queryFactory.FirstAsync(query) as IDictionary<string, dynamic>;

        var authQuery = new Query(AuthorsTable).AsCount();
        authQuery = AttachFilters(authQuery, filters);
        var authorsCount = await _queryFactory.ExecuteScalarAsync<int>(authQuery);

        //Here it's assumed that all data is verified.
        //A data provider decides itself if provided data is verified, or not, or partially verified.
        //This may depend on their own data quality check.
        return new DataAggregatedDto(DateTime.Parse((string)result[earliestPublishedKey]),
                                     DateTime.Parse((string)result[latestPublishedKey]),
                                     authorsCount,
                                     (int)result[booksCountKey],
                                     new[] { new DataClassificationDto(ClassificationType.Verified, 1) });
    }

    public async Task<IReadOnlyList<FilterOptionDto>> DiscoverFilteringOptions(IEnumerable<string>? requiredColumns, IEnumerable<QueryFilterDto>? filters)
    {
        var columnsToUse = GetColumns(requiredColumns);

        //Since we support only LastName as filterable column...
        if (!columnsToUse.Contains(nameof(Author.LastName)))
            return new List<FilterOptionDto>();

        var query = new Query(AuthorsTable)
            .Join(BooksTable, $"{BooksTable}.{nameof(Book.AuthorId)}", $"{AuthorsTable}.{nameof(Author.Id)}")
            .GroupBy($"{AuthorsTable}.{nameof(Author.Id)}")
            .Select($"{AuthorsTable}.{nameof(Author.LastName)} as Key")
            .SelectRaw($"[{AuthorsTable}].[{nameof(Author.FirstName)}] || ' ' || [{AuthorsTable}].[{nameof(Author.LastName)}] as Value")
            .SelectRaw($"MIN({BooksTable}.{nameof(Book.PublishedDate)}) as DataFrom")
            .SelectRaw($"MAX({BooksTable}.{nameof(Book.PublishedDate)}) as DataTo")
            .SelectRaw($"COUNT({BooksTable}.{nameof(Book.Id)}) as DataCount");

        query = AttachFilters(query, filters);

        var rawResult = await _queryFactory.GetAsync(query);
        var lastNameFilterOptionValues = rawResult.Select(row => row as IDictionary<string, dynamic>)
                                .Select(rowDict => new FilterOptionValueDto((string)rowDict["Key"],
                                                                            (string)rowDict["Value"],
                                                                            DateTime.Parse((string)rowDict["DataFrom"]),
                                                                            DateTime.Parse((string)rowDict["DataTo"]),
                                                                            (int)rowDict["DataCount"],
                                                                            new[] { new DataClassificationDto(ClassificationType.Verified, 1) }))
                                .ToList();
        return new[] 
        { 
            new FilterOptionDto(nameof(Author.LastName), lastNameFilterOptionValues) 
        };
    }

    private static Query AttachFilters(Query query, IEnumerable<QueryFilterDto>? filters)
    {
        //For the sake of simplicity here we support filtering data only by LastName and only by AnyInList type of condition.
        //We leave it as an exercise to a reader to implement other filters
        if (filters != null && filters.Any())
        {
            foreach (var filter in filters)
            {
                if (filter.ColumnName.Equals("LastName", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (filter.FilterType == QueryFilterType.AnyInList)
                    {
                        query = query.WhereIn(nameof(Author.LastName), filter.Values);
                    }
                }
            }
        }

        return query;
    }    
}
