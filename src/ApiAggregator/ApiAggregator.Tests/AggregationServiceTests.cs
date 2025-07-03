using ApiAggregator.Application.Interfaces;
using ApiAggregator.Domain;
using ApiAggregator.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiAggregator.Tests;

public class AggregationServiceTests : IDisposable
{
    private readonly Mock<ILogger<AggregationService>> _loggerMock;
    private readonly Mock<IApiClient> _successfulClientMock;
    private readonly Mock<IApiClient> _failingClientMock;
    private readonly MemoryCache _memoryCache;

    public AggregationServiceTests()
    {
        _loggerMock = new Mock<ILogger<AggregationService>>();

        _successfulClientMock = new Mock<IApiClient>();
        _successfulClientMock
            .Setup(c => c.GetData(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AggregatedData> { new() { Title = "Success Data" } });
        _successfulClientMock.Setup(c => c.SourceName).Returns("SuccessAPI");

        _failingClientMock = new Mock<IApiClient>();
        _failingClientMock
            .Setup(c => c.GetData(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("API is down"));
        _failingClientMock.Setup(c => c.SourceName).Returns("FailingAPI");

        _memoryCache = new MemoryCache(new MemoryCacheOptions());
    }

    [Fact]
    public async Task AggregateData_WhenCacheIsMiss_ShouldFetchFromClientsAndSetCache()
    {
        var clients = new List<IApiClient> { _successfulClientMock.Object };
        var service = new AggregationService(clients, _loggerMock.Object, _memoryCache);
        var query = "test";
        var cacheKey = $"aggregation_result_{query}";

        var result = await service.AggregateData(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Success Data");

        _successfulClientMock.Verify(c => c.GetData(
            query, It.IsAny<CancellationToken>()), Times.Once);

        _memoryCache.TryGetValue(cacheKey, out var cachedResult).Should().BeTrue();
        cachedResult.Should().BeEquivalentTo(result);
    }

    [Fact]
    public async Task AggregateData_WhenCacheIsHit_ShouldReturnCachedDataAndNotCallClients()
    {
        var clients = new List<IApiClient> { _successfulClientMock.Object };
        var service = new AggregationService(clients, _loggerMock.Object, _memoryCache);
        var query = "test";
        var cacheKey = $"aggregation_result_{query}";
        var dataToCache = new List<AggregatedData> { new() { Title = "Cached Data" } };

        _memoryCache.Set(cacheKey, dataToCache);

        var result = await service.AggregateData(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Cached Data");

        _successfulClientMock.Verify(c => c.GetData(
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AggregateData_WhenOneClientFails_ShouldStillCacheSuccessfulResults()
    {
        // Arrange
        var clients = new List<IApiClient> { _successfulClientMock.Object, _failingClientMock.Object };
        var service = new AggregationService(clients, _loggerMock.Object, _memoryCache);
        var query = "test";
        var cacheKey = $"aggregation_result_{query}";

        var result = await service.AggregateData(query, CancellationToken.None);

        result.Should().HaveCount(1);

        _memoryCache.TryGetValue(cacheKey, out var cachedResult).Should().BeTrue();
        cachedResult.Should().BeEquivalentTo(result);
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
    }
}