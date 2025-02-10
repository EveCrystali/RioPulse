using System.Text.Json;
using RioPulse.Core.Models;

public class CharacterDeserializationTests
{
    private static async Task<string> LoadJsonTestFile(string fileName)
    {
        return await File.ReadAllTextAsync(fileName);
    }

    [Fact]
    public async Task Character_Deserialization_ShouldWork()
    {
        // Arrange
        string json = await LoadJsonTestFile("J:/CSharp/RioPulse/Example_Api_Characters_Profile.txt");

        // Act
        Character? character = JsonSerializer.Deserialize<Character>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(character);
        Assert.Equal("Ulsoga", character.Name);
        Assert.Equal("Orc", character.Race);
        Assert.Equal("Warlock", character.Class);
        Assert.NotNull(character.RaidProgression);
        Assert.True(character.RaidProgression.ContainsKey("amirdrassil-the-dreams-hope"));
    }
}