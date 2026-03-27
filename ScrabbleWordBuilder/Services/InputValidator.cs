using ScrabbleWordBuilder.Models;

namespace ScrabbleWordBuilder.Services;

public class InputValidator
{
    private readonly Dictionary<char, LetterInfo> _letterData;

    public InputValidator(Dictionary<char, LetterInfo> letterData)
    {
        _letterData = letterData;
    }

    public ValidationResult Validate(string rack, string? boardWord)
    {
        if (string.IsNullOrWhiteSpace(rack))
            return ValidationResult.Failure("Rack cannot be empty.");

        string rackUpper = rack.ToUpper();

        if (rackUpper.Length < 1 || rackUpper.Length > 7)
            return ValidationResult.Failure($"Rack must be between 1 and 7 letters (got {rackUpper.Length}).");

        if (!rackUpper.All(char.IsAsciiLetter))
            return ValidationResult.Failure("Rack must contain only letters A-Z.");

        // Validate board word if provided
        string boardUpper = string.Empty;
        if (!string.IsNullOrEmpty(boardWord))
        {
            boardUpper = boardWord.ToUpper();

            if (boardUpper.Length < 1)
                return ValidationResult.Failure("Board word must be at least 1 letter.");

            if (!boardUpper.All(char.IsAsciiLetter))
                return ValidationResult.Failure("Board word must contain only letters A-Z.");
        }

        // Check tile counts: for each letter, count in rack + count in board word must not exceed game tile count
        var combinedCounts = new Dictionary<char, int>();

        foreach (char c in rackUpper)
        {
            combinedCounts.TryGetValue(c, out int existing);
            combinedCounts[c] = existing + 1;
        }

        foreach (char c in boardUpper)
        {
            combinedCounts.TryGetValue(c, out int existing);
            combinedCounts[c] = existing + 1;
        }

        foreach (var kvp in combinedCounts)
        {
            char letter = kvp.Key;
            int usedCount = kvp.Value;

            if (!_letterData.TryGetValue(letter, out LetterInfo? info))
            {
                // Letter not in standard set — treat as invalid
                return ValidationResult.Failure($"Letter '{letter}' is not a valid Scrabble tile.");
            }

            if (usedCount > info.Count)
            {
                return ValidationResult.Failure(
                    $"Too many '{letter}' tiles: used {usedCount} but only {info.Count} available in the game.");
            }
        }

        return ValidationResult.Success();
    }
}
