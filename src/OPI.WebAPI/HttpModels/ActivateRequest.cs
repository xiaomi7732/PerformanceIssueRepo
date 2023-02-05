namespace OPI.WebAPI.HttpModels;

public record ActivateRequest
{
    public int IssueId { get; set; }
    public bool ToActivate { get; set; }
}