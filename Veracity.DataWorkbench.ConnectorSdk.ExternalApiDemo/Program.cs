using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SqlKata.Compilers;
using SqlKata.Execution;
using Veracity.DataWorkbench.ConnectorSdk.ExternalApiDemo.Application;
using Veracity.DataWorkbench.ConnectorSdk.ExternalApiDemo.Infrastructure;
using Veracity.DataWorkbench.ConnectorSdk.ExternalApiDemo.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSection = builder.Configuration.GetSection("Application");
builder.Services.Configure<Config>(appSection);

//For demo purposes an in-memory relational database is used
var dbConnection = new SqliteConnection("DataSource=file::memory:?cache=shared");
dbConnection.Open();

builder.Services.AddDbContext<DemoContext>(opts => opts.UseSqlite(dbConnection));
builder.Services.AddScoped(prv =>
{
    var connection = prv.GetService<DemoContext>()!.Database.GetDbConnection();
    return new QueryFactory(dbConnection, new SqliteCompiler());
});

//Here application services are initialized in the DI container
builder.Services.AddScoped<QueryValidator>();
builder.Services.AddScoped<IRepository, AuthorsRepository>();
builder.Services.AddScoped<IRepository, BooksRepository>();
builder.Services.AddScoped<RepositoryResolver>();
builder.Services.AddScoped<DataService>();

//Here HTTP pipeline is configured with custom handlers and settings
builder.Services.AddControllers(options =>
    options.Filters.Add<QueryExceptionFilter>())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var app = builder.Build();

//For simplicity, we create, migrate and seed the databse on a start of the application
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetService<DemoContext>()!;
    context.Database.Migrate();
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
    await DBSeeder.Seed(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();

app.Run();
