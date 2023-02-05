using Microsoft.AspNetCore.Components;
using OPI.WebUI.ViewModels;

namespace OPI.WebUI.Parts;

public partial class IssueTypeEditor
{
    [Parameter]
    public string Title { get; set; } = "Types";

    [Parameter]
    public int IssueId { get; set; } = 0;

    [Parameter]
    public IDictionary<string, string>? AllCodeTypes { get; set; }

    [Parameter]
    public IEnumerable<string> RegisteredTypes { get; set; } = Enumerable.Empty<string>();

    [Parameter]
    public EventCallback<(int, IEnumerable<string>)> RegisteredTypesChanged { get; set; }

    public IEnumerable<IssueTypeCheckboxItem> CheckboxItems { get; set; } = Enumerable.Empty<IssueTypeCheckboxItem>();

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        List<IssueTypeCheckboxItem> checkboxItems = new();
        if (AllCodeTypes is not null && AllCodeTypes.Any())
        {
            foreach (KeyValuePair<string, string> kvp in AllCodeTypes)
            {
                bool checkIt = RegisteredTypes.Any(item => string.Equals(item, kvp.Key, StringComparison.OrdinalIgnoreCase));

                checkboxItems.Add(new IssueTypeCheckboxItem()
                {
                    Key = kvp.Key,
                    Value = kvp.Value,
                    IsChecked = checkIt,
                });
            }
        }

        CheckboxItems = checkboxItems;
    }

    public async Task CheckboxChanged(IssueTypeCheckboxItem sender, ChangeEventArgs e)
    {
        bool newValue = (bool)e.Value!;

        Console.WriteLine("Item id: {0}, Checkbox changed to: {1}", sender.Value, newValue);
        IEnumerable<string> result;
        if (newValue)
        {
            result = RegisteredTypes.Append(sender.Key);
        }
        else
        {
            result = RegisteredTypes.Where(item => !string.Equals(item, sender.Key, StringComparison.OrdinalIgnoreCase));
        }

        await RegisteredTypesChanged.InvokeAsync((IssueId, result));
        StateHasChanged();
    }
}