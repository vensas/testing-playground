using Testable.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var postgresResource = builder
    .AddPostgres("postgres");
var postgresDatabaseResource = postgresResource
    .AddDatabase("postgres-database", "testable");

var apiServiceResource = builder.AddProject<Projects.Testable_Api>("testable-api")
    .WithReference(postgresDatabaseResource, "DefaultConnection")
    .WaitFor(postgresDatabaseResource);

var appFrontendResource = builder.AddNpmApp("testable-frontend", "../../frontend")
    .WaitFor(apiServiceResource)
    .WithHttpEndpoint(env: "PUBLIC_PORT")
    .WithEnvironment("PUBLIC_OPEN_BROWSER", "false")
    .WithEnvironment("PUBLIC_BACKEND_URL", apiServiceResource.GetFirstOfEndpoint("https", "http"))
    .WithExternalHttpEndpoints();

apiServiceResource
    .WithEnvironment("CORS__ALLOWEDORIGINS", appFrontendResource.GetFirstOfEndpoint("https", "http"));

await builder
    .Build()
    .RunAsync();