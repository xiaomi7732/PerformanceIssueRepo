using OPI.Core.Models;
using OPI.WebAPI.Contracts;

namespace OPI.Client;

public interface IAnonymousOPIClient
{
    Uri? BaseAddress { get; }

    Task<IEnumerable<string>> ExtractSubstitutes(string specVersion, CancellationToken cancellationToken);
    Task<string> GetAllInJsonStringAsync(string version, CancellationToken cancellationToken);
    Task<IEnumerable<PerfIssueItem>> ListAllAsync(string version, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a perf issue item by its id and version.
    /// </summary>
    /// <param name="id">Permanent id or legacy id.</param>
    /// <param name="version">The spec version.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<PerfIssueItem?> GetPerfIssueItem(Guid issueId, string version, CancellationToken cancellationToken);

    Task<IEnumerable<string>> ListSpecVersionsAsync(CancellationToken cancellationToken);
}

public interface IAuthorizedOPIClient : IAnonymousOPIClient
{
    Task<bool> DeleteAsync(PerfIssueRegisterEntry targetEntry, CancellationToken cancellationToken);
    Task<IEnumerable<PerfIssueRegisterEntry>> ListAllRegisteredAsync(CancellationToken cancellationToken);
    Task<PerfIssueRegisterEntry> RegisterAsync(RegistryEntryRequestData newEntry, CancellationToken cancellationToken);
    Task<PerfIssueRegisterEntry?> ToggleActivateAsync(Guid issueId, CancellationToken cancellationToken);
    Task<PerfIssueRegisterEntry?> UpdateEntryAsync(RegistryEntryRequestData requestData, CancellationToken cancellationToken);
    void UseAccessToken(string accessToken);
}

