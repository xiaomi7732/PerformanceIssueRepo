namespace OPI.Client;

public class OPIClientOptions
{
    public Uri BaseUri { get; set; } = new Uri("https://opir-test.azurewebsites.net/");

    public string SpecRepositoryOwner { get; set; } = "xiaomi7732";

    public string SpecRepositoryName { get; set; } = "PerformanceIssueRepo";
}