using Microsoft.AspNetCore.Components;
using OPI.Client;
using OPI.Core.Models;

namespace OPI.WebUI.Pages;

public partial class Index
{
    [Inject]
    public OPIClient OpiClient { get; private set; } = default!;

    public IEnumerable<PerfIssueRegisterEntry> RegisteredItems { get; set; } = Enumerable.Empty<PerfIssueRegisterEntry>();

    protected override async Task OnInitializedAsync()
    {
        await ReloadDataAsync();
    }

    public async Task ToggleActivateAsync(Guid? issueId)
    {
        if(issueId is null)
        {
            // Must have a valid id to toggle the change.
            return;
        }

        PerfIssueRegisterEntry? result = await OpiClient.ToggleActivateAsync(issueId.Value, cancellationToken: default);
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
        RegisteredItems = (await OpiClient.ListAllRegisteredAsync(default)).OrderBy(item => item.LegacyId);
    }
}