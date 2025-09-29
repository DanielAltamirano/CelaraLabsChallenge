namespace WordFinderApp.Trie;

/// <summary>
/// Represents a node in a Trie data structure for efficient word search.
/// </summary>
public class TrieNode
{
    /// <summary>
    /// Gets or sets the child nodes for each character.
    /// </summary>
    public Dictionary<char, TrieNode> Children { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether this node marks the end of a word.
    /// </summary>
    public bool IsEndOfWord { get; set; }

    /// <summary>
    /// Gets or sets the word associated with this node, if it is the end of a word.
    /// </summary>
    public string? Word { get; set; }
}