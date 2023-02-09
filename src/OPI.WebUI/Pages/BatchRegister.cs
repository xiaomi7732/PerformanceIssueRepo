using System.Text;
using Microsoft.AspNetCore.Components;
using NReco.Csv;
using OPI.Client;
using OPI.Core.Models;
using OPI.WebUI.ViewModels;

namespace OPI.WebUI.Pages;

public partial class BatchRegister
{
    [Inject]
    public OPIClient OpiClient { get; private set; } = default!;

    [Inject]
    public CSVReaderFactory CsvReaderFactory { get; private set; } = default!;

    public BatchContent Content { get; } = new BatchContent();

    public IEnumerable<string> ErrorMessages { get; private set; } = Enumerable.Empty<string>();

    private async Task HandleSubmitAsync()
    {
        if (string.IsNullOrEmpty(Content.Value))
        {
            return;
        }

        using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Content.Value)))
        using (StreamReader inputStream = new StreamReader(memoryStream))
        {
            CsvReader reader = CsvReaderFactory.CreateCSVReader(inputStream);
            while (reader.Read())
            {
                await ProcessLineAsync(reader, default);
            }
        }

        Content.Value = null;
        StateHasChanged();
    }

    private async Task ProcessLineAsync(CsvReader lineReader, CancellationToken cancellationToken)
    {
        int fieldCount = lineReader.FieldsCount;
        if (fieldCount < 4)
        {
            throw new InvalidCastException("There should be at least 4 fields.");
        }

        string? legacyId = lineReader[0];
        string title = lineReader[1] ?? throw new InvalidCastException("Title is required");
        string description = lineReader[2] ?? throw new InvalidCastException("Description is required");
        string recommendation = lineReader[3] ?? throw new InvalidCastException("Recommendation is required");

        string? rationale = null;
        if (fieldCount >= 4)
        {
            rationale = lineReader[4];
        }

        string? docUrlString = null;
        if (fieldCount >= 5)
        {
            docUrlString = lineReader[5];
        }

        Guid guid;
        if (Guid.TryParse(legacyId, out guid))
        {
            legacyId = string.Empty;
        }
        else
        {
            guid = Guid.NewGuid();
        }

        PerfIssueRegisterEntry newEntry = new PerfIssueRegisterEntry()
        {
            IsActive = false,
            LegacyId = string.IsNullOrEmpty(legacyId) ? null : legacyId,
            PermanentId = guid,
            Title = title,
            Description = description,
            Recommendation = recommendation,
            Rationale = rationale,
            DocURL = string.IsNullOrEmpty(docUrlString) ? null : new Uri(docUrlString),
        };
        await OpiClient.RegisterAsync(newEntry, default);
    }
}