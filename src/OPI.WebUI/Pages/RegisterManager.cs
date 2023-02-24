using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using OPI.Client;
using OPI.Core.Models;
using OPI.WebUI.ViewModels;

namespace OPI.WebUI.Pages;

[Authorize]
public partial class RegistryManager
{
    [Inject]
    public OPIClient OpiClient { get; private set; } = default!;

    [Inject]
    private IJSRuntime _jsRuntime { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider _authContext { get; set; } = default!;

    private IReadOnlyCollection<PerfIssueRegisterEntry>? _allRegisteredItems;
    public ObservableCollection<IssueRegistryItemViewModel> RegisteredItems { get; } = new ObservableCollection<IssueRegistryItemViewModel>();

    public bool Initialized { get; private set; }

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

    public int Active { get; private set; }
    public int InActive { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        await ReloadDataAsync();
        Initialized = true;
    }

    public async Task ToggleActivateAsync(IssueRegistryItemViewModel targetVM)
    {
        if (targetVM?.Model?.PermanentId is null)
        {
            return;
        }

        PerfIssueRegisterEntry? result = await OpiClient.ToggleActivateAsync(targetVM.Model.PermanentId.Value, cancellationToken: default);
        if (result is not null)
        {
            // Sync it on UI.
            targetVM.IsActive = !targetVM.IsActive;
            if (targetVM.IsActive)
            {
                // new value is active
                InActive--;
                Active++;
            }
            else
            {
                // new value is inactive
                Active--;
                InActive++;
            }
        }
        else
        {
            await _jsRuntime.InvokeVoidAsync("alert", $"Failed toggling active value for item: {targetVM.Model.PermanentId}");
        }
    }

    // Deleting
    public async Task DeleteAsync(IssueRegistryItemViewModel toDelete)
    {
        if (toDelete.Model?.PermanentId is null)
        {
            return;
        }

        bool confirmed = await _jsRuntime.InvokeAsync<bool>("confirm", $"Are you sure to delete registered issue by id: {toDelete.Model.PermanentId}?");

        if (!confirmed)
        {
            return;
        }

        PerfIssueRegisterEntry deleteTarget = new PerfIssueRegisterEntry()
        {
            PermanentId = toDelete.Model.PermanentId.Value,
        };
        if (await OpiClient.DeleteAsync(deleteTarget, default).ConfigureAwait(false))
        {
            if (toDelete.IsActive)
            {
                Active--;
            }
            else
            {
                InActive--;
            }
            RegisteredItems.Remove(toDelete);
        }
    }

    // Adding

    /// <summary>
    /// Cancel the newly added item from the UI.
    /// </summary>
    public Task OnCancelAddAsync(IssueRegistryItemViewModel target)
    {
        RegisteredItems.Remove(target);
        Console.WriteLine("Added cancelled.");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Add a new item onto the UI for the user to fill up the form.
    /// </summary>
    public async Task AddNewItemAsync()
    {
        if (RegisteredItems is null)
        {
            return;
        }

        PerfIssueRegisterEntry newModel = new PerfIssueRegisterEntry()
        {
            PermanentId = Guid.NewGuid(),
            CreatedBy = (await _authContext.GetAuthenticationStateAsync())?.User?.Identity?.Name,
        };

        IssueRegistryItemViewModel newViewModel = new IssueRegistryItemViewModel(newModel)
        {
            DisplayMode = IssueRegistryItemDisplayMode.Add,
        };
        RegisteredItems.Insert(0, newViewModel);
    }

    /// <summary>
    /// Submit the new item to the backend for persistent.
    /// </summary>
    public async Task OnSubmitAddAsync(IssueRegistryItemViewModel newItemSpec)
    {
        Console.WriteLine(nameof(OnSubmitAddAsync));
        if (newItemSpec?.Model is null)
        {
            return;
        }

        Guid.TryParse(newItemSpec.InsightIdString, out Guid newId);
        Guid? newNullableId = newId == Guid.Empty ? null : newId;

        Uri.TryCreate(newItemSpec.HelpLink, UriKind.Absolute, out Uri? helpLink);

        PerfIssueRegisterEntry newEntry = new PerfIssueRegisterEntry()
        {
            PermanentId = newNullableId,
            LegacyId = newItemSpec.LegacyId,
            IsActive = newItemSpec.IsActive,
            Title = newItemSpec.Title,
            Description = newItemSpec.Description,
            Recommendation = newItemSpec.Description,
            Rationale = newItemSpec.Rationale,
            DocURL = helpLink,
        };

        try
        {
            newEntry = await OpiClient.RegisterAsync(newEntry, default);

            if (newEntry.PermanentId is null)
            {
                throw new InvalidCastException("New item permanent id can't be null.");
            }
            newItemSpec.InsightIdString = newEntry.PermanentId.Value.ToString("D");
            newItemSpec.DisplayMode = IssueRegistryItemDisplayMode.Read;
            newItemSpec.Model.CreatedAt = newEntry.CreatedAt;
            newItemSpec.Model.CreatedBy = newEntry.CreatedBy;
            newItemSpec.Model.LastModifiedAt = newEntry.LastModifiedAt;
            newItemSpec.Model.LastModifiedBy = newEntry.LastModifiedBy;

            if (newItemSpec.IsActive)
            {
                Active++;
            }
            else
            {
                InActive++;
            }
        }
        catch (HttpRequestException ex)
        {
            await _jsRuntime.InvokeVoidAsync("alert", ex.Message);
        }
    }

    // Filter
    private async Task OnKeywordChanged()
    {
        await Task.Yield();
        FilterData();
    }

    private async Task ReloadDataAsync()
    {
        Console.WriteLine(nameof(ReloadDataAsync));
        _allRegisteredItems = (await OpiClient.ListAllRegisteredAsync(default))
            .OrderBy(item => item.LegacyId?.PadLeft(4))
            .ThenBy(item => item.PermanentId)
            .ToList()
            .AsReadOnly();

        Active = _allRegisteredItems.Count(item => item.IsActive);
        InActive = _allRegisteredItems.Count(item => !item.IsActive);
        FilterData();
    }

    private void FilterData()
    {
        if (_allRegisteredItems is null)
        {
            return;
        }

        RegisteredItems.Clear();
        IEnumerable<PerfIssueRegisterEntry> filteredResult = _allRegisteredItems;

        if (!string.IsNullOrEmpty(Keyword))
        {
            filteredResult = filteredResult.Where(item =>
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

        foreach (IssueRegistryItemViewModel item in filteredResult.Select(item => new IssueRegistryItemViewModel(item)))
        {
            RegisteredItems.Add(item);
        }
    }
}