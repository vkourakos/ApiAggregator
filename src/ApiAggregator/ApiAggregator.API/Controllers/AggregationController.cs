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
    [FromQuery] SortField sortBy = SortField.PublishedDate,
    [FromQuery] SortOrder sortOrder = SortOrder.Descending,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Query parameter cannot be empty.");
        }

        var aggregatedData = await _aggregationService.AggregateData(query, cancellationToken);
        var sortedData = SortData(aggregatedData, sortBy, sortOrder);

        return Ok(sortedData);
    }

    private static IEnumerable<AggregatedData> SortData(
        IEnumerable<AggregatedData> data,
        SortField sortBy,
        SortOrder sortOrder)
    {
        var sorted = sortBy switch
        {
            SortField.Title => sortOrder == SortOrder.Ascending
                ? data.OrderBy(d => d.Title, StringComparer.OrdinalIgnoreCase)
                : data.OrderByDescending(d => d.Title, StringComparer.OrdinalIgnoreCase),

            SortField.SourceApi => sortOrder == SortOrder.Ascending
                ? data.OrderBy(d => d.SourceApi, StringComparer.OrdinalIgnoreCase)
                : data.OrderByDescending(d => d.SourceApi, StringComparer.OrdinalIgnoreCase),

            _ => sortOrder == SortOrder.Ascending
                ? data.OrderBy(d => d.PublishedDate)
                : data.OrderByDescending(d => d.PublishedDate)
        };

        return sorted.ThenBy(d => d.Title);
    }
}
