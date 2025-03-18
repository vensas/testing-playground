using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using Testable.Api;
using ValidationException = FluentValidation.ValidationException;

// App setup
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddDbContext<IUnitOfWork, DatabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<Auditor>();
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
using var dbContext = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
await dbContext.Database.MigrateAsync();

// Endpoints
var votesGroup = app.MapGroup("/votes")
    .WithTags("Votes");

votesGroup.MapGet("/", async (IUnitOfWork unitOfWork, Auditor auditor, CancellationToken cancellationToken) =>
{
    var set = await unitOfWork.Votes
        .OrderByDescending(v => v.Timestamp)
        .Select(v => new { v.Id, Vote = new Vote { Candidate = v.Candidate, Party = v.Party } })
        .ToListAsync(cancellationToken);
    
    var votes = new List<Vote>(set.Count);
    foreach (var entry in set)
    {
        votes.Add(entry.Vote);
        await auditor.AddAuditAsync(unitOfWork, nameof(VoteEntity), entry.Id, AuditActions.Read, entry.Vote);
    }

    await unitOfWork.SaveChangesAsync(cancellationToken);
    
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

var auditsGroup = app.MapGroup("/audits")
    .WithTags("Audits");

auditsGroup.MapGet("/", async (IUnitOfWork unitOfWork, CancellationToken cancellationToken) =>
{
    var audits = await unitOfWork.Audits
        .OrderByDescending(v => v.Timestamp)
        .Select(a => new Audit
        {
            EntityName = a.EntityName, 
            EntityId = a.EntityId, 
            Action = a.Action, 
            Payload = a.Payload, 
            Timestamp = a.Timestamp
        })
        .ToListAsync(cancellationToken);
    return TypedResults.Ok(audits);
});

auditsGroup.MapGet("/report", async (IUnitOfWork unitOfWork, CancellationToken cancellationToken) =>
{
    var audits = await unitOfWork.Audits
        .OrderBy(v => v.Timestamp)
        .ToListAsync(cancellationToken);
    
    var xml = AuditXmlReporter.GenerateXmlReport(audits);
    return TypedResults.Text(xml, "text/xml", Encoding.UTF8);
});

await app.RunAsync();

// Classes

public sealed class Audit
{
    public required Guid? EntityId { get; set; }
    
    public required string EntityName { get; set; }
    
    public required string Action { get; set; }
    
    public required string? Payload { get; set; }
    
    public required DateTime Timestamp { get; set; }
}

public sealed class Vote
{
    public required string Candidate { get; set; }
    public required string Party { get; set; }
}

public sealed class VoteResult
{
    public required string Party { get; set; }
    public required int VoteCount { get; set; }
}

public interface IUnitOfWork : IDisposable
{
    DbSet<AuditEntity> Audits { get; }
    
    DbSet<VoteEntity> Votes { get; }

    DatabaseFacade Database { get; }

    ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<VoteEntity> Votes => Set<VoteEntity>();
    
    public DbSet<AuditEntity> Audits => Set<AuditEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("testable");
    }
}

public static class AuditActions
{
    public const string Read = "READ";
    public const string Write = "WRITE";
}

public sealed class Auditor
{
    public Task AddReadAuditAsync<TEntity, TPayload>(IUnitOfWork unitOfWork, TEntity? entity, TPayload? payload)
        where TEntity: AuditableEntity
    {
        return AddAuditAsync(unitOfWork, typeof(TEntity).Name, entity?.Id, AuditActions.Read, payload);
    }
    
    public Task AddWriteAuditAsync<TEntity, TPayload>(IUnitOfWork unitOfWork, TEntity entity, TPayload payload)
        where TEntity: AuditableEntity
    {
        return AddAuditAsync(unitOfWork, typeof(TEntity).Name, entity.Id, AuditActions.Write, payload);
    }
    
    public Task AddAuditAsync<TEntity, TPayload>(IUnitOfWork unitOfWork, TEntity? entity, string action, TPayload? payload)
        where TEntity: AuditableEntity
    {
        return AddAuditAsync(unitOfWork, typeof(TEntity).Name, entity?.Id, action, payload);
    }
    
    public async Task AddAuditAsync<TPayload>(IUnitOfWork unitOfWork, string entityName, Guid? entityId, string action, TPayload? payload)
    {
        var auditEntity = new AuditEntity
        {
            EntityId = entityId,
            EntityName = entityName,
            Action = action,
            Payload = payload is not null ? JsonSerializer.Serialize(payload) : null,
            Timestamp = DateTime.UtcNow.ToUniversalTime()
        };

        await unitOfWork.Audits.AddAsync(auditEntity);
    }
}

public sealed class VoteResultCalculator(IUnitOfWork unitOfWork)
{
    public async Task<List<VoteResult>> CalculateResultsAsync()
    {
        return await unitOfWork.Votes
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

public sealed class VoteRegistrar(VoteValidator validator, IUnitOfWork unitOfWork, Auditor auditor)
{

    public async Task<string> RegisterVoteAsync(Vote vote)
    {
        var validationResult = await validator.ValidateAsync(vote);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var voteEntity = new VoteEntity
        {
            Candidate = vote.Candidate,
            Party = vote.Party,
            Timestamp = DateTime.UtcNow.ToUniversalTime()
        };

        await unitOfWork.Votes.AddAsync(voteEntity);
        await unitOfWork.SaveChangesAsync();
        
        await auditor.AddWriteAuditAsync(unitOfWork, voteEntity, vote);
        await unitOfWork.SaveChangesAsync();

        return voteEntity.Id.ToString();
    }
}

public sealed class VoteValidator : AbstractValidator<Vote>
{
    public VoteValidator()
    {
        RuleFor(vote => vote.Candidate)
            .NotEmpty().WithMessage("Candidate must not be empty");

        RuleFor(vote => vote.Party)
            .NotEmpty().WithMessage("Party must not be empty");
    }
}

public abstract class AuditableEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
}

public sealed class VoteEntity : AuditableEntity
{
    [Required] public required string Candidate { get; set; }

    [Required] public required string Party { get; set; }

    [Required] public DateTime Timestamp { get; set; }
}

public class AuditEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required, MaxLength(64)] public required string EntityName { get; set; }
    
    public required Guid? EntityId { get; set; }
    
    [Required, MaxLength(64)] public required string Action { get; set; }
    
    [MaxLength(8192)] public string? Payload { get; set; }
    
    [Required] public DateTime Timestamp { get; set; }
}