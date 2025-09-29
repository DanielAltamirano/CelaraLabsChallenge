namespace WordFinderApp;

/// <summary>
/// Stores performance statistics for word search in a specific direction.
/// </summary>
public class DirectionStats
{
    /// <summary>
    /// Gets or sets the direction of the search (horizontal or vertical).
    /// </summary>
    public Direction Direction { get; set; }

    /// <summary>
    /// Gets or sets the number of words found in this direction.
    /// </summary>
    public int WordsFound { get; set; }

    /// <summary>
    /// Gets or sets the number of positions searched in this direction.
    /// </summary>
    public int PositionsSearched { get; set; }

    /// <summary>
    /// Gets or sets the elapsed time in milliseconds for the search in this direction.
    /// </summary>
    public long ElapsedMilliseconds { get; set; }

    /// <summary>
    /// Gets or sets the thread ID that performed the search.
    /// </summary>
    public int ThreadId { get; set; }

    /// <summary>
    /// Gets the number of words found per second in this direction.
    /// </summary>
    public double WordsPerSecond => WordsFound > 0 ? WordsFound * 1000.0 / ElapsedMilliseconds : 0;

    /// <summary>
    /// Gets the number of positions searched per second in this direction.
    /// </summary>
    public double PositionsPerSecond => PositionsSearched > 0 ? PositionsSearched * 1000.0 / ElapsedMilliseconds : 0;
}