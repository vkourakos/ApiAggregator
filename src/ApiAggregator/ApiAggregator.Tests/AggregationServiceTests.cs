using ApiAggregator.Application.Interfaces;
using ApiAggregator.Domain;
using ApiAggregator.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiAggregator.Tests;

public class AggregationServiceTests
{
    private readonly Mock<ILogger<AggregationService>> _loggerMock;
    private readonly Mock<IApiClient> _successfulClientMock;
    private readonly Mock<IApiClient> _failingClientMock;
    private readonly Mock<IApiClient> _emptyClientMock;

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

        _emptyClientMock = new Mock<IApiClient>();
        _emptyClientMock
            .Setup(c => c.GetData(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AggregatedData>());
        _emptyClientMock.Setup(c => c.SourceName).Returns("EmptyAPI");
    }

    [Fact]
    public async Task AggregateData_WhenOneClientFails_ShouldReturnDataFromSuccessfulClients()
    {
        var clients = new List<IApiClient> { _successfulClientMock.Object, _failingClientMock.Object };
        var service = new AggregationService(clients, _loggerMock.Object);

        var result = await service.AggregateData("test", CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Success Data");

        _loggerMock.Verify(
            log => log.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error fetching data from FailingAPI")),
                It.IsAny<HttpRequestException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task AggregateData_WithAllClientsSucceeding_ShouldReturnAllAggregatedData()
    {
        var clients = new List<IApiClient> { _successfulClientMock.Object, _emptyClientMock.Object };
        var service = new AggregationService(clients, _loggerMock.Object);

        var result = await service.AggregateData("test", CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
