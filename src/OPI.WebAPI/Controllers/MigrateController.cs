#if DEBUG
using Microsoft.AspNetCore.Mvc;
using OPI.Core.Models;
using OPI.WebAPI.Services;

namespace OPI.WebAPI.Controllers;


[ApiController]
[Route("[controller]")]
public class MigrateController : ControllerBase
{
    private readonly IssueRegistryService _registryService;
    private readonly LegacyIssueRegistryService _legacyIssueRegistryService;

    public MigrateController(
        IssueRegistryService registryService,
        LegacyIssueRegistryService legacyIssueRegistryService)
    {
        _registryService = registryService ?? throw new ArgumentNullException(nameof(registryService));
        _legacyIssueRegistryService = legacyIssueRegistryService ?? throw new ArgumentNullException(nameof(legacyIssueRegistryService));
    }

    [HttpPost]
    public async Task Migrate(CancellationToken cancellationToken)
    {
        await foreach (PerfIssueRegisterEntry entry in _legacyIssueRegistryService.GetRegisteredIssuesAsync(cancellationToken).ConfigureAwait(false))
        {
            PerfIssueRegisterEntry edit = entry with
            {
                Rationale = @"{criteria}% of your {issueCategory} was spent in `{symbol}`, We expected this value to be {relation} {value}%.",
            };
            await _registryService.SaveRegistryItemAsync(edit, cancellationToken).ConfigureAwait(false);
        }
    }
}
#endif