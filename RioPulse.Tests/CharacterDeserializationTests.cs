using System.Text.Json;
using Xunit;
using RioPulse.Models;
using System.IO;
using System.Threading.Tasks;

public class CharacterDeserializationTests
{
    private async Task<string> LoadJsonTestFile(string fileName)
    {
        return await File.ReadAllTextAsync(fileName);
    }

    [Fact]
    public async Task Character_Deserialization_ShouldWork()
    {
        // Arrange
        string json = await LoadJsonTestFile("TestData/Example_Api_Characters_Profile.json");

        // Act
        Character? character = JsonSerializer.Deserialize<Character>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Assert
        Assert.NotNull(character);
        Assert.Equal("Ulsoga", character.Name);
        Assert.Equal("Orc", character.Race);
        Assert.Equal("Warlock", character.Class);
        Assert.NotNull(character.RaidProgression);
        Assert.True(character.RaidProgression.ContainsKey("amirdrassil-the-dreams-hope"));
    }
}
