namespace Testable.IntTests;

[CollectionDefinition(nameof(TestCollection))]
public class TestCollection : ICollectionFixture<IntTestApplicationFactory>;