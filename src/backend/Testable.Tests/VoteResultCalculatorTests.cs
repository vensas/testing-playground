using EntityFrameworkCore.Testing.NSubstitute;
using Microsoft.EntityFrameworkCore;

namespace Testable.Tests;

public class VoteResultCalculatorTests : VerifyTestBase
{
    
    [Fact]
    public async Task CalculateResultsAsync_ReturnsCorrectResult()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var mockedDbContext = Create.MockedDbContextFor<DatabaseContext>(dbContextOptions);
        await mockedDbContext.Set<VoteEntity>().AddRangeAsync(
            new VoteEntity { Candidate = "Candidate 1", Party = "Party A" },
            new VoteEntity { Candidate = "Candidate 2", Party = "Party A" },
            new VoteEntity { Candidate = "Candidate 3", Party = "Party B" });
        await mockedDbContext.SaveChangesAsync();

        var voteResultCalculator = new VoteResultCalculator(mockedDbContext);

        // Act
        var results = await voteResultCalculator.CalculateResultsAsync();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("Party A", results[0].Party);
        Assert.Equal(2, results[0].VoteCount);
        Assert.Equal("Party B", results[1].Party);
        Assert.Equal(1, results[1].VoteCount);
    }
    
    [Fact]
    public async Task CalculateResultsAsync_ReturnsCorrectResult_AsSnapshotTest()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var mockedDbContext = Create.MockedDbContextFor<DatabaseContext>(dbContextOptions);
        await mockedDbContext.Set<VoteEntity>().AddRangeAsync(
            new VoteEntity { Candidate = "Candidate 1", Party = "Party A" },
            new VoteEntity { Candidate = "Candidate 2", Party = "Party A" },
            new VoteEntity { Candidate = "Candidate 3", Party = "Party B" });
        await mockedDbContext.SaveChangesAsync();

        var voteResultCalculator = new VoteResultCalculator(mockedDbContext);

        // Act
        var results = await voteResultCalculator.CalculateResultsAsync();

        // Assert
        await Verify(results, VerifySettings);
    }
}