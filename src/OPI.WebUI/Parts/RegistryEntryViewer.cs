using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OPI.WebUI.ViewModels;

namespace OPI.WebUI.Parts;

public partial class RegistryEntryViewer
{
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
    public EventCallback<Guid> DeleteItemCallback { get; set; }

    [Parameter]
    public EventCallback<Guid> CancelAddCallback { get; set; }

    [Parameter]
    public EventCallback<IssueRegistryItemViewModel> SubmitAddedCallback { get; set; }

    public async Task ToggleActivateAsync()
    {
        if (ViewModel?.Model?.PermanentId is null)
        {
            return;
        }

        await ToggleActivateCallback.InvokeAsync(ViewModel);
    }

    public async Task HandleValidAddSubmit()
    {
        if (ViewModel is null)
        {
            return;
        }

        await SubmitAddedCallback.InvokeAsync(ViewModel);
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

        await DeleteItemCallback.InvokeAsync(ViewModel.Model.PermanentId.Value);
    }

    public async Task CancelAddAsync()
    {
        if (ViewModel?.Model?.PermanentId is null)
        {
            return;
        }
        await CancelAddCallback.InvokeAsync(ViewModel.Model.PermanentId.Value);
    }
}