using Microsoft.AspNetCore.Components;

namespace OPI.WebUI.Parts;

public partial class SubstituteViewer
{
    [Parameter]
    public string Title { get; set; } = "Substitute";

    [Parameter]
    public IReadOnlyCollection<string>? ExtractedSubstitutes { get; set; } = null;
}