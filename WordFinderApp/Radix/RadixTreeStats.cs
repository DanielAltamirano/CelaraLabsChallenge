namespace WordFinderApp.Radix;

/// <summary>
/// Stores statistics about the structure and compression of a Radix Tree.
/// </summary>
public class RadixTreeStats
{
    /// <summary>
    /// Gets or sets the total number of nodes in the Radix Tree.
    /// </summary>
    public int TotalNodes { get; set; }
    /// <summary>
    /// Gets or sets the total number of compressed characters in all edge labels.
    /// </summary>
    public int TotalCompressedChars { get; set; }
    /// <summary>
    /// Gets or sets the maximum depth of the Radix Tree.
    /// </summary>
    public int MaxDepth { get; set; }
    /// <summary>
    /// Gets or sets the compression ratio (compressed characters per node).
    /// </summary>
    public double CompressionRatio { get; set; }
}