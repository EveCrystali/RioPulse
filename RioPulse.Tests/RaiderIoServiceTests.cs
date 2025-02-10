using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using RioPulse.Core.Models;
using RioPulse.Core.Services;

public class RaiderIoServiceTests
{
    private readonly HttpClient _httpClient;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly RaiderIoService _raiderIoService;

    public RaiderIoServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://raider.io/api/v1/")
        };

        _raiderIoService = new RaiderIoService(_httpClient);
    }

    [Fact]
    public async Task GetCharacterDataAsync_ShouldReturnCharacter_WhenApiResponseIsValid()
    {
        // Arrange
        var fakeCharacter = new Character
        {
            Name = "TestCharacter",
            Race = "Orc",
            Class = "Warlock",
            Region = "us",
            Realm = "Skullcrusher",
            ProfileUrl = "https://raider.io/characters/us/skullcrusher/TestCharacter"
        };

        var jsonResponse = JsonSerializer.Serialize(fakeCharacter);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                                              "SendAsync",
                                              ItExpr.IsAny<HttpRequestMessage>(),
                                              ItExpr.IsAny<CancellationToken>()
                                             )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _raiderIoService.GetCharacterDataAsync("Skullcrusher", "TestCharacter", "us");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestCharacter", result.Name);
        Assert.Equal("Orc", result.Race);
        Assert.Equal("Warlock", result.Class);
    }

    [Fact]
    public async Task GetCharacterDataAsync_ShouldReturnNull_WhenApiReturnsNotFound()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                                              "SendAsync",
                                              ItExpr.IsAny<HttpRequestMessage>(),
                                              ItExpr.IsAny<CancellationToken>()
                                             )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var result = await _raiderIoService.GetCharacterDataAsync("Skullcrusher", "UnknownCharacter", "us");

        // Assert
        Assert.Null(result);
    }
}