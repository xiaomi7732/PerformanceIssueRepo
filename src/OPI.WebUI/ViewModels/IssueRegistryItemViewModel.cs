using System.ComponentModel.DataAnnotations;
using OPI.Core.Models;

namespace OPI.WebUI.ViewModels;

public class IssueRegistryItemViewModel
{
    public PerfIssueRegisterEntry? Model { get; }
    public IssueRegistryItemViewModel(PerfIssueRegisterEntry model)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));

        InsightIdString = model.PermanentId.HasValue ? model.PermanentId.Value.ToString("D") : string.Empty;
        IsActive = model.IsActive;
        LegacyId = model.LegacyId;
        Title = model.Title;
        Description = model.Description;
        HelpLink = model.DocURL?.AbsoluteUri;
        Recommendation = model.Recommendation;
        Rationale = model.Rationale;
    }

    public IssueRegistryItemDisplayMode DisplayMode { get; set; } = IssueRegistryItemDisplayMode.Read;

    [Required]
    public string? InsightIdString
    {
        get
        {
            if (Model?.PermanentId is null)
            {
                return null;
            }
            return Model.PermanentId.Value.ToString("D");
        }
        set
        {
            if (Model is null)
            {
                return;
            }
            if (Guid.TryParse(value, out Guid newGuid))
            {
                Model.PermanentId = newGuid;
            }
            else
            {
                Model.PermanentId = Guid.Empty;
            }
        }
    }

    public bool IsActive { get; set; }

    public string? LegacyId { get; set; }

    [Required]
    public string Title { get; set; }

    [Required]
    public string Description { get; set; }

    public string? HelpLink { get; set; }

    [Required]
    public string Recommendation { get; set; }

    public string? Rationale { get; set; }
}