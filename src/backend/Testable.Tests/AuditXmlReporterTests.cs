using Testable.Api;

namespace Testable.Tests;

public class AuditXmlReporterTests : VerifyTestBase
{
    [Fact]
    public void GenerateXmlReport_OneEntry_ReturnsCorrectXml()
    {
        // Arrange
        var auditEntities = new List<AuditEntity>
        {
            new()
            {
                Id = Guid.Parse("1702c6ca-ac32-44ce-848d-9cb8a65b832a"),
                EntityName = "TestEntity",
                EntityId = Guid.Parse("6537b7ca-0e59-4610-97d3-536bdd1f228a"),
                Action = AuditActions.Write,
                Payload = "{\"data\":\"test\"}",
                Timestamp = DateTime.FromFileTimeUtc(133867812473180590)
            }
        };

        // Act
        var result = AuditXmlReporter.GenerateXmlReport(auditEntities);

        // Assert
        Assert.Contains("<?xml version=\"1.0\" encoding=\"utf-16\"?>", result);
        Assert.Contains("AuditReport", result);
        Assert.Contains("TestEntity", result);
        Assert.Contains(AuditActions.Write, result);
        Assert.Contains("{\"data\":\"test\"}", result);
    }
    
    [Fact]
    public async Task GenerateXmlReport_OneEntry_ReturnsCorrectXml_AsSnapshotTest()
    {
        // Arrange
        var auditEntities = new List<AuditEntity>
        {
            new()
            {
                Id = Guid.Parse("1702c6ca-ac32-44ce-848d-9cb8a65b832a"),
                EntityName = "TestEntity",
                EntityId = Guid.Parse("6537b7ca-0e59-4610-97d3-536bdd1f228a"),
                Action = AuditActions.Write,
                Payload = "{\"data\":\"test\"}",
                Timestamp = DateTime.FromFileTimeUtc(133867812473180590)
            }
        };

        // Act
        var result = AuditXmlReporter.GenerateXmlReport(auditEntities);
        
        // Assert
        await Verify(result, VerifySettings);
    }
    
    [Fact]
    public void GenerateXmlReport_MultipleEntries_ReturnsCorrectXml()
    {
        // Arrange
        var auditEntities = new List<AuditEntity>
        {
            new()
            {
                Id = Guid.Parse("1702c6ca-ac32-44ce-848d-9cb8a65b832a"),
                EntityName = "TestEntity",
                EntityId = Guid.Parse("6537b7ca-0e59-4610-97d3-536bdd1f228a"),
                Action = AuditActions.Write,
                Payload = "{\"data\":\"test\"}",
                Timestamp = DateTime.FromFileTimeUtc(133867812473180590)
            },
            new()
            {
                Id = Guid.Parse("21ba56bc-7226-49aa-9831-d3fa8b27b83d"),
                EntityName = "TestEntity",
                EntityId = Guid.Parse("92b40209-cacc-4a8a-8b79-8d2d3bfd4041"),
                Action = AuditActions.Read,
                Payload = "{\"data\":\"test\"}",
                Timestamp = DateTime.FromFileTimeUtc(133967812473180590)
            }
        };

        // Act
        var result = AuditXmlReporter.GenerateXmlReport(auditEntities);

        // Assert
        Assert.Contains("<?xml version=\"1.0\" encoding=\"utf-16\"?>", result);
        Assert.Contains("AuditReport", result);
        Assert.Contains("TestEntity", result);
        Assert.Contains(AuditActions.Write, result);
        Assert.Contains("{\"data\":\"test\"}", result);
    }
    
    [Fact]
    public async Task GenerateXmlReport_MultipleEntries_ReturnsCorrectXml_AsSnapshotTest()
    {
        // Arrange
        var auditEntities = new List<AuditEntity>
        {
            new()
            {
                Id = Guid.Parse("1702c6ca-ac32-44ce-848d-9cb8a65b832a"),
                EntityName = "TestEntity",
                EntityId = Guid.Parse("6537b7ca-0e59-4610-97d3-536bdd1f228a"),
                Action = AuditActions.Write,
                Payload = "{\"data\":\"test\"}",
                Timestamp = DateTime.FromFileTimeUtc(133867812473180590)
            },
            new()
            {
                Id = Guid.Parse("21ba56bc-7226-49aa-9831-d3fa8b27b83d"),
                EntityName = "TestEntity",
                EntityId = Guid.Parse("92b40209-cacc-4a8a-8b79-8d2d3bfd4041"),
                Action = AuditActions.Read,
                Payload = "{\"data\":\"test\"}",
                Timestamp = DateTime.FromFileTimeUtc(133967812473180590)
            }
        };

        // Act
        var result = AuditXmlReporter.GenerateXmlReport(auditEntities);

        // Assert
        await Verify(result, VerifySettings);
    }
    
    [Fact]
    public void GenerateXmlReport_NullEntities_ThrowsArgumentNullException()
    { 
        // Act
        var action = new Action(() => AuditXmlReporter.GenerateXmlReport(null));

        // Assert
        var ex = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal("auditEntities", ex.ParamName);
        Assert.Contains("Value cannot be null.", ex.Message);
    }
    
    [Fact]
    public async Task GenerateXmlReport_NullEntities_ThrowsArgumentNullException_AsSnapshotTest()
    { 
        // Arrange & Act
        var action = new Action(() => AuditXmlReporter.GenerateXmlReport(null));

        // Assert
        await Throws(action, VerifySettings);
    }
}