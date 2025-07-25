using Microsoft.EntityFrameworkCore;
using IdentityService.Infrastructure.Persistence;

namespace IdentityService.Tests.Infrastructure;

public class TestDbContext : IDisposable
{
    public IdentityServiceDbContext Context { get; }

    public TestDbContext()
    {
        var options = new DbContextOptionsBuilder<IdentityServiceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new IdentityServiceDbContext(options);
        Context.Database.EnsureCreated();
    }

    public async Task SeedDataAsync()
    {
        await Context.SaveChangesAsync();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
} 