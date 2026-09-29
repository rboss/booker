using Booker.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace booker.Api.Tests;

/// <summary>Hosts the API in memory with its own isolated, empty database.</summary>
public sealed class BookApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"booker-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace the app's shared in-memory database so tests don't see each other's data.
            services.RemoveAll<DbContextOptions<BookDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<BookDbContext>>();
            services.AddDbContext<BookDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }

    /// <summary>Removes the seed data and replaces it with the given entities.</summary>
    public async Task SeedAsync(params object[] entities)
    {
        using var scope = Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<BookDbContext>();
        database.LoanHistory.RemoveRange(database.LoanHistory);
        database.BookStats.RemoveRange(database.BookStats);
        database.BookEntities.RemoveRange(database.BookEntities);
        database.Books.RemoveRange(database.Books);
        await database.SaveChangesAsync();

        database.AddRange(entities);
        await database.SaveChangesAsync();
    }

    public async Task<T> QueryAsync<T>(Func<BookDbContext, Task<T>> query)
    {
        using var scope = Services.CreateScope();
        return await query(scope.ServiceProvider.GetRequiredService<BookDbContext>());
    }
}
