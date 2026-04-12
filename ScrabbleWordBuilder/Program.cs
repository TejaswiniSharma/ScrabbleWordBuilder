using ScrabbleWordBuilder.Services;

namespace ScrabbleWordBuilder;

class Program
{
    private static InputValidator? _validator;
    private static WordFinder? _wordFinder;

    static int Main(string[] args)
    {
        // Locate the Data directory
        string? dataPath = FindDataDirectory();
        if (dataPath == null)
        {
            Console.Error.WriteLine("Error: Could not locate the Data directory.");
            return 1;
        }

        // Load letter data
        var letterDataService = new LetterDataService(dataPath);
        Dictionary<char, Models.LetterInfo> letterData;
        try
        {
            letterData = letterDataService.LoadLetterData();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading letter data: {ex.Message}");
            return 1;
        }

        _validator = new InputValidator(letterData);
        _wordFinder = new WordFinder(dataPath, letterData);

        string? rack = null;
        string? boardWord = null;

        if (args.Length > 0)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("--rack", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                    rack = args[++i];
                else if (args[i].Equals("--word", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                    boardWord = args[++i];
            }

            if (string.IsNullOrWhiteSpace(rack))
            {
                Console.Error.WriteLine("Invalid input: --rack is required.");
                return 1;
            }
        }

        Console.WriteLine("Scrabble Word Builder — press Ctrl+C to exit.");
        while (true)
        {
            Console.WriteLine();

            Console.Write("Enter your rack letters (1-7 letters): ");
            string? newRack = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(newRack))
                rack = newRack;

            Console.Write("Enter board word (optional, press Enter to skip): ");
            string? newBoard = Console.ReadLine()?.Trim();
            boardWord = string.IsNullOrWhiteSpace(newBoard) ? null : newBoard;

            Console.WriteLine(ResolveWord(rack!, boardWord));
        }
    }

    private static string ResolveWord(string rack, string? boardWord)
    {
        var validation = _validator!.Validate(rack, boardWord);
        if (!validation.IsValid)
            return $"Invalid input: {validation.ErrorMessage}";

        try
        {
            string? bestWord = _wordFinder!.FindBestWord(rack, boardWord);
            return bestWord == null ? "No valid word found." : bestWord.ToUpper();
        }
        catch (Exception ex)
        {
            return $"Error finding word: {ex.Message}";
        }
    }

    private static string? FindDataDirectory()
    {
        // walk up in case we're running from a nested build output folder
        string? startDir = Path.GetDirectoryName(AppContext.BaseDirectory);

        if (startDir == null)
            startDir = Directory.GetCurrentDirectory();

        string candidate = Path.Combine(startDir, "Data");
        if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "dictionary.txt")))
            return candidate;

        DirectoryInfo? current = new DirectoryInfo(startDir);
        while (current != null)
        {
            candidate = Path.Combine(current.FullName, "Data");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "dictionary.txt")))
                return candidate;

            current = current.Parent;
        }

        return null;
    }
}
