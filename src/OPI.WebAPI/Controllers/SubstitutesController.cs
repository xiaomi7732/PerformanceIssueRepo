using Microsoft.AspNetCore.Mvc;
using OPI.WebAPI.Services;

namespace OPI.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SubstitutesController : ControllerBase
{
    private readonly IssueItemService _issueItemService;

    public SubstitutesController(IssueItemService issueItemService)
    {
        _issueItemService = issueItemService ?? throw new ArgumentNullException(nameof(issueItemService));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> GetSubstitute([FromQuery(Name = "spec-version")] string specVersion, CancellationToken cancellationToken)
    {
        IEnumerable<string> substitutes = await _issueItemService.ListSubstitutesAsync(specVersion, cancellationToken).ConfigureAwait(false);

        if (substitutes is not null && substitutes.Any())
        {
            return Ok(substitutes);
        }
        return NoContent();
    }
}