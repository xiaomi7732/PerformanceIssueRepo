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

    public async Task RegisteredTypeChanged((int IssueId, IEnumerable<string> RegisteredTypes) sender)
    {
        Console.Write("Issue {0} Registered type changed to: {1}", sender.IssueId, string.Join(',', sender.RegisteredTypes));
        PerfIssueRegisterEntry? target = RegisteredItems.FirstOrDefault(item => item.IssueId == sender.IssueId);
        if (target is null)
        {
            throw new IndexOutOfRangeException($"Can't find registered issue #{sender.IssueId}");
        }
        target = target with
        {
            SupportedTypes = sender.RegisteredTypes,
        };

        // Update
        PerfIssueRegisterEntry? newItem = await OpiClient.UpdateEntryAsync(target, CancellationToken.None);

        if (newItem is not null)
        {
            Console.WriteLine("Is not null");

            await ReloadDataAsync();
            // Refresh data;
            StateHasChanged();
        }
    }

    private async Task ReloadDataAsync()
    {
        RegisteredItems = (await OpiClient.ListAllRegisteredAsync(default)).OrderBy(item => item.IssueId);
    }
}