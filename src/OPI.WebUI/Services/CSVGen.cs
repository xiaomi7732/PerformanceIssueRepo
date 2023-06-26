using System.Text;

namespace OPI.WebUI.Services;

/// <summary>
/// A csv generating service.
/// </summary>
/// <typeparam name="T">The type of the object to generate csv.</typeparam>
public class CSVGen<T>
{
    /// <summary>
    /// Create a text of CSV file.
    /// </summary>
    /// <param name="data">The set of data to generate csv.</param>
    /// <param name="createRawFields">A method to extract raw fields for a line of csv. For example, for an object with Title and Description, return an
    /// enumerable of strings, the first one is Title, the second one is description. No escaping of text needs to be considered, no comma.
    ///  </param>
    /// <returns>A text represent of a csv content.</returns>
    public string CreateCSVText(IEnumerable<T> data, Func<T, IEnumerable<string?>> createRawFields)
    {
        StringBuilder builder = new StringBuilder();
        foreach (T item in data)
        {
            builder.AppendLine(CreateCSVLine(item, createRawFields));
        }
        return builder.ToString();
    }

    private string CreateCSVLine(T item, Func<T, IEnumerable<string?>> createRawFields)
    {
        string line = string.Empty;
        foreach (string? rawItem in createRawFields(item))
        {
            line += $@"""{EscapeCSVField(rawItem)}"",";
        }

        // Remove the tailing comma to the end of the line.
        return line.Substring(0, line.Length - 1);
    }

    private string? EscapeCSVField(string? content)
        => content?.Replace("\"", "\"\"", StringComparison.Ordinal);
}