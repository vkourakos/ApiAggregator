using ApiAggregator.Application.Interfaces;
using ApiAggregator.Domain;
using ApiAggregator.Domain.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiAggregator.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting("default")]
public class AggregationController : ControllerBase
{
    private readonly IAggregationService _aggregationService;

    public AggregationController(IAggregationService aggregationService)
    {
        _aggregationService = aggregationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AggregatedData>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Get(
    [FromQuery] string query,
    [FromQuery] string? sources = null,
    [FromQuery] SortField sortBy = SortField.PublishedDate,
    [FromQuery] SortOrder sortOrder = SortOrder.Descending,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Query parameter cannot be empty.");
        }

        var result = await _aggregationService.AggregateData(
            query, sortBy, sortOrder, sources, cancellationToken);

        return Ok(result);
    }
}
