using Veracity.DataWorkbench.ConnectorSdk.ExternalApiDemo.Domain;
using Veracity.DataWorkbench.ConnectorSdk.ExternalApiDemo.Infrastructure;

namespace Veracity.DataWorkbench.ConnectorSdk.ExternalApiDemo.Utils;

/// <summary>
/// Seeds the DB with test/demo data
/// </summary>
public static class DBSeeder
{
    public static async Task Seed(DemoContext dbContext)
    {
        var author1 = new Author 
        { 
            FirstName = "Richard", 
            LastName = "Dawkins",
            Books = new List<Book> 
            { 
                new Book {Title = "The Selfish Gene", PublishedDate = new DateTime(1985, 3, 1)}, 
                new Book {Title = "The Extended Phenotype", PublishedDate = new DateTime(1995, 3, 1)},
                new Book {Title = "The Blind Watchmaker", PublishedDate = new DateTime(2005, 3, 1)},
            }
        };
        
        var author2 = new Author 
        { 
            FirstName = "Stephen", 
            LastName = "Hawking",
            Books = new List<Book>
            {
                new Book {Title = "A Brief History of Time", PublishedDate = new DateTime(1989, 3, 1)},
                new Book {Title = "The Universe in a Nutshell", PublishedDate = new DateTime(1999, 3, 1)},
                new Book {Title = "The Grand Design", PublishedDate = new DateTime(2009, 3, 1)},
            }
        };

        await dbContext.Authors.AddRangeAsync(author1, author2);
        await dbContext.SaveChangesAsync();
    }
}
