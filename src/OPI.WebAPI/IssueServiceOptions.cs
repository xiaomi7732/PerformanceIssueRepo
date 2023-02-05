using System.Reflection;

namespace OPI.WebAPI.Services;

public class IssueServiceOptions
{
    private const string DefaultFileName = @"perf-issue.json";
    public IssueServiceOptions()
    {
        string? assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);
        if (string.IsNullOrEmpty(assemblyPath))
        {
            throw new DirectoryNotFoundException("Can't locate entry assembly folder. How could this happen?");
        }
        IssueFilePath = Path.Combine(assemblyPath, DefaultFileName);
    }

    public string IssueFilePath { get; set; }
}