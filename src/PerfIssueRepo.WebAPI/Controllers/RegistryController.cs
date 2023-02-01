using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using PerfIssueRepo.Models;
using PerfIssueRepo.WebAPI.RequestModels;
using PerfIssueRepo.WebAPI.Services;

namespace PerfIssueRepo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class RegistryController : ControllerBase
{
    private readonly IssueService _issueService;

    public RegistryController(IssueService issueService)
    {
        _issueService = issueService ?? throw new ArgumentNullException(nameof(issueService));
    }

    [HttpGet]
    public async IAsyncEnumerable<PerfIssueRegisterEntry> GetAll([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (PerfIssueRegisterEntry entry in _issueService.GetAllIssuesAsync(cancellationToken))
        {
            yield return entry;
        }
    }

    [HttpGet]
    [Route("{issueId}")]
    public async Task<ActionResult<PerfIssueRegisterEntry>> Get([FromRoute] int issueId, CancellationToken cancellationToken)
    {
        PerfIssueRegisterEntry? result = await _issueService.GetRegisteredItem(issueId, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return NotFound();
        }
        return result;
    }

    [HttpPost]
    public async Task<ActionResult<PerfIssueRegisterEntry>> RegisterNew([FromBody] PerfIssueRegisterEntry newItem, CancellationToken cancellationToken)
    {
        try
        {
            PerfIssueRegisterEntry result = await _issueService.RegisterNewIssueAsync(newItem, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(Get), new { issueId = result.Id }, result);
        }
        catch (InvalidOperationException)
        {
            return BadRequest();
        }
        catch (Exception)
        {
            return Conflict();
        }
    }

    [HttpPut]
    public async Task<ActionResult<PerfIssueRegisterEntry>> ReplaceExists([FromBody] PerfIssueRegisterEntry newItem, CancellationToken cancellationToken)
    {
        try
        {
            PerfIssueRegisterEntry result = await _issueService.UpdateAsync(newItem, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }
        catch (IndexOutOfRangeException)
        {
            return NotFound();
        }
    }

    [HttpPatch()]
    [Route("{issueId}")]
    public async Task<ActionResult<PerfIssueRegisterEntry>> Activate([FromRoute] int issueId, CancellationToken cancellationToken)
    {
        try
        {
            PerfIssueRegisterEntry? newResult = await _issueService.FlipActivationAsync(issueId, cancellationToken).ConfigureAwait(false);
            if (newResult is null)
            {
                // Found the item, but no update was needed
                return new StatusCodeResult(304); // Not modified.
            }
            return Ok(newResult);
        }
        catch (IndexOutOfRangeException)
        {
            return NotFound();
        }
    }
}