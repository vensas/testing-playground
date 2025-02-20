using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace Testable.IntTests;

public sealed class IntTestApplicationFactory : WebApplicationFactory<DatabaseContext>, IAsyncLifetime
{
    private Respawner _respawner = default!;
    private DbConnection _dbConnection = default!;
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithDatabase("test-db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithImage("postgres:latest")
        .Build();
        
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var configBuilder = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", _postgreSqlContainer.GetConnectionString() },
            });
        builder.UseConfiguration(configBuilder.Build());

        base.ConfigureWebHost(builder);
    }

    public Task ResetDatabaseAsync() => _respawner.ResetAsync(_dbConnection);

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        
        await InitializeDatabaseAsync();
        await InitializeRespawnerAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbConnection.DisposeAsync();
        await _postgreSqlContainer.StopAsync();
    }

    private Task InitializeDatabaseAsync()
    {
        using var c = CreateClient(); // Create a client to ensure the database is created
        return Task.CompletedTask;
    }

    private async Task InitializeRespawnerAsync()
    {
        _dbConnection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await _dbConnection.OpenAsync();
        
        using var scope = Services.CreateScope();
        var dbOptions = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = [dbOptions.SchemaName],
        });
    }
}