using System.Collections.Concurrent;
using System.Diagnostics;

namespace WordFinderApp.Radix;

/// <summary>
/// Provides functionality to find words in a character matrix using a Radix Tree for efficient search.
/// </summary>
public class RadixWordFinder : IWordFinder
{
    private readonly char[,] _matrix;
    private readonly int _rows;
    private readonly int _cols;
    private RadixTree _radixTree = null!;

    /// <summary>
    /// Gets the performance statistics for the last search, by direction.
    /// </summary>
    public List<RadixDirectionStats> LastSearchStats { get; private set; } = new List<RadixDirectionStats>();
    /// <summary>
    /// Gets statistics about the Radix Tree used in the last search.
    /// </summary>
    public RadixTreeStats TreeStats { get; private set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="RadixWordFinder"/> class with the specified matrix.
    /// </summary>
    /// <param name="matrix">A collection of strings representing the character matrix.</param>
    /// <exception cref="ArgumentException">Thrown if the matrix is empty, exceeds 64x64, or rows are not of equal length.</exception>
    public RadixWordFinder(IEnumerable<string> matrix)
    {
        var matrixList = matrix.ToList();

        if (matrixList.Count == 0)
            throw new ArgumentException("Matrix cannot be empty");

        _rows = matrixList.Count;
        _cols = matrixList[0].Length;

        // Validate matrix dimensions
        if (_rows > 64 || _cols > 64)
            throw new ArgumentException("Matrix size cannot exceed 64x64");

        if (matrixList.Any(row => row.Length != _cols))
            throw new ArgumentException("All matrix rows must have the same length");

        // Initialize the matrix
        _matrix = new char[_rows, _cols];
        for (var i = 0; i < _rows; i++)
        {
            for (var j = 0; j < _cols; j++)
            {
                _matrix[i, j] = char.ToLower(matrixList[i][j]);
            }
        }
    }

    /// <summary>
    /// Finds the top 10 most frequently found words from the given word stream in the matrix using a Radix Tree.
    /// </summary>
    /// <param name="wordstream">A collection of words to search for in the matrix.</param>
    /// <returns>An enumerable of up to 10 most frequently found words.</returns>
    public IEnumerable<string> Find(IEnumerable<string>? wordstream)
    {
        if (wordstream == null)
            return new List<string>();

        var words = wordstream.Where(w => !string.IsNullOrWhiteSpace(w))
                             .Select(w => w.ToLower())
                             .Distinct()
                             .ToList();

        if (words.Count == 0)
            return new List<string>();

        // Build Radix Tree from word stream
        _radixTree = BuildRadixTree(words);
        TreeStats = _radixTree.GetStats();

        // Use direction-based parallelization with radix tree
        var results = new ConcurrentBag<(string word, int row, int col, Direction dir)>();
        var statsCollection = new ConcurrentBag<RadixDirectionStats>();

        var directions = new[] { Direction.Horizontal, Direction.Vertical };

        // Process each direction in parallel
        Parallel.ForEach(directions, direction =>
        {
            var stopwatch = Stopwatch.StartNew();
            var threadId = Environment.CurrentManagedThreadId;
            var localResults = new List<(string, int, int, Direction)>();
            var positionsSearched = 0;
            var nodesTraversed = 0;
            var charactersMatched = 0;

            for (var i = 0; i < _rows; i++)
            {
                for (var j = 0; j < _cols; j++)
                {
                    positionsSearched++;
                    var searchResult = SearchFromPosition(i, j, direction, localResults);
                    nodesTraversed += searchResult.NodesTraversed;
                    charactersMatched += searchResult.CharactersProcessed;
                }
            }

            stopwatch.Stop();

            // Create enhanced statistics for this direction
            var stats = new RadixDirectionStats
            {
                Direction = direction,
                WordsFound = localResults.Count,
                PositionsSearched = positionsSearched,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                ThreadId = threadId,
                NodesTraversed = nodesTraversed,
                CharactersMatched = charactersMatched
            };

            statsCollection.Add(stats);

            // Add results
            foreach (var result in localResults)
                results.Add(result);
        });

        // Store statistics
        LastSearchStats = statsCollection.OrderBy(s => s.Direction).ToList();

        // Return top 10 most found words
        return results
            .GroupBy(r => r.word)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => g.Key)
            .ToList();
    }

    /// <summary>
    /// Builds a Radix Tree from the provided collection of words.
    /// </summary>
    /// <param name="words">The words to insert into the Radix Tree.</param>
    /// <returns>The constructed RadixTree instance.</returns>
    private RadixTree BuildRadixTree(IEnumerable<string> words)
    {
        var tree = new RadixTree();
        foreach (var word in words)
        {
            tree.Insert(word);
        }
        return tree;
    }

    /// <summary>
    /// Searches for words in the matrix starting from a specific position and direction using the Radix Tree.
    /// </summary>
    /// <param name="startRow">The starting row index.</param>
    /// <param name="startCol">The starting column index.</param>
    /// <param name="direction">The direction to search (horizontal or vertical).</param>
    /// <param name="results">A list to collect found words and their positions.</param>
    /// <returns>A <see cref="RadixSearchResult"/> containing found words and search statistics.</returns>
    private RadixSearchResult SearchFromPosition(int startRow, int startCol, Direction direction,
                                               List<(string word, int row, int col, Direction dir)> results)
    {
        // Build string for this direction from the starting position
        var searchString = BuildSearchString(startRow, startCol, direction);

        // Search using radix tree
        var searchResult = _radixTree.Search(searchString);

        // Add found words to results
        results.AddRange(searchResult.FoundWords.Select(match => (match.Word, startRow, startCol, direction))!);

        return searchResult;
    }

    /// <summary>
    /// Builds a string from the matrix in the specified direction starting at the given position.
    /// </summary>
    /// <param name="startRow">The starting row index.</param>
    /// <param name="startCol">The starting column index.</param>
    /// <param name="direction">The direction to build the string.</param>
    /// <returns>The constructed string for searching.</returns>
    private string BuildSearchString(int startRow, int startCol, Direction direction)
    {
        var chars = new List<char>();
        int row = startRow, col = startCol;

        while (IsValidPosition(row, col))
        {
            chars.Add(_matrix[row, col]);

            // Move to next position
            var (nextRow, nextCol) = GetNextPosition(row, col, direction);
            row = nextRow;
            col = nextCol;
        }

        return new string(chars.ToArray());
    }

    /// <summary>
    /// Determines if the specified row and column are valid positions within the matrix.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="col">The column index.</param>
    /// <returns>True if the position is valid; otherwise, false.</returns>
    private bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < _rows && col >= 0 && col < _cols;
    }

    /// <summary>
    /// Gets the next position in the matrix based on the current position and direction.
    /// </summary>
    /// <param name="row">The current row index.</param>
    /// <param name="col">The current column index.</param>
    /// <param name="direction">The direction to move.</param>
    /// <returns>A tuple containing the next row and column indices.</returns>
    private (int nextRow, int nextCol) GetNextPosition(int row, int col, Direction direction)
    {
        return direction switch
        {
            Direction.Horizontal => (row, col + 1),
            Direction.Vertical => (row + 1, col),
            _ => (row, col)
        };
    }
}