using System.Net;
using Microsoft.AspNetCore.Mvc;
using PerfIssueRepo.Models;
using PerfIssueRepo.WebAPI.Services;

namespace PerfIssueRepo.WebAPI.Controllers;

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
    [Route("versions/{specVersion}")]
    public async Task<ActionResult<IEnumerable<PerfIssueItem>>> Get([FromRoute] string specVersion, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<PerfIssueItem> result = await _issueItemService.ListByAsync(specVersion, cancellationToken);
            return Ok(result);
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