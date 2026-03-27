using ScrabbleWordBuilder.Services;

namespace ScrabbleWordBuilder;

class Program
{
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

        var validator = new InputValidator(letterData);
        var wordFinder = new WordFinder(dataPath, letterData);

        string? rack = null;
        string? boardWord = null;

        if (args.Length == 0)
        {
            // Interactive mode
            Console.Write("Enter your rack letters (1-7 letters): ");
            rack = Console.ReadLine()?.Trim();

            Console.Write("Enter board word (optional, press Enter to skip): ");
            string? boardInput = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(boardInput))
                boardWord = boardInput;
        }
        else
        {
            // CLI argument mode
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("--rack", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    rack = args[++i];
                }
                else if (args[i].Equals("--word", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    boardWord = args[++i];
                }
            }

            if (string.IsNullOrWhiteSpace(rack))
            {
                Console.Error.WriteLine("Invalid input: --rack is required.");
                return 1;
            }
        }

        // Validate input
        var validation = validator.Validate(rack!, boardWord);
        if (!validation.IsValid)
        {
            Console.WriteLine($"Invalid input: {validation.ErrorMessage}");
            return 1;
        }

        // Find the best word
        string? bestWord;
        try
        {
            bestWord = wordFinder.FindBestWord(rack!, boardWord);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error finding word: {ex.Message}");
            return 1;
        }

        if (bestWord == null)
        {
            Console.WriteLine("No valid word found.");
            return 0;
        }

        Console.WriteLine(bestWord.ToUpper());
        return 0;
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
