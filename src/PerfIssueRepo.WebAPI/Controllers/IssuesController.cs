using Microsoft.AspNetCore.Mvc;
using PerfIssueRepo.Models;
using PerfIssueRepo.WebAPI.Services;

namespace PerfIssueRepo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class IssuesController : ControllerBase
{
    private readonly IssueService _issueService;

    public IssuesController(IssueService issueService)
    {
        _issueService = issueService ?? throw new ArgumentNullException(nameof(issueService));
    }

    [HttpGet]
    public IAsyncEnumerable<PerfIssueItem> Get(CancellationToken cancellationToken) => _issueService.GetAllIssueItems(activeState: true, cancellationToken);
}