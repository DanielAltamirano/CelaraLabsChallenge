namespace WordFinderApp.Radix;

/// <summary>
/// Implements a Radix Tree for efficient word storage and search in a character matrix.
/// </summary>
public class RadixTree
{
    private readonly RadixNode _root = new();
    /// <summary>
    /// Gets the total number of nodes in the Radix Tree.
    /// </summary>
    public int TotalNodes { get; private set; }
    /// <summary>
    /// Gets the total number of compressed characters in all edge labels.
    /// </summary>
    public int TotalCompressedChars { get; private set; }
    /// <summary>
    /// Gets the maximum depth of the Radix Tree.
    /// </summary>
    public int MaxDepth { get; private set; }

    /// <summary>
    /// Inserts a word into the Radix Tree.
    /// </summary>
    /// <param name="word">The word to insert.</param>
    public void Insert(string word)
    {
        if (string.IsNullOrEmpty(word)) return;

        var current = _root;
        var wordIndex = 0;
        var depth = 0;

        while (wordIndex < word.Length)
        {
            var firstChar = word[wordIndex];

            if (!current.Children.TryGetValue(firstChar, out var childNode))
            {
                // Create new node with remaining word as edge label
                var newNode = new RadixNode
                {
                    EdgeLabel = word.Substring(wordIndex),
                    IsEndOfWord = true,
                    Word = word,
                    WordCount = 1,
                    Depth = depth + 1
                };
                current.Children[firstChar] = newNode;
                TotalNodes++;
                TotalCompressedChars += newNode.EdgeLabel.Length;
                MaxDepth = Math.Max(MaxDepth, newNode.Depth);
                return;
            }

            var edgeLabel = childNode.EdgeLabel;

            // Find common prefix between remaining word and edge label
            var commonLength = 0;
            var maxLength = Math.Min(word.Length - wordIndex, edgeLabel.Length);

            while (commonLength < maxLength &&
                   word[wordIndex + commonLength] == edgeLabel[commonLength])
            {
                commonLength++;
            }

            if (commonLength == edgeLabel.Length)
            {
                // Full edge match - continue to child
                wordIndex += commonLength;
                current = childNode;
                depth++;
            }
            else if (commonLength == 0)
            {
                // No match - this shouldn't happen as we checked first char
                throw new InvalidOperationException("Radix tree implementation error");
            }
            else
            {
                // Partial match - need to split the edge
                SplitEdge(current, firstChar, commonLength, word, wordIndex);
                return;
            }
        }

        // Word insertion complete at current node
        if (!current.IsEndOfWord)
        {
            current.IsEndOfWord = true;
            current.Word = word;
            current.WordCount = 1;
        }
        else
        {
            current.WordCount++;
        }
    }

    private void SplitEdge(RadixNode parent, char edgeChar, int splitPoint, string newWord, int wordIndex)
    {
        var oldChild = parent.Children[edgeChar];
        var oldEdgeLabel = oldChild.EdgeLabel;

        // Create intermediate node for common prefix
        var intermediateNode = new RadixNode
        {
            EdgeLabel = oldEdgeLabel.Substring(0, splitPoint),
            Depth = oldChild.Depth
        };

        // Update old child - remove common prefix from its edge
        oldChild.EdgeLabel = oldEdgeLabel.Substring(splitPoint);
        oldChild.Depth++;

        // Connect intermediate node
        parent.Children[edgeChar] = intermediateNode;

        if (oldChild.EdgeLabel.Length > 0)
        {
            intermediateNode.Children[oldChild.EdgeLabel[0]] = oldChild;
        }

        TotalNodes++;
        TotalCompressedChars += intermediateNode.EdgeLabel.Length;

        // Add remaining part of new word
        var remainingWordIndex = wordIndex + splitPoint;
        if (remainingWordIndex < newWord.Length)
        {
            var newLeaf = new RadixNode
            {
                EdgeLabel = newWord.Substring(remainingWordIndex),
                IsEndOfWord = true,
                Word = newWord,
                WordCount = 1,
                Depth = intermediateNode.Depth + 1
            };
            intermediateNode.Children[newLeaf.EdgeLabel[0]] = newLeaf;
            TotalNodes++;
            TotalCompressedChars += newLeaf.EdgeLabel.Length;
            MaxDepth = Math.Max(MaxDepth, newLeaf.Depth);
        }
        else
        {
            // New word ends at intermediate node
            intermediateNode.IsEndOfWord = true;
            intermediateNode.Word = newWord;
            intermediateNode.WordCount = 1;
        }
    }

    /// <summary>
    /// Searches for words in the Radix Tree that match the given text starting at the specified index.
    /// </summary>
    /// <param name="text">The text to search within.</param>
    /// <param name="startIndex">The index to start searching from.</param>
    /// <returns>A <see cref="RadixSearchResult"/> containing found words and search statistics.</returns>
    public RadixSearchResult Search(string text, int startIndex = 0)
    {
        var result = new RadixSearchResult();
        var current = _root;
        var textIndex = startIndex;

        while (textIndex < text.Length)
        {
            var currentChar = text[textIndex];

            if (!current.Children.TryGetValue(currentChar, out var childNode))
            {
                break;
            }

            var edgeLabel = childNode.EdgeLabel;

            // Check if we can match the entire edge label
            if (textIndex + edgeLabel.Length <= text.Length)
            {
                var matches = !edgeLabel.Where((t, i) => text[textIndex + i] != t).Any();

                if (matches)
                {
                    textIndex += edgeLabel.Length;
                    current = childNode;

                    if (current.IsEndOfWord)
                    {
                        result.FoundWords.Add(new RadixMatch
                        {
                            Word = current.Word,
                            StartIndex = startIndex,
                            EndIndex = textIndex - 1,
                            Length = textIndex - startIndex
                        });
                    }
                    result.NodesTraversed++;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        result.CharactersProcessed = textIndex - startIndex;
        return result;
    }

    /// <summary>
    /// Gets statistics about the current Radix Tree structure and compression.
    /// </summary>
    /// <returns>A <see cref="RadixTreeStats"/> object containing tree statistics.</returns>
    public RadixTreeStats GetStats()
    {
        return new RadixTreeStats
        {
            TotalNodes = TotalNodes,
            TotalCompressedChars = TotalCompressedChars,
            MaxDepth = MaxDepth,
            CompressionRatio = TotalCompressedChars > 0 ? (double)TotalCompressedChars / TotalNodes : 0
        };
    }
}