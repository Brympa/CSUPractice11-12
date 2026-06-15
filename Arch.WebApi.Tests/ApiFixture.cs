using Arch.WebApi.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.WebApi.Tests;

public class ApiFixture : IAsyncLifetime
{
    public ApiFactory Api { get; } = new();
    private SqliteConnection? _connection;
    
    public async ValueTask InitializeAsync()
    {
        _connection = new SqliteConnection("Data Source=test_db;Mode=Memory;Cache=Shared");
        await _connection.OpenAsync();

        await using var scope = Api.Services.CreateAsyncScope();
        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        await dataContext.Database.EnsureDeletedAsync();
        await dataContext.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await using (var scope = Api.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<DataContext>().Database.EnsureDeletedAsync();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }
}