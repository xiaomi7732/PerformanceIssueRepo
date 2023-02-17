using System.Resources.NetStandard;
using OPI.Core.Models;

namespace OPI.Generators;

/// <summary>
/// Generates a resx file from a given collection of PerfIssueItem.
/// </summary>
public class ResxGenerator
{
    /// <summary>
    /// Generates a resx file from a given collection of PerfIssueItem.
    /// </summary>
    /// <param name="entries">The collection of PerfIssueItem.</param>
    /// <param name="outputPath">The output path of the resx file.</param>
    /// <param name="resxSchemaVersion">The schema version of the resx file.</param>
    public static void Generate(IEnumerable<PerfIssueItem> entries, string outputPath, string resxSchemaVersion = "2.0", string prefix = "")
    {
        if (entries is null)
        {
            throw new ArgumentNullException(nameof(entries));
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentNullException(nameof(outputPath));
        }

        // Initialize the resource writer
        using ResXResourceWriter writer = new ResXResourceWriter(outputPath);
        // Write the schema version
        writer.AddResource("$schema", resxSchemaVersion);
        // Write the entries
        foreach (PerfIssueItem entry in entries)
        {
            // Write the title with key "{PermanentId}.Title"
            writer.AddResource(ResxGenerator.GetKey(entry.PermanentId, "Title", prefix), entry.Title);
            // Write the description with key "{PermanentId}.Description"
            writer.AddResource(ResxGenerator.GetKey(entry.PermanentId, "Description", prefix), entry.Description);
            // Write the doc url with key "{PermanentId}.DocURL"
            if (entry.DocURL is not null)
            {
                writer.AddResource(ResxGenerator.GetKey(entry.PermanentId, "DocURL", prefix), entry.DocURL?.AbsoluteUri);
            }
            // Write the recommendation with key "{PermanentId}.Recommendation"
            writer.AddResource(ResxGenerator.GetKey(entry.PermanentId, "Recommendation", prefix), entry.Recommendation);
            // Write the rationale with key "{PermanentId}.Rationale"
            writer.AddResource(ResxGenerator.GetKey(entry.PermanentId, "Rationale", prefix), entry.Rationale);
        }
        writer.Generate();
    }

    private static string GetKey(Guid? permanentId, string key, string prefix)
    {
        if (permanentId is null)
        {
            throw new ArgumentNullException(nameof(permanentId));
        }

        prefix = string.IsNullOrWhiteSpace(prefix) ? string.Empty : $"{prefix}.";
        return $"{prefix}{permanentId}.{key}";
    }
}
