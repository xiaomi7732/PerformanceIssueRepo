using System.Net;
using Microsoft.AspNetCore.Mvc;
using OPI.Core.Models;
using OPI.WebAPI.HttpModels;
using OPI.WebAPI.Services;

namespace OPI.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class IssuesController : ControllerBase
{
    private readonly IssueItemService _issueItemService;
    private readonly IssueRegistryService _issueRegistryService;
    private readonly ILogger _logger;

    public IssuesController(
        IssueItemService issueItemService,
        IssueRegistryService issueRegistryService,
        ILogger<IssuesController> logger)
    {
        _issueItemService = issueItemService ?? throw new ArgumentNullException(nameof(issueItemService));
        _issueRegistryService = issueRegistryService ?? throw new ArgumentNullException(nameof(issueRegistryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PerfIssueItem>>> Get(
        [FromQuery(Name = "spec-version")] string specVersion,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.Equals(specVersion, "latest", StringComparison.OrdinalIgnoreCase))
            {
                return Ok(await GeneratePerfIssueItemsAsync(cancellationToken));
            }
            else
            {
                return Ok(await GetApprovedPerfIssueItemsAsync(specVersion, cancellationToken).ConfigureAwait(false));
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            string registryUrl = "https://github.com/xiaomi7732/PerformanceIssueRepo/tags";
            return NotFound(new Error
            {
                Message = $"Find out all available version tags at: {registryUrl}",
                HelpLink = new Uri(registryUrl),
            });
        }
    }

    [HttpGet]
    [Route("{uniqueId}")]
    public async Task<ActionResult<PerfIssueItem>> GetUnique(
        [FromQuery(Name = "spec-version")] string specVersion,
        [FromRoute] Guid uniqueId,
        CancellationToken cancellationToken)
    {
        try
        {
            PerfIssueItem? target = await _issueItemService.GetAsync(specVersion, uniqueId, cancellationToken).ConfigureAwait(false);
            if (target is null)
            {
                return NotFound(new Error
                {
                    Message = $"There's no item with unique id {uniqueId} in Registry of version {specVersion}",
                });
            }
            return Ok(target);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            string registryUrl = "https://github.com/xiaomi7732/PerformanceIssueRepo/tree/main/specs/registry";
            return NotFound(new Error
            {
                Message = $"Find out all available versions at: {registryUrl}",
                HelpLink = new Uri(registryUrl),
            });
        }
    }

    private async Task<IEnumerable<PerfIssueItem>> GeneratePerfIssueItemsAsync(CancellationToken cancellationToken)
    {
        List<PerfIssueItem> result = new List<PerfIssueItem>();
        _logger.LogInformation("Generating issues from registry:");
        await foreach (PerfIssueItem item in _issueRegistryService.GetAllIssueItems(activeState: true, cancellationToken).ConfigureAwait(false))
        {
            result.Add(item);
        }

        return result;
    }

    private Task<IEnumerable<PerfIssueItem>> GetApprovedPerfIssueItemsAsync(string specVersion, CancellationToken cancellationToken)
        => _issueItemService.ListByAsync(specVersion, cancellationToken);
}