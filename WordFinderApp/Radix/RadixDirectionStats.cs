namespace WordFinderApp.Radix;

/// <summary>
/// Stores performance statistics for word search in a specific direction using a Radix Tree.
/// </summary>
public class RadixDirectionStats : DirectionStats
{
    /// <summary>
    /// Gets or sets the number of nodes traversed during the search in this direction.
    /// </summary>
    public int NodesTraversed { get; set; }

    /// <summary>
    /// Gets or sets the number of characters matched during the search in this direction.
    /// </summary>
    public int CharactersMatched { get; set; }

    /// <summary>
    /// Gets the average number of nodes traversed per position searched.
    /// </summary>
    public double AvgNodesPerPosition => PositionsSearched > 0 ? (double)NodesTraversed / PositionsSearched : 0;

    /// <summary>
    /// Gets the compression efficiency (characters matched per node traversed).
    /// </summary>
    public double CompressionEfficiency => NodesTraversed > 0 ? (double)CharactersMatched / NodesTraversed : 0;
}