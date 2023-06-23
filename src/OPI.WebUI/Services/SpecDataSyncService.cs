using OPI.Client;
using OPI.Core.Models;
using OPI.WebAPI.Contracts;

namespace OPI.WebUI.Services;

/// <summary>
/// Sync registry item with a published spec version
/// </summary>
public class SpecDataSyncService
{
    private IAuthorizedOPIClient _opiClient;

    public SpecDataSyncService(IAuthorizedOPIClient opiClient)
    {
        _opiClient = opiClient ?? throw new ArgumentNullException(nameof(opiClient));
    }

    public async Task RunAsync(string specVersion, RegistryEntryOptions? updatePolicy, CancellationToken cancellationToken)
    {
        List<PerfIssueRegisterEntry> lookup = (await _opiClient.ListAllRegisteredAsync(cancellationToken).ConfigureAwait(false)).ToList();

        updatePolicy ??= RegistryEntryOptions.Default;
        foreach (PerfIssueItem item in await _opiClient.ListAllAsync(specVersion, cancellationToken))
        {
            PerfIssueRegisterEntry? existItem = lookup.FirstOrDefault(existItem =>
            {
                if (existItem.PermanentId is null)
                {
                    return string.Equals(existItem.LegacyId, item.LegacyId);
                }
                return existItem.PermanentId == item.PermanentId;
            });

            RegistryEntryRequestData? request = BuildRequest(existItem, item, updatePolicy);

            if(request is null)
            {
                continue;
            }

            await _opiClient.UpdateEntryAsync(request, cancellationToken);
        }
    }

    private RegistryEntryRequestData? BuildRequest(PerfIssueRegisterEntry? oldItem, PerfIssueItem newItem, RegistryEntryOptions options)
    {
        if (oldItem is null)
        {
            return null;
        }

        PerfIssueRegisterEntry newEntry = new PerfIssueRegisterEntry(newItem)
        {
            IsActive = oldItem.IsActive,
        };

        return new RegistryEntryRequestData()
        {
            Data = newEntry,
            Options = options,
        };
    }
}