using System.Text.Json;
using ScrabbleWordBuilder.Models;

namespace ScrabbleWordBuilder.Services;

public class LetterDataService
{
    private readonly string _dataPath;

    public LetterDataService(string dataPath)
    {
        _dataPath = dataPath;
    }

    public Dictionary<char, LetterInfo> LoadLetterData()
    {
        string jsonPath = Path.Combine(_dataPath, "letter_data.json");

        if (!File.Exists(jsonPath))
            throw new FileNotFoundException($"letter_data.json not found at: {jsonPath}");

        string json = File.ReadAllText(jsonPath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new LetterCharConverter() }
        };

        var letterData = JsonSerializer.Deserialize<LetterData>(json, options)
            ?? throw new InvalidOperationException("Failed to deserialize letter_data.json");

        var result = new Dictionary<char, LetterInfo>();
        foreach (var info in letterData.Letters)
        {
            result[char.ToUpper(info.Letter)] = info;
        }

        return result;
    }
}
