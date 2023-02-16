using System.Resources;
using System.Resources.NetStandard;
using System.Xml;
using OPI.Core.Models;

namespace OPI.Generators;

public class ResxGenerator
{
    /// <summary>
    /// Generates a resx file from a given collection of PerfIssueRegisterEntry.
    /// </summary>
    /// <param name="entries">The collection of PerfIssueRegisterEntry.</param>
    /// <param name="outputPath">The output path of the resx file.</param>
    /// <param name="resxSchemaVersion">The schema version of the resx file.</param>
    public static void Generate(IEnumerable<PerfIssueRegisterEntry> entries, string outputPath, string resxSchemaVersion = "2.0")
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
        ResXResourceWriter writer = new ResXResourceWriter(outputPath);
        // Write the schema version
        writer.AddResource("$schema", resxSchemaVersion);
        // Write the entries
        foreach (PerfIssueRegisterEntry entry in entries)
        {
            // Write the title with key "{PermanentId}.Title"
            writer.AddResource($"{entry.PermanentId}.Title", entry.Title);
            // Write the description with key "{PermanentId}.Description"
            writer.AddResource($"{entry.PermanentId}.Description", entry.Description);
            // Write the doc url with key "{PermanentId}.DocURL"
            writer.AddResource($"{entry.PermanentId}.DocURL", entry.DocURL?.ToString());
            // Write the recommendation with key "{PermanentId}.Recommendation"
            writer.AddResource($"{entry.PermanentId}.Recommendation", entry.Recommendation);
            // Write the rationale with key "{PermanentId}.Rationale"
            writer.AddResource($"{entry.PermanentId}.Rationale", entry.Rationale);
        }
        writer.Generate();
        writer.Close();
    }
}
