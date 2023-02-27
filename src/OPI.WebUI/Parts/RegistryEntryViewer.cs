using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using OPI.WebUI.ViewModels;

namespace OPI.WebUI.Parts;

public partial class RegistryEntryViewer
{
    [Inject]
    private IJSRuntime _jsRuntime { get; set; } = default!;

    private EditContext? _editContext;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (ViewModel is null)
        {
            _editContext = null;
        }
        else
        {
            _editContext = new EditContext(ViewModel);
        }
    }

    [Parameter]
    public IssueRegistryItemViewModel? ViewModel { get; set; }

    [Parameter]
    public EventCallback<IssueRegistryItemViewModel> ToggleActivateCallback { get; set; }

    [Parameter]
    public EventCallback<IssueRegistryItemViewModel> DeleteItemCallback { get; set; }

    [Parameter]
    public EventCallback<IssueRegistryItemViewModel> CancelAddCallback { get; set; }

    [Parameter]
    public EventCallback<IssueRegistryItemViewModel> CancelEditCallback { get; set; }

    [Parameter]
    public EventCallback<IssueRegistryItemViewModel> SubmitAddedCallback { get; set; }

    [Parameter]
    public EventCallback<IssueRegistryItemViewModel> SubmitEditCallback { get; set; }

    public async Task ToggleActivateAsync()
    {
        if (ViewModel?.Model?.PermanentId is null)
        {
            return;
        }

        await ToggleActivateCallback.InvokeAsync(ViewModel);
    }

    public async Task HandleValidSubmit()
    {
        if (ViewModel is null)
        {
            return;
        }

        if (ViewModel.DisplayMode == IssueRegistryItemDisplayMode.Add)
        {
            await SubmitAddedCallback.InvokeAsync(ViewModel);
        }
        else if (ViewModel.DisplayMode == IssueRegistryItemDisplayMode.Edit)
        {
            await SubmitEditCallback.InvokeAsync(ViewModel);
        }
    }

    public void GenerateNewGuid()
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.InsightIdString = Guid.NewGuid().ToString("D");
    }

    public async Task DeleteItemAsync()
    {
        if (ViewModel?.Model?.PermanentId is null)
        {
            return;
        }

        await DeleteItemCallback.InvokeAsync(ViewModel);
    }

    public async Task CancelAddAsync()
    {
        if (ViewModel is null)
        {
            return;
        }

        bool go = await _jsRuntime.InvokeAsync<bool>("confirm", "Do you want to cancel adding this new item?");
        if (!go)
        {
            return;
        }

        await CancelAddCallback.InvokeAsync(ViewModel);
    }

    public async Task CancelEditAsync()
    {
        if (ViewModel is null)
        {
            return;
        }

        bool go = await _jsRuntime.InvokeAsync<bool>("confirm", "Do you want to cancel editing the item?");
        if (!go)
        {
            return;
        }

        await CancelEditCallback.InvokeAsync(ViewModel);
    }

    public void EnterEditMode()
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.DisplayMode = IssueRegistryItemDisplayMode.Edit;
    }
}