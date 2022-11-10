using Microsoft.EntityFrameworkCore;
using Veracity.DataWorkbench.Connector.ExternalApiDemo.Domain;

namespace Veracity.DataWorkbench.Connector.ExternalApiDemo.Infrastructure;

/// <summary>
/// Defines a relational DB with provider's data 
/// </summary>
public class DemoContext : DbContext
{
    public DemoContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>().HasOne(b => b.Author).WithMany(author => author.Books).HasForeignKey(book => book.AuthorId);
    }

    public DbSet<Author> Authors { get; init; } = default!;
    public DbSet<Book> Books { get; init; } = default!;
}
