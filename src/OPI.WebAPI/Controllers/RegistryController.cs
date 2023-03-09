using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPI.Core.Models;
using OPI.WebAPI.Contracts;
using OPI.WebAPI.Services;

namespace OPI.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Admin")]
public class RegistryController : ControllerBase
{
    private readonly IssueRegistryService _issueRegistryService;
    public RegistryController(
        IssueRegistryService issueService)
    {
        _issueRegistryService = issueService ?? throw new ArgumentNullException(nameof(issueService));
    }

    [HttpGet]
    public async IAsyncEnumerable<PerfIssueRegisterEntry> GetAll([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (PerfIssueRegisterEntry entry in _issueRegistryService.GetRegisteredIssuesAsync(cancellationToken))
        {
            yield return entry;
        }
    }

    [HttpGet]
    [Route("{permanentId}")]
    public async Task<ActionResult<PerfIssueRegisterEntry>> Get([FromRoute] Guid permanentId, CancellationToken cancellationToken)
    {
        PerfIssueRegisterEntry? result = await _issueRegistryService.GetRegisteredItemAsync(permanentId, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return NotFound();
        }
        return result;
    }

    [HttpPost]
    public async Task<ActionResult<PerfIssueRegisterEntry>> RegisterNew([FromBody] RegistryEntryRequestData newItem, CancellationToken cancellationToken)
    {
        try
        {
            PerfIssueRegisterEntry result = await _issueRegistryService.RegisterNewIssueAsync(newItem.Data, newItem.Options, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(Get), new { permanentId = result.PermanentId }, result);
        }
        catch (DataModelValidationException validationException)
        {
            return Conflict(new Error() { Message = validationException.Message });
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
    public async Task<ActionResult<PerfIssueRegisterEntry>> ReplaceExists([FromBody] RegistryEntryRequestData newItem, CancellationToken cancellationToken)
    {
        try
        {
            PerfIssueRegisterEntry result = await _issueRegistryService.UpdateAsync(newItem.Data, newItem.Options, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }
        catch (DataModelValidationException validationException)
        {
            return Conflict(new Error() { Message = validationException.Message });
        }
        catch (IndexOutOfRangeException)
        {
            return NotFound();
        }
    }

    [HttpPatch()]
    [Route("{issueId}")]
    public async Task<ActionResult<PerfIssueRegisterEntry>> Activate([FromRoute] Guid issueId, CancellationToken cancellationToken)
    {
        try
        {
            PerfIssueRegisterEntry? newResult = await _issueRegistryService.FlipActivationAsync(issueId, cancellationToken).ConfigureAwait(false);
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

    [HttpDelete()]
    [Route("{issueId}")]
    public async Task<ActionResult> Delete([FromRoute] Guid issueId, CancellationToken cancellationToken)
    {
        bool deleted = await _issueRegistryService.DeleteAnIssueAsync(issueId, cancellationToken);
        if (deleted)
        {
            return Ok();
        }
        else
        {
            return NoContent();
        }
    }
}