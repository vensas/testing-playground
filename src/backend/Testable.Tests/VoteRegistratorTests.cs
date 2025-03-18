using FluentValidation;
using NSubstitute;

namespace Testable.Tests;

public class VoteRegistrarTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly VoteRegistrar _voteRegistrar;

    public VoteRegistrarTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _voteRegistrar = new VoteRegistrar(new VoteValidator(), _unitOfWork, new Auditor());
    }

    [Fact]
    public async Task RegisterVoteAsync_ValidVote_AddsVoteToDatabase()
    {
        // Arrange
        var vote = new Vote { Candidate = "Candidate A", Party = "Party A" };

        // Act
        await _voteRegistrar.RegisterVoteAsync(vote);

        // Assert
        await _unitOfWork.Received(1).Votes.AddAsync(Arg.Is<VoteEntity>(v =>
            v.Candidate == vote.Candidate && v.Party == vote.Party));
        await _unitOfWork.Received(1).Audits.AddAsync(Arg.Is<AuditEntity>(v =>
            v.Action == AuditActions.Write && v.EntityName == "VoteEntity"));
        await _unitOfWork.Received(2).SaveChangesAsync();
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