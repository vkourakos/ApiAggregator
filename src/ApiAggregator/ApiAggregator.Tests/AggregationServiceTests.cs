using ApiAggregator.Application.Interfaces;
using ApiAggregator.Domain;
using ApiAggregator.Domain.Enumerations;
using ApiAggregator.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiAggregator.Tests;

public class AggregationServiceTests : IDisposable
{
    private readonly Mock<ILogger<AggregationService>> _loggerMock;
    private readonly MemoryCache _memoryCache;
    private readonly List<IApiClient> _allClients;

    private readonly AggregatedData _gitHubData = new()
    {
        SourceApi = "GitHub",
        Title = "C Repo",
        PublishedDate = DateTime.UtcNow.AddDays(-1)
    };

    private readonly AggregatedData _newsData = new()
    {
        SourceApi = "NewsAPI",
        Title = "B News",
        PublishedDate = DateTime.UtcNow
    };

    public AggregationServiceTests()
    {
        _loggerMock = new Mock<ILogger<AggregationService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        var gitHubClientMock = new Mock<IApiClient>();
        gitHubClientMock.Setup(c => c.SourceName).Returns("GitHub");
        gitHubClientMock.Setup(c => c.GetData(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<AggregatedData> { _gitHubData });

        var newsApiClientMock = new Mock<IApiClient>();
        newsApiClientMock.Setup(c => c.SourceName).Returns("NewsAPI");
        newsApiClientMock.Setup(c => c.GetData(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new List<AggregatedData> { _newsData });

        _allClients = new List<IApiClient> { gitHubClientMock.Object, newsApiClientMock.Object };
    }

    [Fact]
    public async Task AggregateData_WithNoFilteringOrSorting_ReturnsAllDataSortedByDateDescending()
    {
        var service = new AggregationService(_allClients, _loggerMock.Object, _memoryCache);

        var result = await service.AggregateData(
            "test",
            SortField.PublishedDate,
            SortOrder.Descending,
            null,
            CancellationToken.None);

        result.Should().HaveCount(2);
        result.First().SourceApi.Should().Be("NewsAPI");
    }

    [Fact]
    public async Task AggregateData_WhenFilteredBySource_ReturnsOnlyMatchingData()
    {
        var service = new AggregationService(_allClients, _loggerMock.Object, _memoryCache);

        var result = await service.AggregateData(
            "test",
            SortField.PublishedDate,
            SortOrder.Descending,
            "GitHub",
            CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().SourceApi.Should().Be("GitHub");
    }

    [Fact]
    public async Task AggregateData_WhenSortedByTitleAscending_ReturnsCorrectlyOrderedData()
    {
        var service = new AggregationService(_allClients, _loggerMock.Object, _memoryCache);

        var result = await service.AggregateData(
            "test", SortField.Title, SortOrder.Ascending, null, CancellationToken.None);

        result.Should().HaveCount(2);
        result.First().Title.Should().Be("B News");
    }

    [Fact]
    public async Task AggregateData_WhenCacheIsHit_ReturnsCachedData()
    {
        var service = new AggregationService(_allClients, _loggerMock.Object, _memoryCache);
        var query = "test_cache_hit";
        var sources = "NewsAPI";
        var sortBy = SortField.Title;
        var sortOrder = SortOrder.Ascending;

        var cacheKey = $"agg_result_{query}_{sources}_{sortBy}_{sortOrder}";

        var dataToCache = new List<AggregatedData> { new() { Title = "Cached Data" } };
        _memoryCache.Set(cacheKey, dataToCache);

        var result = await service.AggregateData(
            query,
            sortBy,
            sortOrder,
            sources,
            CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Cached Data");
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
    }
}