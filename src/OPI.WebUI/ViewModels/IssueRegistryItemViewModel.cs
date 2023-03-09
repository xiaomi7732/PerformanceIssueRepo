using System.ComponentModel.DataAnnotations;
using OPI.Core.Models;
using OPI.WebAPI.Contracts;

namespace OPI.WebUI.ViewModels;

public class IssueRegistryItemViewModel
{
    public PerfIssueRegisterEntry? Model { get; }
    public IssueRegistryItemViewModel(PerfIssueRegisterEntry model)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        UpdateBy(model);
    }

    public void UpdateBy(PerfIssueRegisterEntry entry)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        InsightIdString = entry.PermanentId.HasValue ? entry.PermanentId.Value.ToString("D") : string.Empty;
        IsActive = entry.IsActive;
        LegacyId = entry.LegacyId;
        Title = entry.Title;
        Description = entry.Description;
        HelpLink = entry.DocURL?.AbsoluteUri;
        Recommendation = entry.Recommendation;
        Rationale = entry.Rationale;
    }

    public RegistryEntryRequestData ToRequestData()
    {
        Guid.TryParse(InsightIdString, out Guid newId);
        Guid? newNullableId = newId == Guid.Empty ? null : newId;

        Uri.TryCreate(HelpLink, UriKind.Absolute, out Uri? helpLink);

        RegistryEntryRequestData newEntry = new()
        {
            Data = new PerfIssueRegisterEntry()
            {
                PermanentId = newNullableId,
                LegacyId = LegacyId,
                IsActive = IsActive,
                Title = Title,
                Description = Description,
                Recommendation = Recommendation,
                Rationale = Rationale,
                DocURL = helpLink,
            },
            Options = new RegistryEntryOptions
            {
                AllowsDuplicatedHelpDocs = AllowsDuplicatedHelpDocs,
            },
        };

        return newEntry;
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

    public bool AllowsDuplicatedHelpDocs { get; set; } = false;

    public bool IsActive { get; set; }

    public string? LegacyId { get; set; }

    [Required]
    public string Title { get; set; } = default!;

    [Required]
    public string Description { get; set; } = default!;

    public string? HelpLink { get; set; }

    [Required]
    public string Recommendation { get; set; } = default!;

    public string? Rationale { get; set; }
}