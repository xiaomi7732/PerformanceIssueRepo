using Microsoft.AspNetCore.Components;
using OPI.Client;
using OPI.Core.Models;

namespace OPI.WebUI.Pages;

public partial class IssueBrowser
{
    public IEnumerable<string> SpecVersionCollection { get; private set; } = Enumerable.Empty<string>();

    public IEnumerable<PerfIssueItem>? IssueCollection { get; private set; } = null;

    private string? _pickedVersion = null;
    public string? PickedVersion
    {
        get
        {
            return _pickedVersion;
        }
        set
        {
            if (!string.Equals(_pickedVersion, value))
            {
                _pickedVersion = value;
                Task.Run(PickedVersionChanged);
            }
        }
    }

    [Inject]
    public OPIClient OpiClient { get; private set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if (SpecVersionCollection is null || !SpecVersionCollection.Any())
        {
            SpecVersionCollection = (await OpiClient.ListSpecVersionsAsync(default).ConfigureAwait(false)).OrderByDescending(v => v, StringComparer.OrdinalIgnoreCase);
            PickedVersion = SpecVersionCollection.FirstOrDefault();
        }
    }

    private async Task PickedVersionChanged()
    {
        try
        {
            if (string.IsNullOrEmpty(_pickedVersion))
            {
                IssueCollection = Enumerable.Empty<PerfIssueItem>();
                return;
            }
            IssueCollection = await OpiClient.ListAllAsync(_pickedVersion, default);
            StateHasChanged();
        }
        catch
        {
            IssueCollection = Enumerable.Empty<PerfIssueItem>();
            // TODO: Handle this error!
            throw;
        }
    }
}