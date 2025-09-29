namespace WordFinderApp;

/// <summary>
/// Provides methods to load the matrix and word stream from files.
/// </summary>
public static class FileLoader
{
    /// <summary>
    /// Loads the matrix from a file, where each line represents a row of characters.
    /// </summary>
    /// <param name="filePath">The path to the matrix file.</param>
    /// <returns>A list of strings representing the matrix rows.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the matrix file does not exist.</exception>
    public static IList<string> LoadMatrix(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Matrix file not found: {filePath}");
        }

        return File.ReadAllLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
    }

    /// <summary>
    /// Loads the word stream from a file, splitting words by spaces, commas, or tabs.
    /// </summary>
    /// <param name="filePath">The path to the word stream file.</param>
    /// <returns>A list of words from the file.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the word stream file does not exist.</exception>
    public static IList<string> LoadWordStream(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Word stream file not found: {filePath}");
        }

        return File.ReadAllLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .SelectMany(line => line.Split([' ', ',', '\t'],
                StringSplitOptions.RemoveEmptyEntries))
            .ToList();
    }
}