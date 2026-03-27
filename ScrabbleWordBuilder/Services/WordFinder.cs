using ScrabbleWordBuilder.Models;

namespace ScrabbleWordBuilder.Services;

public class WordFinder
{
    private readonly string _dataPath;
    private readonly Dictionary<char, LetterInfo> _letterData;
    private List<string>? _dictionary;

    public WordFinder(string dataPath, Dictionary<char, LetterInfo> letterData)
    {
        _dataPath = dataPath;
        _letterData = letterData;
    }

    private List<string> LoadDictionary()
    {
        if (_dictionary != null)
            return _dictionary;

        string dictPath = Path.Combine(_dataPath, "dictionary.txt");

        if (!File.Exists(dictPath))
            throw new FileNotFoundException($"dictionary.txt not found at: {dictPath}");

        // TODO: consider caching this to disk if startup time becomes an issue with larger word lists
        _dictionary = File.ReadAllLines(dictPath)
            .Select(line => line.Trim().ToUpper())
            .Where(word => word.Length >= 2 && word.Length <= 15 && word.All(char.IsAsciiLetter))
            .Distinct()
            .ToList();

        return _dictionary;
    }

    public string? FindBestWord(string rack, string? boardWord)
    {
        var dictionary = LoadDictionary();

        // Build the combined pool of available letters
        string combined = rack.ToUpper() + (boardWord?.ToUpper() ?? string.Empty);
        var poolCounts = BuildLetterCounts(combined);

        string? bestWord = null;
        int bestScore = -1;

        foreach (string word in dictionary)
        {
            if (!CanForm(word, poolCounts))
                continue;

            int score = CalculateScore(word);

            if (score > bestScore || (score == bestScore && string.Compare(word, bestWord, StringComparison.Ordinal) < 0))
            {
                bestScore = score;
                bestWord = word;
            }
        }

        return bestWord;
    }

    private static bool CanForm(string word, Dictionary<char, int> poolCounts)
    {
        var needed = BuildLetterCounts(word);

        foreach (var kvp in needed)
        {
            if (!poolCounts.TryGetValue(kvp.Key, out int available) || available < kvp.Value)
                return false;
        }

        return true;
    }

    private int CalculateScore(string word)
    {
        int score = 0;
        foreach (char c in word)
        {
            if (_letterData.TryGetValue(c, out LetterInfo? info))
                score += info.Score;
        }
        return score;
    }

    private static Dictionary<char, int> BuildLetterCounts(string letters)
    {
        var counts = new Dictionary<char, int>();
        foreach (char c in letters)
        {
            counts.TryGetValue(c, out int existing);
            counts[c] = existing + 1;
        }
        return counts;
    }
}
