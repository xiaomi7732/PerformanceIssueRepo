using Microsoft.AspNetCore.Components;
using OPI.Client;
using OPI.Core.Models;

namespace OPI.WebUI.Pages;

public partial class SingleIssueViewer
{
    public bool InProgress { get; private set; } = false;
    public string? Message { get; private set; }
    
    [Parameter]
    public string IssueId { get; set; } = default!;

    [Parameter]
    [SupplyParameterFromQuery(Name = "spec-version")]
    public string? SpecVersion { get; set; } = null;

    [Inject]
    private IAnonymousOPIClient _opiClient { get; set; } = default!;

    private PerfIssueItem? _perfIssueItem = null;

    public PerfIssueItem? Data => _perfIssueItem;

    protected override async Task OnInitializedAsync()
    {
        InProgress = true;
        try
        {
            if (string.IsNullOrEmpty(IssueId))
            {
                _perfIssueItem = null;
                Message= $"Issue id is not specified.";

                return;
            }

            if (!Guid.TryParse(IssueId, out Guid issueGuid))
            {
                _perfIssueItem = null;
                Message= $"{IssueId} is not a valid guid.";
                return;
            }

            // Default version to latest.
            if (string.IsNullOrEmpty(SpecVersion))
            {
                SpecVersion = "latest";
            }

            _perfIssueItem = await _opiClient.GetPerfIssueItem(issueGuid, SpecVersion, default);
            StateHasChanged();
        }
        catch(Exception ex)
        {
            Message = ex.Message;
        }
        finally
        {
            InProgress = false;
        }
    }
}