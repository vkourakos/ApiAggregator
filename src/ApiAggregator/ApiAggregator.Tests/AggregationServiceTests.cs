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
    private readonly Mock<IApiClient> _gitHubClientMock;
    private readonly Mock<IApiClient> _newsApiClientMock;
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

        _gitHubClientMock = new Mock<IApiClient>();
        _gitHubClientMock.Setup(c => c.SourceName).Returns("GitHub");
        _gitHubClientMock.Setup(c => c.GetData(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new List<AggregatedData> { _gitHubData });

        _newsApiClientMock = new Mock<IApiClient>();
        _newsApiClientMock.Setup(c => c.SourceName).Returns("NewsAPI");
        _newsApiClientMock.Setup(c => c.GetData(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new List<AggregatedData> { _newsData });

        _allClients = new List<IApiClient> { _gitHubClientMock.Object, _newsApiClientMock.Object };
    }

    [Fact]
    public async Task AggregateData_WhenCacheIsMiss_ShouldFetchFromClientsAndCacheRawData()
    {
        var service = new AggregationService(_allClients, _loggerMock.Object, _memoryCache);
        var query = "test_miss";
        var cacheKey = $"raw_aggregation_result_{query}";

        var result = await service.AggregateData(
            query,
            SortField.PublishedDate,
            SortOrder.Descending,
            null, CancellationToken.None);

        _gitHubClientMock.Verify(c => c.GetData(
            query, It.IsAny<CancellationToken>()), Times.Once);
        _newsApiClientMock.Verify(c => c.GetData(
            query, It.IsAny<CancellationToken>()), Times.Once);

        _memoryCache.TryGetValue(
            cacheKey, out List<AggregatedData>? cachedResult).Should().BeTrue();
        cachedResult.Should().HaveCount(2);
        cachedResult.Should().ContainEquivalentOf(_gitHubData);
        cachedResult.Should().ContainEquivalentOf(_newsData);

        result.First().SourceApi.Should().Be("NewsAPI");
    }

    [Fact]
    public async Task AggregateData_WhenCacheIsHit_ShouldUseCachedDataAndNotCallClients()
    {
        var service = new AggregationService(_allClients, _loggerMock.Object, _memoryCache);
        var query = "test_hit";
        var cacheKey = $"raw_aggregation_result_{query}";

        var rawDataToCache = new List<AggregatedData> { _gitHubData, _newsData };
        _memoryCache.Set(cacheKey, rawDataToCache);

        var result = await service.AggregateData(
            query,
            SortField.Title,
            SortOrder.Ascending,
            null,
            CancellationToken.None);

        _gitHubClientMock.Verify(c => c.GetData(
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _newsApiClientMock.Verify(c => c.GetData(
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        result.Should().HaveCount(2);
        result.First().Title.Should().Be("B News");
    }

    [Fact]
    public async Task AggregateData_WhenCacheIsHit_ShouldStillApplyFiltering()
    {
        var service = new AggregationService(_allClients, _loggerMock.Object, _memoryCache);
        var query = "test_hit_filter";
        var cacheKey = $"raw_aggregation_result_{query}";

        var rawDataToCache = new List<AggregatedData> { _gitHubData, _newsData };
        _memoryCache.Set(cacheKey, rawDataToCache);

        var result = await service.AggregateData(
            query,
            SortField.PublishedDate,
            SortOrder.Descending,
            "GitHub",
            CancellationToken.None);

        _gitHubClientMock.Verify(c => c.GetData(
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        result.Should().HaveCount(1);
        result.First().SourceApi.Should().Be("GitHub");
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
    }
}