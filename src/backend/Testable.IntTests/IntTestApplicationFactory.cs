using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using VerifyTests.Http;

namespace Testable.IntTests;

public sealed class IntTestApplicationFactory : WebApplicationFactory<DatabaseContext>, IAsyncLifetime
{
    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!;
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
        using var c = CreateClient();
        return Task.CompletedTask;
    }

    private async Task InitializeRespawnerAsync()
    {
        _dbConnection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await _dbConnection.OpenAsync();
        
        using var scope = Services.CreateScope();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["testable"],
        });
    }

    public (HttpClient, RecordingHandler) CreateHttpRecordingClient()
    {
        var recording = new RecordingHandler(recording: false);
        return (CreateDefaultClient(ClientOptions.BaseAddress, recording), recording);
    }
}