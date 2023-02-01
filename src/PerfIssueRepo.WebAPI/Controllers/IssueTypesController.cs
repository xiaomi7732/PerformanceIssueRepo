using Microsoft.AspNetCore.Mvc;
using PerfIssueRepo.WebAPI.Services;

namespace PerfIssueRepo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class IssueTypesController : ControllerBase
{
    private readonly IssueTypeCodeService _issueTypeCodeService;

    public IssueTypesController(IssueTypeCodeService issueTypeCodeService)
    {
        _issueTypeCodeService = issueTypeCodeService ?? throw new ArgumentNullException(nameof(issueTypeCodeService));
    }

    public async Task<ActionResult<IDictionary<string, string>>> Get(CancellationToken cancellationToken)
    {
        IDictionary<string, string>? mapping = await _issueTypeCodeService.GetTypesAsync(cancellationToken);
        if (mapping is null)
        {
            return NoContent();
        }
        return Ok(mapping);
    }
}