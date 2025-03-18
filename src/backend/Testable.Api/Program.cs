using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using ValidationException = FluentValidation.ValidationException;

// App setup
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddDbContext<IDatabaseContext, DatabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<VoteValidator>();
builder.Services.AddScoped<VoteRegistrar>();
builder.Services.AddScoped<VoteResultCalculator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// Auto Migrate and DB Reset
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<IDatabaseContext>();
await dbContext.Database.MigrateAsync();

// Endpoints
var votesGroup = app.MapGroup("/votes")
    .WithTags("Votes");
votesGroup.MapGet("/", async (DatabaseContext db) =>
{
    var votes = await db.Votes
        .Select(v => new {v.Id, v.Candidate, v.Party, v.Timestamp})
        .OrderByDescending(v => v.Timestamp)
        .ToListAsync();
    return Results.Ok(votes);
});
votesGroup.MapPost("/", async (VoteRegistrar voteRegistrar, [FromBody] Vote vote) =>
{
    var registeredVoteId = await voteRegistrar.RegisterVoteAsync(vote);
    return Results.Created($"/votes/{registeredVoteId}", registeredVoteId);
});
var resultsGroup = app.MapGroup("/results")
    .WithTags("Results");
resultsGroup.MapGet("/", async (VoteResultCalculator calculator) =>
{
    var aggregatedVotes = await calculator.CalculateResultsAsync();
    return TypedResults.Ok(aggregatedVotes);
});

await app.RunAsync();

// Classes

public record Vote
{
    public Guid? Id { get; set; }
    public required string Candidate { get; set; }
    public required string Party { get; set; }
}

public record VoteResult
{
    public required string Party { get; set; }
    public required int VoteCount { get; set; }
}

public interface IDatabaseContext
{
    DbSet<VoteEntity> Votes { get; }

    DatabaseFacade Database { get; }

    ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class DatabaseOptions
{
    public string SchemaName { get; set; } = "testable";
}

public class DatabaseContext : DbContext, IDatabaseContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    public DbSet<VoteEntity> Votes => Set<VoteEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var dbOptions = this.GetService<IOptions<DatabaseOptions>>().Value;
        modelBuilder.HasDefaultSchema(dbOptions.SchemaName);
    }
}


public class VoteResultCalculator
{
    private readonly DatabaseContext _dbContext;

    public VoteResultCalculator(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<VoteResult>> CalculateResultsAsync()
    {
        return await _dbContext.Votes
            .GroupBy(v => v.Party)
            .Select(g => new VoteResult
            {
                Party = g.Key,
                VoteCount = g.Count()
            })
            .OrderByDescending(v => v.VoteCount)
            .ToListAsync();
    }
}

public class VoteRegistrar
{
    private readonly VoteValidator _validator;
    private readonly IDatabaseContext _databaseContext;

    public VoteRegistrar(VoteValidator validator, IDatabaseContext databaseContext)
    {
        _validator = validator;
        _databaseContext = databaseContext;
    }

    public async Task<string> RegisterVoteAsync(Vote vote)
    {
        ArgumentNullException.ThrowIfNull(vote);

        var validationResult = await _validator.ValidateAsync(vote);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var voteEntity = new VoteEntity
        {
            Id = Guid.NewGuid(),
            Candidate = vote.Candidate,
            Party = vote.Party,
            Timestamp = DateTime.UtcNow.ToUniversalTime()
        };

        await _databaseContext.Votes.AddAsync(voteEntity);
        await _databaseContext.SaveChangesAsync();

        return voteEntity.Id.ToString();
    }
}

public class VoteValidator : AbstractValidator<Vote>
{
    public VoteValidator()
    {
        RuleFor(vote => vote.Candidate)
            .NotEmpty().WithMessage("Candidate must not be empty");

        RuleFor(vote => vote.Party)
            .NotEmpty().WithMessage("Party must not be empty");
    }
}

public class VoteEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required] public required string Candidate { get; set; }

    [Required] public required string Party { get; set; }

    [Required] public DateTime Timestamp { get; set; }
}