using System.Net.Http.Json;

namespace Testable.IntTests;

[Collection(nameof(TestCollection))]
public class EndpointIntTests : IAsyncLifetime
{
    private readonly IntTestApplicationFactory _factory;
    private readonly HttpClient _client;

    public EndpointIntTests(IntTestApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
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

}