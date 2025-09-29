namespace WordFinderApp;

/// <summary>
/// Defines a contract for finding words in a character matrix.
/// </summary>
public interface IWordFinder
{
    /// <summary>
    /// Finds words from the provided word stream in the character matrix.
    /// </summary>
    /// <param name="wordstream">A collection of words to search for in the matrix. If null or empty, returns an empty collection.</param>
    /// <returns>An enumerable of up to 10 most frequently found words. If no words are found or the input is null/empty, returns an empty collection.</returns>
    IEnumerable<string> Find(IEnumerable<string>? wordstream);
}