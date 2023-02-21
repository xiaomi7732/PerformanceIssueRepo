using Microsoft.AspNetCore.Components;
using OPI.Core.Models;
using OPI.WebUI.Services;

namespace OPI.WebUI.Parts;

public partial class IssueVersionViewer
{
    [Inject]
    public IssueVersionService IssueVersionService { get; private set; } = default!;

    [Parameter]
    public PerfIssue? Issue { get; set; }
    public IEnumerable<string>? SpecVersions { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (Issue?.PermanentId is null || Issue.PermanentId.Value == Guid.Empty)
        {
            return;
        }

        List<string> versions = new List<string>();
        await foreach (string version in IssueVersionService.GetSpecVersionsAsync(Issue, default).ConfigureAwait(false))
        {
            versions.Add(version);
        }
        SpecVersions = versions;

        await base.OnParametersSetAsync();
    }
}