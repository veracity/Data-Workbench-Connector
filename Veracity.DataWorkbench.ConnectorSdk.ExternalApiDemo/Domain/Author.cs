using System.ComponentModel.DataAnnotations.Schema;

namespace Veracity.DataWorkbench.ConnectorSdk.ExternalApiDemo.Domain;

/// <summary>
/// In this simple 'Library' demo, this class represents an 'Author' type of data
/// </summary>
public class Author
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public List<Book> Books { get; init; } = default!;
}
