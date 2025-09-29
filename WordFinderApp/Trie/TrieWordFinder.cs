using System.Collections.Concurrent;
using System.Diagnostics;

namespace WordFinderApp.Trie;

public class TrieWordFinder : IWordFinder
{
    private readonly char[,] _matrix;
    private readonly int _rows;
    private readonly int _cols;
    private TrieNode _root = null!;

    // Public property to access performance statistics
    public List<DirectionStats> LastSearchStats { get; private set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="TrieWordFinder"/> class with the specified matrix.
    /// </summary>
    /// <param name="matrix">A collection of strings representing the character matrix.</param>
    /// <exception cref="ArgumentException">Thrown if the matrix is empty, exceeds 64x64, or rows are not of equal length.</exception>
    public TrieWordFinder(IEnumerable<string> matrix)
    {
        var matrixList = matrix.ToList();

        if (matrixList.Count == 0)
        {
            throw new ArgumentException("Matrix cannot be empty");
        }

        _rows = matrixList.Count;
        _cols = matrixList[0].Length;

        // Validate matrix dimensions (max 64x64 as per requirements)
        if (_rows > 64 || _cols > 64)
        {
            throw new ArgumentException("Matrix size cannot exceed 64x64");
        }

        // Validate all rows have same length
        if (matrixList.Any(row => row.Length != _cols))
        {
            throw new ArgumentException("All matrix rows must have the same length");
        }

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
    /// Finds the top 10 most frequently found words from the given word stream in the matrix.
    /// </summary>
    /// <param name="wordstream">A collection of words to search for in the matrix.</param>
    /// <returns>An enumerable of up to 10 most frequently found words.</returns>
    public IEnumerable<string> Find(IEnumerable<string>? wordstream)
    {
        if (wordstream == null)
        {
            return new List<string>();
        }

        var words = wordstream.Where(w => !string.IsNullOrWhiteSpace(w))
                             .Select(w => w.ToLower())
                             .Distinct()
                             .ToList();

        if (words.Count == 0)
        {
            return new List<string>();
        }

        // Build Trie from word stream
        _root = BuildTrie(words);

        // Use direction-based parallelization with statistics tracking
        var results = new ConcurrentBag<(string word, int row, int col, Direction dir)>();
        var statsCollection = new ConcurrentBag<DirectionStats>();

        var directions = new[]
        {
            Direction.Horizontal,
            Direction.Vertical
        };

        // Process each direction in parallel with performance tracking
        Parallel.ForEach(directions, direction =>
        {
            var stopwatch = Stopwatch.StartNew();
            var threadId = Environment.CurrentManagedThreadId;
            var localResults = new List<(string, int, int, Direction)>();
            var positionsSearched = 0;

            for (var i = 0; i < _rows; i++)
            {
                for (var j = 0; j < _cols; j++)
                {
                    positionsSearched++;
                    SearchFromPosition(i, j, direction, localResults);
                }
            }

            stopwatch.Stop();

            // Create statistics for this direction
            var stats = new DirectionStats
            {
                Direction = direction,
                WordsFound = localResults.Count,
                PositionsSearched = positionsSearched,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                ThreadId = threadId
            };

            statsCollection.Add(stats);

            // Add all local results at once to minimize contention
            foreach (var result in localResults)
            {
                results.Add(result);
            }
        });

        // Store statistics for external access
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
    /// Builds a Trie data structure from the provided collection of words.
    /// </summary>
    /// <param name="words">The words to insert into the Trie.</param>
    /// <returns>The root node of the constructed Trie.</returns>
    private TrieNode BuildTrie(IEnumerable<string> words)
    {
        var root = new TrieNode();

        foreach (var word in words)
        {
            var current = root;
            foreach (var ch in word)
            {
                if (!current.Children.ContainsKey(ch))
                {
                    current.Children[ch] = new TrieNode();
                }
                current = current.Children[ch];
            }
            current.IsEndOfWord = true;
            current.Word = word;
        }

        return root;
    }

    /// <summary>
    /// Searches for words in the matrix starting from a specific position and direction.
    /// </summary>
    /// <param name="startRow">The starting row index.</param>
    /// <param name="startCol">The starting column index.</param>
    /// <param name="direction">The direction to search (horizontal or vertical).</param>
    /// <param name="results">A list to collect found words and their positions.</param>
    private void SearchFromPosition(int startRow, int startCol, Direction direction,
                                  List<(string word, int row, int col, Direction dir)> results)
    {
        var current = _root;
        int row = startRow, col = startCol;

        while (IsValidPosition(row, col))
        {
            var currentChar = _matrix[row, col];

            if (!current.Children.TryGetValue(currentChar, out var child))
            {
                break;
            }

            current = child;

            if (current.IsEndOfWord)
            {
                results.Add((current.Word, startRow, startCol, direction)!);
            }

            // Move to next position based on direction
            var (nextRow, nextCol) = GetNextPosition(row, col, direction);
            row = nextRow;
            col = nextCol;
        }
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