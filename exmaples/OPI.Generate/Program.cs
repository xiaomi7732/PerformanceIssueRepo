using Microsoft.Extensions.Configuration;
using OPI.Core.Models;
using OPI.Generators;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

// Configure command line arguments
IConfiguration configuration = new ConfigurationBuilder()
    .AddCommandLine(args)
    .Build();
// Log the configuration
Console.WriteLine("Configuration: " + JsonSerializer.Serialize(configuration.AsEnumerable()));
// Get type of the generator
string generatorType = configuration["type"] ?? throw new ArgumentNullException(nameof(generatorType));
// Get the output path
string outputPath = configuration["output"] ?? throw new ArgumentNullException(nameof(outputPath));
// Get the registry version
string registryVersion = configuration["registryVersion"] ?? "latest";
// Get the registry endpoint
string registryEndpoint = configuration["registryEndpoint"] ?? "https://opir-test.azurewebsites.net/issues";
// Get the optional prefix
string prefix = configuration["prefix"] ?? string.Empty;

// Reference OPI.CLient - Add DI container
// Make get request to registry endpoint and get returned json
Func<PerfIssueItem[]> GetRegistryJson = () =>
{
    Console.WriteLine("Getting registry json from " + registryEndpoint + "?spec-version=" + registryVersion);
    string json = new HttpClient().GetStringAsync(registryEndpoint + "?spec-version=" + registryVersion).Result ?? throw new ArgumentNullException(nameof(json));
    // Deserialize json into PerfIssueRegisterEntry
    PerfIssueItem[] entries = JsonSerializer.Deserialize<PerfIssueItem[]>(json, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin)
    });

    return entries;
};


// Generate command for resx
Action ResXGenerator = () =>
{
    PerfIssueItem[] entries = GetRegistryJson();
    string resxSchemaVersion = configuration["resxSchemaVersion"] ?? "2.0";
    ResxGenerator.Generate(entries, outputPath, resxSchemaVersion, prefix);
    Console.WriteLine("Wrote resx file to " + outputPath);
};

// Generate based on type
if (generatorType.ToLower() == "resx")
{
    ResXGenerator();
}

return 0;