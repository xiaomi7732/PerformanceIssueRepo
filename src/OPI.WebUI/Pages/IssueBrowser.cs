using Microsoft.AspNetCore.Components;
using OPI.Client;
using OPI.Core.Models;

namespace OPI.WebUI.Pages;

public partial class IssueBrowser
{
    [Inject]
    public OPIClient OpiClient { get; private set; } = default!;

    public IEnumerable<string> SpecVersionCollection { get; private set; } = Enumerable.Empty<string>();

    private IReadOnlyCollection<PerfIssueItem>? _loadedIssues = null;
    public IEnumerable<PerfIssueItem>? IssueCollection { get; private set; } = null;

    private string? _keyword;
    public string? Keyword
    {
        get { return _keyword; }
        set
        {
            if (!string.Equals(_keyword, value))
            {
                _keyword = value;
                Task.Run(OnKeywordChanged);
            }
        }
    }

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
            _loadedIssues = new List<PerfIssueItem>(await OpiClient.ListAllAsync(_pickedVersion, default)).AsReadOnly();
            ApplyFilter();
            StateHasChanged();
        }
        catch
        {
            IssueCollection = Enumerable.Empty<PerfIssueItem>();
            // TODO: Handle this error!
            throw;
        }
    }

    private async Task OnKeywordChanged()
    {
        await Task.Yield();
        ApplyFilter();
        StateHasChanged();
    }

    private void ApplyFilter()
    {
        if(_loadedIssues is null)
        {
            return;
        }

        if (string.IsNullOrEmpty(Keyword))
        {
            IssueCollection = _loadedIssues;
        }
        else
        {
            IssueCollection = _loadedIssues.Where(item =>
            {
                return (item.LegacyId is not null && item.LegacyId.Contains(Keyword, StringComparison.OrdinalIgnoreCase))
                    || (item.PermanentId.HasValue && item.PermanentId.Value.ToString("d").Contains(Keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(item.Title) && item.Title.Contains(Keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(item.Description) && item.Description.Contains(Keyword, StringComparison.OrdinalIgnoreCase))
                    || (item.DocURL is not null && item.DocURL.AbsoluteUri.Contains(Keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(item.Recommendation) && item.Recommendation.Contains(Keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(item.Rationale) && item.Rationale.Contains(Keyword, StringComparison.OrdinalIgnoreCase));
            });
        }
    }
}