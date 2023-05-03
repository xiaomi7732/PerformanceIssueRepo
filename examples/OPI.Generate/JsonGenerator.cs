using System.Text.Json;
using OPI.Core.Models;

namespace OPI.Generators;

/// <summary>
/// Generates a json dictionary from a given collection of PerfIssueItem.
/// </summary>
public class JsonGenerator
{
    public static void Generate(IEnumerable<PerfIssueItem> entries, string outputPath, string prefix = "")
    {
        if (entries is null)
        {
            throw new ArgumentNullException(nameof(entries));
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentNullException(nameof(outputPath));
        }

        // Initialize a dictionary to be serialized
        Dictionary<Guid, PerfIssueItem> entryDictionary = new Dictionary<Guid, PerfIssueItem>();

        // Iterate over entries
        foreach (PerfIssueItem entry in entries)
        {
            Guid id = entry.PermanentId ?? throw new ArgumentNullException($"{nameof(entry.PermanentId)} cannot be null.");

            entryDictionary.Add(id, entry);
        }

        // Serialize the dictionary
        string json = JsonSerializer.Serialize(entryDictionary);

        // Write the json to the output path
        File.WriteAllText(outputPath, json);
    }
}