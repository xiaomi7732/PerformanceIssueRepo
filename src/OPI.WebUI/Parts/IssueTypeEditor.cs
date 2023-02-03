using Microsoft.AspNetCore.Components;

namespace OPI.WebUI.Parts;

public partial class IssueTypeEditor
{
    [Parameter]
    public string Title { get; set; } = "Types";

    [Parameter]
    public IDictionary<string, string>? AllCodeTypes { get; set; }

    [Parameter]
    public IEnumerable<string> RegisteredTypes { get; set; } = Enumerable.Empty<string>();

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
}