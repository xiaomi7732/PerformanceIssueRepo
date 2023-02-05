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

    public IssuesController(IssueItemService issueItemService)
    {
        _issueItemService = issueItemService ?? throw new ArgumentNullException(nameof(issueItemService));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PerfIssueItem>>> Get(
        [FromQuery(Name = "spec-version")] string specVersion,
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<PerfIssueItem> result = await _issueItemService.ListByAsync(specVersion, cancellationToken);
            return Ok(result);
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
        [FromRoute] string uniqueId,
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
}