using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OPI.Client;
using OPI.Core.Models;

namespace OPI.WebUI.Pages;

[Authorize]
public partial class RegistryManager
{
    [Inject]
    public OPIClient OpiClient { get; private set; } = default!;

    [Inject]
    private IJSRuntime _jsRuntime { get; set; } = default!;

    private IReadOnlyCollection<PerfIssueRegisterEntry>? _allRegisteredItems;
    public IEnumerable<PerfIssueRegisterEntry>? RegisteredItems { get; set; } = null;

    private string? _keyword;
    public string? Keyword
    {
        get { return _keyword; }
        set
        {
            Console.WriteLine("Try to set new keyword of: {0}", value);
            if (!string.Equals(_keyword, value))
            {
                _keyword = value;
                Task.Run(OnKeywordChanged);
            }
        }
    }


    protected override async Task OnInitializedAsync()
    {
        await ReloadDataAsync();
    }

    public async Task ToggleActivateAsync(Guid? issueId)
    {
        if (issueId is null)
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

    public async Task DeleteAsync(PerfIssueRegisterEntry toDelete)
    {
        if (toDelete is null)
        {
            return;
        }

        bool confirmed = await _jsRuntime.InvokeAsync<bool>("confirm", $"Are you sure to delete registered issue by id: {toDelete.PermanentId}?");

        if (!confirmed)
        {
            return;
        }

        if (await OpiClient.DeleteAsync(toDelete, default).ConfigureAwait(false))
        {
            await ReloadDataAsync();
        }
    }


    private async Task OnKeywordChanged()
    {
        await Task.Yield();
        FilterData();
        StateHasChanged();
    }

    private async Task ReloadDataAsync()
    {
        _allRegisteredItems = (await OpiClient.ListAllRegisteredAsync(default))
            .OrderBy(item => item.LegacyId?.PadLeft(4))
            .ThenBy(item => item.PermanentId)
            .ToList()
            .AsReadOnly();
        FilterData();
    }

    private void FilterData()
    {
        if (_allRegisteredItems is null)
        {
            return;
        }

        if (string.IsNullOrEmpty(Keyword))
        {
            RegisteredItems = _allRegisteredItems;
        }
        else
        {
            RegisteredItems = _allRegisteredItems.Where(item =>
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