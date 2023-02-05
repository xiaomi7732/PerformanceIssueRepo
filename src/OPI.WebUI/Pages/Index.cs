using Microsoft.AspNetCore.Components;
using OPI.Client;
using OPI.Core.Models;

namespace OPI.WebUI.Pages;

public partial class Index
{
    [Inject]
    public OPIClient OpiClient { get; private set; } = default!;

    public IEnumerable<PerfIssueRegisterEntry> RegisteredItems { get; set; } = Enumerable.Empty<PerfIssueRegisterEntry>();

    public IDictionary<string, string>? AllIssueTypes { get; set; } = null;

    protected override async Task OnInitializedAsync()
    {
        await ReloadDataAsync();
        AllIssueTypes = new Dictionary<string, string>(await OpiClient.ListAllIssueTypes(default).ConfigureAwait(false));
    }

    public async Task ToggleActivateAsync(int issueId)
    {
        PerfIssueRegisterEntry? result = await OpiClient.ToggleActivateAsync(issueId, cancellationToken: default);
        if (result is not null)
        {
            await ReloadDataAsync();
        }
        else
        {
            // TODO: Output error;
        }
    }

    private async Task ReloadDataAsync()
    {
        RegisteredItems = (await OpiClient.ListAllRegisteredAsync(default)).OrderBy(item => item.IssueId);
    }
}