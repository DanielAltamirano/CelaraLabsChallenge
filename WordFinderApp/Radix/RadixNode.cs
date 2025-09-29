namespace WordFinderApp.Radix;

/// <summary>
/// Represents a node in a Radix Tree, used for efficient word storage and search.
/// </summary>
public class RadixNode
{
    /// <summary>
    /// Gets or sets the label for the edge leading to this node.
    /// </summary>
    public string EdgeLabel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the child nodes for each character.
    /// </summary>
    public Dictionary<char, RadixNode> Children { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether this node marks the end of a word.
    /// </summary>
    public bool IsEndOfWord { get; set; }

    /// <summary>
    /// Gets or sets the word associated with this node, if it is the end of a word.
    /// </summary>
    public string? Word { get; set; }

    /// <summary>
    /// Gets or sets the number of complete words ending at this node.
    /// </summary>
    public int WordCount { get; set; } // Number of complete words ending at this node

    // Statistics for analysis
    /// <summary>
    /// Gets or sets the depth of this node in the tree.
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// Gets the length of the compressed edge label.
    /// </summary>
    public int CompressedLength => EdgeLabel.Length;
}