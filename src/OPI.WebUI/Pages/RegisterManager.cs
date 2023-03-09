using System.Collections.ObjectModel;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using OPI.Client;
using OPI.Core.Models;
using OPI.Core.Validators;
using OPI.WebAPI.Contracts;
using OPI.WebUI.ViewModels;

namespace OPI.WebUI.Pages;

[Authorize]
public partial class RegistryManager
{
    [Inject]
    private IAuthorizedOPIClient OpiClient { get; set; } = default!;

    [Inject]
    private IJSRuntime _jsRuntime { get; set; } = default!;

    [Inject]
    private NavigationManager _navigationManager { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider _authContext { get; set; } = default!;

    private IReadOnlyCollection<PerfIssueRegisterEntry>? _allRegisteredItems;
    public ObservableCollection<IssueRegistryItemViewModel> RegisteredItems { get; } = new ObservableCollection<IssueRegistryItemViewModel>();

    private bool _showActiveEntries = true;
    public bool ShowActiveEntries
    {
        get
        {
            return _showActiveEntries;
        }
        set
        {
            if (_showActiveEntries != value)
            {
                _showActiveEntries = value;
                FilterData();
            }
        }
    }

    private bool _showInactiveEntries = true;
    public bool ShowInactiveEntries
    {
        get
        {
            return _showInactiveEntries;
        }
        set
        {
            if (_showInactiveEntries != value)
            {
                _showInactiveEntries = value;
                FilterData();
            }

        }
    }

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
        try
        {
            await ReloadDataAsync(default);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            await _jsRuntime.InvokeVoidAsync("alert", "You don't have permission to get the data. Please apply for the proper role. You will be redirect back to the home page.");
            _navigationManager.NavigateTo("/", true);
        }
        catch (Exception ex)
        {
            await _jsRuntime.InvokeVoidAsync("alert", "Unknown error happened. Details: " + ex.Message);
        }
        finally
        {
            Initialized = true;
        }
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

            if (targetVM.Model is not null)
            {
                targetVM.Model.LastModifiedAt = result.LastModifiedAt;
                targetVM.Model.LastModifiedBy = result.LastModifiedBy;
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

        RegistryEntryRequestData requestData = newItemSpec.ToRequestData();

        if (!await ValidateSubmitAsync(requestData, cancellationToken: default))
        {
            return;
        }

        try
        {
            PerfIssueRegisterEntry newIssue = await OpiClient.RegisterAsync(requestData, default);

            if (newIssue.PermanentId is null)
            {
                throw new InvalidCastException("New item permanent id can't be null.");
            }
            newItemSpec.InsightIdString = newIssue.PermanentId.Value.ToString("D");
            newItemSpec.DisplayMode = IssueRegistryItemDisplayMode.Read;
            newItemSpec.Model.CreatedAt = newIssue.CreatedAt;
            newItemSpec.Model.CreatedBy = newIssue.CreatedBy;
            newItemSpec.Model.LastModifiedAt = newIssue.LastModifiedAt;
            newItemSpec.Model.LastModifiedBy = newIssue.LastModifiedBy;

            if (newIssue.IsActive)
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

    // Editing
    public Task OnCancelEditAsync(IssueRegistryItemViewModel target)
    {
        Console.WriteLine(nameof(OnCancelEditAsync));
        if (target?.Model is null)
        {
            Console.WriteLine("Target?.Model is null");
            return Task.CompletedTask;
        }

        // Restore values before editing
        target.UpdateBy(target.Model);
        target.DisplayMode = IssueRegistryItemDisplayMode.Read;

        Console.WriteLine("Edit cancelled.");
        return Task.CompletedTask;
    }

    public async Task OnSubmitEditAsync(IssueRegistryItemViewModel target)
    {
        if (target is null)
        {
            Console.WriteLine("Nothing to submit for editing.");
            return;
        }

        RegistryEntryRequestData requestData = target.ToRequestData();

        if (!await ValidateSubmitAsync(requestData, cancellationToken: default))
        {
            return;
        }

        PerfIssueRegisterEntry? result = await OpiClient.UpdateEntryAsync(requestData, default);

        if (result is null)
        {
            // Update failed.
            await _jsRuntime.InvokeVoidAsync("alert", $"Failed editing item by id: {target.InsightIdString}");
            return;
        }

        // Succeeded.
        target.DisplayMode = IssueRegistryItemDisplayMode.Read;

        // Update tracking info.
        if (target.Model is not null)
        {
            target.Model.LastModifiedAt = result.LastModifiedAt;
            target.Model.LastModifiedBy = result.LastModifiedBy;
        }
    }

    private async Task<bool> ValidateSubmitAsync(RegistryEntryRequestData requestData, CancellationToken cancellationToken)
    {
        if (!requestData.Options.AllowsDuplicatedHelpDocs)
        {
            if (_allRegisteredItems is null)
            {
                return true;
            }

            SameHelpLinkValidator helpLinkValidator = new SameHelpLinkValidator(requestData.Data, _allRegisteredItems);

            bool pass = await helpLinkValidator.ValidateAsync(cancellationToken: default);
            if (!pass)
            {
                await _jsRuntime.InvokeVoidAsync("alert", "Are you intend to submit an item with a duplicated help link? Check the proper option if that's the intention. Details: " + helpLinkValidator.Reason);
                return false;
            }
        }
        return true;
    }

    // Filter
    private async Task OnKeywordChanged()
    {
        await Task.Yield();
        FilterData();
    }

    private async Task ReloadDataAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine(nameof(ReloadDataAsync));
        _allRegisteredItems = (await OpiClient.ListAllRegisteredAsync(default))
            .OrderBy(item => item.LegacyId?.PadLeft(4))
            .ThenBy(item => item.PermanentId)
            .ToList()
            .AsReadOnly();

        ExtractedSubstitutes = (await OpiClient.ExtractSubstitutes("latest", cancellationToken).ConfigureAwait(false)).OrderBy(item => item, StringComparer.OrdinalIgnoreCase).ToList().AsReadOnly();

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

        if (!_showActiveEntries)
        {
            filteredResult = filteredResult.Where(item => !item.IsActive);
        }

        if (!_showInactiveEntries)
        {
            filteredResult = filteredResult.Where(item => item.IsActive);
        }

        foreach (IssueRegistryItemViewModel item in filteredResult.Select(item => new IssueRegistryItemViewModel(item)))
        {
            RegisteredItems.Add(item);
        }
    }

    // Substitutes
    public IReadOnlyCollection<string>? ExtractedSubstitutes { get; set; } = null;
}