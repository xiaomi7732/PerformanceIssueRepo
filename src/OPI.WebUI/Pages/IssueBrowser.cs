using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using OPI.Client;
using OPI.Core.Models;

namespace OPI.WebUI.Pages;

public partial class IssueBrowser
{
    [Inject]
    public IAnonymousOPIClient OpiClient { get; private set; } = default!;

    [Inject]
    private IOptions<JsonGenOptions> JsonGenOptions { get; set; } = Options.Create<JsonGenOptions>(new JsonGenOptions());

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    public bool IsLoading { get; set; }

    public IEnumerable<string> SpecVersionCollection { get; private set; } = Enumerable.Empty<string>();

    private IReadOnlyCollection<PerfIssueItem>? _loadedIssues = null;
    public IEnumerable<PerfIssueItem>? IssueCollection { get; private set; } = null;

    public string? JsonContent { get; set; } = null;
    public string? CSVContent { get; set; } = null;

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

    private string _currentView = "card";
    public string CurrentView
    {
        get { return _currentView; }
        set
        {
            if (!string.Equals(_currentView, value, StringComparison.OrdinalIgnoreCase))
            {
                _currentView = value;
            }
        }
    }

    public IReadOnlyCollection<string>? ExtractedSubstitutes { get; set; } = null;

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
                Task.Run(OnPickedVersionChanged);
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

    private async Task OnPickedVersionChanged()
    {
        try
        {
            IsLoading = true;
            StateHasChanged();

            if (string.IsNullOrEmpty(_pickedVersion))
            {
                IssueCollection = Enumerable.Empty<PerfIssueItem>();
                return;
            }
            _loadedIssues = new List<PerfIssueItem>(await OpiClient.ListAllAsync(_pickedVersion, default)).OrderBy(item => item.LegacyId?.PadLeft(4)).ThenBy(item => item.PermanentId).ToList().AsReadOnly();
            ApplyFilter();
            await UpdateJsonViewAsync(cancellationToken: default);
            await UpdateCSVViewAsync(cancellationToken: default);
            await UpdateSubstitutesAsync(cancellationToken: default);
            StateHasChanged();
        }
        catch
        {
            IssueCollection = Enumerable.Empty<PerfIssueItem>();

            // TODO: Handle this error!
            throw;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task UpdateSubstitutesAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_pickedVersion))
        {
            ExtractedSubstitutes = null;
            return;
        }

        ExtractedSubstitutes = (await OpiClient.ExtractSubstitutes(_pickedVersion, cancellationToken).ConfigureAwait(false)).OrderBy(item => item, StringComparer.OrdinalIgnoreCase).ToList().AsReadOnly();
    }

    private async Task UpdateJsonViewAsync(CancellationToken cancellationToken)
    {
        JsonContent = null;
        if (string.IsNullOrEmpty(_pickedVersion))
        {
            return;
        }

        List<PerfIssueItem>? allIssueItems = (await OpiClient.ListAllAsync(_pickedVersion, cancellationToken).ConfigureAwait(false)).OrderBy(item => item.LegacyId?.PadLeft(4)).ThenBy(item => item.PermanentId)?.ToList();
        if (allIssueItems is null || !allIssueItems.Any())
        {
            JsonContent = string.Empty;
            return;
        }

        PerfIssueRegistryDocument doc = new PerfIssueRegistryDocument()
        {
            Schema = JsonGenOptions.Value.SchemaPath,
            Items = allIssueItems,
        };
        JsonSerializerOptions reSerializeOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        reSerializeOptions.WriteIndented = true;
        reSerializeOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

        JsonContent = JsonSerializer.Serialize<PerfIssueRegistryDocument>(doc, reSerializeOptions);
    }

    private async Task UpdateCSVViewAsync(CancellationToken cancellationToken)
    {
        CSVContent = null;
        if (string.IsNullOrEmpty(_pickedVersion))
        {
            return;
        }

        List<PerfIssueItem>? allIssueItems = (await OpiClient.ListAllAsync(_pickedVersion, cancellationToken).ConfigureAwait(false)).OrderBy(item => item.LegacyId?.PadLeft(4)).ThenBy(item => item.PermanentId)?.ToList();
        if (allIssueItems is null || !allIssueItems.Any())
        {
            Console.WriteLine("No csv content.");
            // Nothing
            CSVContent = string.Empty;
            return;
        }

        CSVContent = CreateCSVText(allIssueItems);
        Console.WriteLine("csv content: {0}", CSVContent);
    }

    private string CreateCSVText(IEnumerable<PerfIssueItem> items)
    {
        StringBuilder builder = new StringBuilder();
        foreach (PerfIssueItem item in items)
        {
            builder.AppendLine(CreateCSVLine(item));
        }
        return builder.ToString();
    }

    private string CreateCSVLine(PerfIssueItem item)
    {
        string line = string.Empty;
        // CSV schema: id,issue*,description*,recommendation*,rationale,docUrl

        // id
        if (item.PermanentId.HasValue)
        {
            line = item.PermanentId.Value.ToString("D");
        }
        else if (!string.IsNullOrEmpty(item.LegacyId))
        {
            line = $@"""{EscapeCSVField(item.LegacyId)}""";
        }
        line += ",";

        // issue
        line += $@"""{EscapeCSVField(item.Title)}"",";

        // description
        line += $@"""{EscapeCSVField(item.Description)}"",";

        // Recommendation
        line += $@"""{EscapeCSVField(item.Recommendation)}"",";

        // rationale
        if (!string.IsNullOrEmpty(item.Rationale))
        {
            line += $@"""{EscapeCSVField(item.Description)}""";
        }
        line += ",";

        // docurl
        if (item.DocURL is not null)
        {
            line += $@"""{EscapeCSVField(item.DocURL.AbsoluteUri)}""";
        }

        return line;
    }

    private string EscapeCSVField(string content)
        => content.Replace("\"", "\"\"", StringComparison.Ordinal);

    private async Task OnKeywordChanged()
    {
        await Task.Yield();
        ApplyFilter();
        StateHasChanged();
    }

    private void ApplyFilter()
    {
        if (_loadedIssues is null)
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