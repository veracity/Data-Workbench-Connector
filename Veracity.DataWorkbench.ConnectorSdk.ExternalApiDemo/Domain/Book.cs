using System.ComponentModel.DataAnnotations.Schema;

namespace Veracity.DataWorkbench.Connector.ExternalApiDemo.Domain;

/// <summary>
/// In this simple 'Library' demo, this class represents a 'Book' type of data
/// </summary>
public class Book
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; } = default!;
    public string Title { get; init; } = default!;
    public DateTime PublishedDate { get; init; } = default!;
    public Author Author { get; init; } = default!;
    public int AuthorId { get; init; } = default!;
}
