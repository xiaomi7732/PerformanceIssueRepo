namespace OPI.Client;

public class OPIClientOptions
{
    public Uri BaseUri { get; set; } = new Uri("https://opir-test.azurewebsites.net/");
    public string DefaultScope { get; set; } = "api://549847ed-4e8b-47e3-829d-5e1a381ec08f/.default";

    public string SpecRepositoryOwner { get; set; } = "xiaomi7732";

    public string SpecRepositoryName { get; set; } = "PerformanceIssueRepo";
}