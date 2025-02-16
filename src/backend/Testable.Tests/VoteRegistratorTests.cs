using FluentValidation;
using NSubstitute;

namespace Testable.Tests;

public class VoteRegistrarTests
{
    private readonly IDatabaseContext _databaseContext;
    private readonly VoteRegistrar _voteRegistrar;

    public VoteRegistrarTests()
    {
        _databaseContext = Substitute.For<IDatabaseContext>();
        _voteRegistrar = new VoteRegistrar(new VoteValidator(), _databaseContext);
    }

    [Fact]
    public async Task RegisterVoteAsync_ValidVote_AddsVoteToDatabase()
    {
        // Arrange
        var vote = new Vote { Candidate = "Candidate A", Party = "Party A" };

        // Act
        await _voteRegistrar.RegisterVoteAsync(vote);

        // Assert
        await _databaseContext.Received(1).Votes.AddAsync(Arg.Is<VoteEntity>(v =>
            v.Candidate == vote.Candidate && v.Party == vote.Party));
        await _databaseContext.Received(1).SaveChangesAsync();
    }

    public static IEnumerable<object[]> GetInvalidVotes()
    {
        yield return
        [
            new Vote { Candidate = "", Party = "Party A" },
        ];
        yield return
        [
            new Vote { Candidate = "Candidate A", Party = "" },
        ];
        yield return
        [
            new Vote { Candidate = "", Party = "" },
        ];
    }

    [Theory]
    [MemberData(nameof(GetInvalidVotes))]
    public async Task RegisterVoteAsync_InvalidVote_ThrowsValidationException(Vote vote)
    {
        // Arrange, see MemberData

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _voteRegistrar.RegisterVoteAsync(vote));
    }
}