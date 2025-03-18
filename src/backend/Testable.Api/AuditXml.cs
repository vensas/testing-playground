using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Testable.Api;

[XmlRoot("AuditReport")]
public class AuditReport
{
    [XmlElement("AuditEntry")]
    public List<AuditEntryXml> Entries { get; set; } = [];
}

public class AuditEntryXml
{
    [XmlAttribute] public required string Id { get; set; }

    [XmlElement] public required string EntityName { get; set; }

    [XmlAttribute] public required string EntityId { get; set; }

    [XmlAttribute] public required string Action { get; set; }

    [XmlElement] public required string? Payload { get; set; }

    [XmlAttribute] public DateTime Timestamp { get; set; }
}

public static class AuditXmlReporter
{
    public static string GenerateXmlReport(IEnumerable<AuditEntity>? auditEntities)
    {
        ArgumentNullException.ThrowIfNull(auditEntities);

        var auditReport = new AuditReport();

        foreach (var entity in auditEntities)
        {
            auditReport.Entries.Add(new AuditEntryXml
            {
                Id = entity.Id.ToString("N"),
                EntityName = entity.EntityName,
                EntityId = entity.EntityId?.ToString("N") ?? string.Empty,
                Action = entity.Action,
                Payload = entity.Payload,
                Timestamp = entity.Timestamp
            });
        }

        var xmlSerializer = new XmlSerializer(typeof(AuditReport));

        using var stringWriter = new StringWriter();

        var settings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8
        };

        using var xmlWriter = XmlWriter.Create(stringWriter, settings);

        xmlSerializer.Serialize(xmlWriter, auditReport);

        return stringWriter.ToString();
    }
}
