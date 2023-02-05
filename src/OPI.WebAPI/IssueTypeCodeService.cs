using System.Reflection;
using System.Text.Json;

namespace OPI.WebAPI.Services;

public class IssueTypeCodeService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private static IDictionary<string, string>? _typeCodeMapping;

    public IssueTypeCodeService(
        JsonSerializerOptions jsonSerializerOptions)
    {
        _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
    }

    public Task<IDictionary<string, string>?> GetTypesAsync(CancellationToken cancellationToken)
    {
        return GetTypeCodeMappingAsync(cancellationToken);
    }

    // Logic
    public async Task<string> GetTypeStringAsync(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(code))
        {
            throw new ArgumentException($"'{nameof(code)}' cannot be null or empty.", nameof(code));
        }

        IDictionary<string, string>? mapping = await GetTypeCodeMappingAsync(cancellationToken).ConfigureAwait(false);
        if (mapping is null)
        {
            throw new InvalidDataException("Performance issue type code mapping does not exist.");
        }

        if (mapping.ContainsKey(code))
        {
            return mapping[code];
        }

        throw new IndexOutOfRangeException($"Invalid performance issue type code: {code}");
    }

    // DAL
    private async Task<IDictionary<string, string>?> GetTypeCodeMappingAsync(CancellationToken cancellationToken)
    {
        string binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string filePath = Path.Combine(binPath, "issue-code.json");

        if (_typeCodeMapping is null)
        {
            using (Stream readingStream = File.OpenRead(filePath))
            {
                Dictionary<string, string>? mapping = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(readingStream, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
                if (mapping is null)
                {
                    return null;
                }

                _typeCodeMapping = new Dictionary<string, string>(mapping, StringComparer.OrdinalIgnoreCase);
            }
        }
        return _typeCodeMapping;
    }
}