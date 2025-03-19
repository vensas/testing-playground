using System.Net.Http.Json;
using VerifyTests.Http;

namespace Testable.IntTests;

[Collection(nameof(TestCollection))]
public class EndpointIntTests : VerifyTestBase, IAsyncLifetime
{
    private readonly IntTestApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly RecordingHandler _recordingHandler;

    public EndpointIntTests(IntTestApplicationFactory factory)
    {
        _factory = factory;
        (_client, _recordingHandler) = _factory.CreateHttpRecordingClient();
    }
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client.Dispose();
    }

    [Fact]
    public async Task GetVotes_NoVotes_ReturnsEmpty()
    {
        // Act
        var response = await _client.GetAsync("/votes");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var votes = await response.Content.ReadFromJsonAsync<List<VoteEntity>>();
        Assert.NotNull(votes);
        Assert.Empty(votes);
    }

    [Fact]
    public async Task GetVotes_WithVotesInDatabase_ReturnsVotes()
    {
        // Arrange
        var vote1 = new Vote { Candidate = "Boromir", Party = "Gondor" };
        var vote2 = new Vote { Candidate = "Legolas", Party = "Woodelves" };
        await _client.PostAsJsonAsync("/votes", vote1);
        await _client.PostAsJsonAsync("/votes", vote2);

        // Act
        var response = await _client.GetAsync("/votes");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var votes = await response.Content.ReadFromJsonAsync<List<VoteEntity>>();
        Assert.NotNull(votes);
        Assert.Equal(2, votes.Count);
        Assert.Contains(votes, v => v.Candidate == vote1.Candidate);
        Assert.Contains(votes, v => v.Candidate == vote2.Candidate);
    }

    [Fact]
    public async Task PostVote_ReturnsCreated()
    {
        // Arrange
        var vote = new Vote { Candidate = "Boromir", Party = "Gondor" };
        
        // Act
        var response = await _client.PostAsJsonAsync("/votes", vote);
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        var voteId = await response.Content.ReadAsStringAsync();
        Assert.NotNull(voteId);
    }

    [Fact]
    public async Task Results_ReturnsCorrectResults()
    {
        // Arrange
        var vote1 = new Vote { Candidate = "Boromir", Party = "Gondor" };
        var vote2 = new Vote { Candidate = "Legolas", Party = "Woodelves" };
        var vote3 = new Vote { Candidate = "Faramir", Party = "Gondor" };
        await _client.PostAsJsonAsync("/votes", vote1);
        await _client.PostAsJsonAsync("/votes", vote2);
        await _client.PostAsJsonAsync("/votes", vote3);

        // Act
        var response = await _client.GetAsync("/results");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var results = await response.Content.ReadFromJsonAsync<List<VoteResult>>();
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.Party == vote1.Party);
        Assert.Contains(results, r => r.Party == vote2.Party);
        Assert.Equal(2, results.First(r => r.Party == "Gondor").VoteCount);
    }

    [Fact]
    public async Task Audits_ReturnsCorrectAuditsReport()
    {
        // Arrange
        var vote1 = new Vote { Candidate = "Boromir", Party = "Gondor" };
        var vote2 = new Vote { Candidate = "Legolas", Party = "Woodelves" };
        var vote3 = new Vote { Candidate = "Faramir", Party = "Gondor" };
        await _client.PostAsJsonAsync("/votes", vote1);
        await _client.PostAsJsonAsync("/votes", vote2);
        await _client.PostAsJsonAsync("/votes", vote3);
        await _client.GetAsync("/votes");

        // Act
        var response = await _client.GetAsync("/audits/report");
        
        // Assert
        await Verify(response, VerifySettings);
    }
    
    [Fact]
    public async Task Audits_CallsAreCorrectAndReturnsCorrectAuditsReport()
    {
        _recordingHandler.Resume();
        
        // Arrange
        var voteService = new VoteService(_client);

        // Act
        await voteService.AddVoteAsync("Boromir", "Gondor");
        await voteService.AddVoteAsync("Legolas", "Woodelves");
        await voteService.AddVoteAsync("Faramir", "Gondor");

        var results = await voteService.GetVotesAsync();

        // Assert
        await Verify(new
        {
            results,
            _recordingHandler.Sends
        }, VerifySettings);
    }

    class VoteService(HttpClient client)
    {
        public Task AddVoteAsync(string candidate, string party) => client.PostAsJsonAsync("/votes", new Vote { Candidate = candidate, Party = party });
        
        public Task<List<VoteEntity>?> GetVotesAsync() => client.GetFromJsonAsync<List<VoteEntity>>("/votes");
    }
}