namespace OPI.WebUI;

public class JsonGenOptions
{
    /// <summary>
    /// Gets the schema path to use in the generated issue registry file for PR.
    /// </summary>
    public string SchemaPath { get; set; } = @"https://raw.githubusercontent.com/xiaomi7732/PerformanceIssueRepo/main/specs/registry/schema.20230306.json";
}