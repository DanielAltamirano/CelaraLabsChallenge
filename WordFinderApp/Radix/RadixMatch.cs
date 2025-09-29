namespace WordFinderApp.Radix;

/// <summary>
/// Represents a match found during a Radix Tree search.
/// </summary>
public class RadixMatch
{
    /// <summary>
    /// Gets or sets the matched word.
    /// </summary>
    public required string? Word { get; set; }

    /// <summary>
    /// Gets or sets the start index of the match in the search string.
    /// </summary>
    public int StartIndex { get; set; }

    /// <summary>
    /// Gets or sets the end index of the match in the search string.
    /// </summary>
    public int EndIndex { get; set; }

    /// <summary>
    /// Gets or sets the length of the matched word.
    /// </summary>
    public int Length { get; set; }
}