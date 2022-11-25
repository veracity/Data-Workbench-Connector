using SqlKata;
using SqlKata.Execution;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts;
using Veracity.DataWorkbench.Connector.Provider.Abstractions.Contracts.DataDiscovery.Response;
using Veracity.DataWorkbench.ConnectorSdk.ExternalApiDemo.Domain;
using Veracity.DataWorkbench.ConnectorSdk.ExternalApiDemo.Infrastructure;

namespace Veracity.DataWorkbench.ConnectorSdk.ExternalApiDemo.Application;

/// <summary>
/// This class represents a Books table as a data source. It has a few columns: Title, Author (by ref) and PublishedDate. 
/// For the sake of simplicity we will assume that this data source supports filtering only by book's Title. This is implemented here.
/// Another filtering option could be, for example, a book's Published Date (for finding books published before or after given date).
/// But this we leave as en exercise to a reader. ;-)
/// Also here Books are both an asset and data. This makes the 'Data Discovery / Filtering Optinos' scenario a little bit too flat.
/// See a similar functionality in AuthorsRepository (where Authors represent assets and their books - data) for a more interesting implementation
/// </summary>
public class BooksRepository : IRepository
{
    private const string AuthorsTable = nameof(DemoContext.Authors);
    private const string BooksTable = nameof(DemoContext.Books);

    private readonly QueryFactory _queryFactory;

    public BooksRepository(QueryFactory queryFactory)
    {
        _queryFactory = queryFactory;
    }

    public string DataSource => BooksTable;

    public IReadOnlyList<string> GetColumns(IEnumerable<string>? requiredColumns) =>
        new[] { nameof(Book.Title), nameof(Book.PublishedDate), "Author" }
        .Where(col => requiredColumns == null || !requiredColumns.Any() || requiredColumns.Contains(col, StringComparer.InvariantCultureIgnoreCase))
        .ToList();

    public async Task<IReadOnlyList<IReadOnlyList<dynamic>>> GetData(IEnumerable<string>? requiredColumns,
        IEnumerable<QueryFilterDto>? filters)
    {
        var columnsToUse = GetColumns(requiredColumns);

        var query = new Query(BooksTable);

        if (columnsToUse.Contains("Author"))
        {
            query = query.Join(AuthorsTable, $"{AuthorsTable}.{nameof(Author.Id)}", $"{BooksTable}.{nameof(Book.AuthorId)}")
                        .Select(columnsToUse.Where(col => col != "Author").Select(bookCol => $"{BooksTable}.{bookCol}"))
                        .SelectRaw($"[{AuthorsTable}].[{nameof(Author.FirstName)}] || ' ' || [{AuthorsTable}].[{nameof(Author.LastName)}]");
        }
        else
        {
            query = query.Select(columnsToUse.Select(bookCol => $"{BooksTable}.{bookCol}"));
        }

        query = AttachFilters(query, filters);

        var queryRes = await _queryFactory.GetAsync(query);
        return queryRes.Select(row => ((row as IDictionary<string, dynamic>)!).Values.ToList()).ToList();
    }

    public async Task<DataAggregatedDto> GetDataSummary(IEnumerable<QueryFilterDto>? filters)
    {
        var countKey = "Count";
        var minDateKey = "Min";
        var maxDateKey = "Max";

        var query = new Query(BooksTable)
                            .SelectRaw($"count(*) as [{countKey}], MIN([{nameof(Book.PublishedDate)}]) as [{minDateKey}], MAX([{nameof(Book.PublishedDate)}]) as [{maxDateKey}]");

        query = AttachFilters(query, filters);

        var result = await _queryFactory.FirstAsync(query) as IDictionary<string, dynamic>;
        
        //Here it's assumed that all data is verified.
        //It depends on a data provider if data is verified, or not, or partially verified.
        return new DataAggregatedDto(DateTime.Parse((string)result![minDateKey]),
                                     DateTime.Parse((string)result[maxDateKey]),
                                     (int)result[countKey],
                                     (int)result[countKey],
                                     new[] { new DataClassificationDto(ClassificationType.Verified, 1) });
    }

    public async Task<IReadOnlyList<FilterOptionDto>> DiscoverFilteringOptions(IEnumerable<string>? requiredColumns, IEnumerable<QueryFilterDto>? filters)
    {
        var columnsToUse = GetColumns(requiredColumns);

        //Since we support only Title as filterable column...
        if (!columnsToUse.Contains(nameof(Book.Title)))
            return new List<FilterOptionDto>();

        var query = new Query(BooksTable).Select();
        query = AttachFilters(query, filters);

        var books = await _queryFactory.GetAsync<Book>(query);

        var titleFilterOptionValues = books.Select(book => new FilterOptionValueDto(book.Title, book.Title, book.PublishedDate, book.PublishedDate, 1, new[] { new DataClassificationDto(ClassificationType.Verified, 1) })).ToList();

        return new[] 
        {
            new FilterOptionDto(nameof(Book.Title), titleFilterOptionValues)
        };
    }

    private static Query AttachFilters(Query query, IEnumerable<QueryFilterDto>? filters)
    {
        if (filters != null && filters.Any())
        {
            foreach (var filter in filters)
            {
                if (filter.ColumnName.Equals(nameof(Book.Title), StringComparison.InvariantCultureIgnoreCase))
                {
                    if (filter.FilterType == QueryFilterType.StringContains)
                    {
                        query = query.WhereLike($"{BooksTable}.{nameof(Book.Title)}", $"%{filter.Values.Single()}%");
                    }
                }
            }
        }

        return query;
    }      
}
