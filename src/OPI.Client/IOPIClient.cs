using OPI.Core.Models;

namespace OPI.Client;

public interface IAnonymousOPIClient
{
    Uri? BaseAddress { get; }

    Task<IEnumerable<string>> ExtractSubstitutes(string specVersion, CancellationToken cancellationToken);
    Task<string> GetAllInJsonStringAsync(string version, CancellationToken cancellationToken);
    Task<IEnumerable<PerfIssueItem>> ListAllAsync(string version, CancellationToken cancellationToken);
    Task<IEnumerable<string>> ListSpecVersionsAsync(CancellationToken cancellationToken);
}

public interface IAuthorizedOPIClient : IAnonymousOPIClient
{
    Task<bool> DeleteAsync(PerfIssueRegisterEntry targetEntry, CancellationToken cancellationToken);
    Task<IEnumerable<PerfIssueRegisterEntry>> ListAllRegisteredAsync(CancellationToken cancellationToken);
    Task<PerfIssueRegisterEntry> RegisterAsync(PerfIssueRegisterEntry newEntry, CancellationToken cancellationToken);
    Task<PerfIssueRegisterEntry?> ToggleActivateAsync(Guid issueId, CancellationToken cancellationToken);
    Task<PerfIssueRegisterEntry?> UpdateEntryAsync(PerfIssueRegisterEntry target, CancellationToken cancellationToken);
    void UseAccessToken(string accessToken);
}

