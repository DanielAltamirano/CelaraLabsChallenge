namespace WordFinderApp.Radix;

/// <summary>
/// Represents the result of a search operation in a Radix Tree.
/// </summary>
public class RadixSearchResult
{
    /// <summary>
    /// Gets or sets the list of words found during the search.
    /// </summary>
    public List<RadixMatch> FoundWords { get; set; } = [];

    /// <summary>
    /// Gets or sets the number of nodes traversed during the search.
    /// </summary>
    public int NodesTraversed { get; set; }

    /// <summary>
    /// Gets or sets the number of characters processed during the search.
    /// </summary>
    public int CharactersProcessed { get; set; }
}