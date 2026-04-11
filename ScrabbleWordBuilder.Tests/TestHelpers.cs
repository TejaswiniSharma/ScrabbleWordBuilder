namespace ScrabbleWordBuilder.Tests;

public static class TestHelpers
{
    /// <summary>
    /// Walks up from the test binary directory to find the Data/ folder.
    /// </summary>
    public static string GetDataPath()
    {
        DirectoryInfo? dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            string candidate = Path.Combine(dir.FullName, "ScrabbleWordBuilder", "Data");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "dictionary.txt")))
                return candidate;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("Could not find Data/ directory for tests.");
    }
}
