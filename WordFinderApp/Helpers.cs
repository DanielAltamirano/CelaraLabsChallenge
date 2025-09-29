using System.Diagnostics;
using WordFinderApp.Radix;
using WordFinderApp.Trie;

namespace WordFinderApp;

/// <summary>
/// Provides helper methods for file path retrieval, usage display, and comparison analysis in the WordFinder console application.
/// </summary>
public static class Helpers
{
    /// <summary>
    /// Runs a comparison between Trie-based and Radix Tree-based word finders, displaying results and performance statistics.
    /// </summary>
    /// <param name="matrix">The character matrix as a collection of strings.</param>
    /// <param name="wordStream">The collection of words to search for.</param>
    public static void RunComparison(IEnumerable<string> matrix, IEnumerable<string> wordStream)
    {
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("TRIE-BASED WORDFINDER");
        Console.WriteLine("=".PadRight(80, '='));

        // Test Trie implementation
        var trieStopwatch = Stopwatch.StartNew();
        var trieFinder = new TrieWordFinder(matrix);
        var trieResults = trieFinder.Find(wordStream).ToList();
        trieStopwatch.Stop();

        DisplayTrieResults(trieFinder, trieResults, trieStopwatch);

        Console.WriteLine("\n" + "=".PadRight(80, '='));
        Console.WriteLine("RADIX TREE-BASED WORDFINDER");
        Console.WriteLine("=".PadRight(80, '='));

        // Test Radix implementation
        var radixStopwatch = Stopwatch.StartNew();
        var radixFinder = new RadixWordFinder(matrix);
        var radixResults = radixFinder.Find(wordStream).ToList();
        radixStopwatch.Stop();

        DisplayRadixResults(radixFinder, radixResults, radixStopwatch);

        Console.WriteLine("\n" + "=".PadRight(80, '='));
        Console.WriteLine("COMPARISON ANALYSIS");
        Console.WriteLine("=".PadRight(80, '='));

        DisplayComparison(trieFinder, trieStopwatch, radixFinder, radixStopwatch, trieResults, radixResults);
    }

    /// <summary>
    /// Displays the results and statistics for the Trie-based word finder.
    /// </summary>
    /// <param name="finder">The TrieWordFinder instance.</param>
    /// <param name="results">The list of found words.</param>
    /// <param name="stopwatch">The stopwatch measuring execution time.</param>
    public static void DisplayTrieResults(TrieWordFinder finder, List<string> results, Stopwatch stopwatch)
    {
        Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Words Found: {results.Count}");
        Console.WriteLine($"Found Words: {string.Join(", ", results)}");
        Console.WriteLine();

        Console.WriteLine("Direction Performance:");
        Console.WriteLine("Direction           | Thread | Words | Positions | Time    | Words/sec | Pos/sec  ");
        Console.WriteLine("-".PadRight(80, '-'));

        foreach (var stat in finder.LastSearchStats)
        {
            Console.WriteLine($"{stat.Direction,-18} | {stat.ThreadId,6} | {stat.WordsFound,5} | " +
                              $"{stat.PositionsSearched,9} | {stat.ElapsedMilliseconds,6}ms | {stat.WordsPerSecond,9:F1} | {stat.PositionsPerSecond,8:F0}");
        }

        var totalWords = finder.LastSearchStats.Sum(s => s.WordsFound);
        var totalPositions = finder.LastSearchStats.Sum(s => s.PositionsSearched);
        var maxTime = finder.LastSearchStats.Max(s => s.ElapsedMilliseconds);
        var avgTime = finder.LastSearchStats.Average(s => s.ElapsedMilliseconds);
        var efficiency = avgTime > 0 ? (avgTime * 2) / maxTime : 0;

        Console.WriteLine();
        Console.WriteLine($"Total words found: {totalWords}");
        Console.WriteLine($"Total positions: {totalPositions:N0}");
        Console.WriteLine($"Parallel efficiency: {efficiency:F1}x (max: 2.0x)");
        Console.WriteLine($"Throughput: {(totalPositions * 1000.0 / maxTime):N0} pos/sec");
    }

    /// <summary>
    /// Displays the results and statistics for the Radix Tree-based word finder.
    /// </summary>
    /// <param name="finder">The RadixWordFinder instance.</param>
    /// <param name="results">The list of found words.</param>
    /// <param name="stopwatch">The stopwatch measuring execution time.</param>
    public static void DisplayRadixResults(RadixWordFinder finder, List<string> results, Stopwatch stopwatch)
    {
        Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Words Found: {results.Count}");
        Console.WriteLine($"Found Words: {string.Join(", ", results)}");
        Console.WriteLine();

        Console.WriteLine("Radix Tree Statistics:");
        var treeStats = finder.TreeStats;
        Console.WriteLine($"Total Nodes: {treeStats.TotalNodes}");
        Console.WriteLine($"Compressed Characters: {treeStats.TotalCompressedChars}");
        Console.WriteLine($"Max Depth: {treeStats.MaxDepth}");
        Console.WriteLine($"Compression Ratio: {treeStats.CompressionRatio:F2} chars/node");
        Console.WriteLine();

        Console.WriteLine("Direction Performance:");
        Console.WriteLine("Direction           | Thread | Words | Positions | Time    | Nodes    | Chars    | Avg N/P | Comp Eff");
        Console.WriteLine("-".PadRight(95, '-'));

        foreach (var stat in finder.LastSearchStats)
        {
            Console.WriteLine($"{stat.Direction,-18} | {stat.ThreadId,6} | {stat.WordsFound,5} | " +
                $"{stat.PositionsSearched,9} | {stat.ElapsedMilliseconds,6}ms | {stat.NodesTraversed,8} | " +
                $"{stat.CharactersMatched,8} | {stat.AvgNodesPerPosition,7:F1} | {stat.CompressionEfficiency,8:F1}");
        }

        var totalWords = finder.LastSearchStats.Sum(s => s.WordsFound);
        var totalPositions = finder.LastSearchStats.Sum(s => s.PositionsSearched);
        var totalNodes = finder.LastSearchStats.Sum(s => s.NodesTraversed);
        var totalChars = finder.LastSearchStats.Sum(s => s.CharactersMatched);
        var maxTime = finder.LastSearchStats.Max(s => s.ElapsedMilliseconds);
        var avgTime = finder.LastSearchStats.Average(s => s.ElapsedMilliseconds);
        var efficiency = avgTime > 0 ? (avgTime * 2) / maxTime : 0;

        Console.WriteLine();
        Console.WriteLine($"Total words found: {totalWords}");
        Console.WriteLine($"Total positions: {totalPositions:N0}");
        Console.WriteLine($"Total nodes traversed: {totalNodes:N0}");
        Console.WriteLine($"Total characters matched: {totalChars:N0}");
        Console.WriteLine($"Parallel efficiency: {efficiency:F1}x (max: 2.0x)");
        Console.WriteLine($"Throughput: {(totalPositions * 1000.0 / maxTime):N0} pos/sec");
        Console.WriteLine($"Node traversal rate: {(totalNodes * 1000.0 / maxTime):N0} nodes/sec");
    }

    /// <summary>
    /// Displays a comparison analysis between Trie and Radix Tree word finders, including performance and accuracy.
    /// </summary>
    /// <param name="trieFinder">The TrieWordFinder instance.</param>
    /// <param name="trieTime">The stopwatch for Trie execution time.</param>
    /// <param name="radixFinder">The RadixWordFinder instance.</param>
    /// <param name="radixTime">The stopwatch for Radix execution time.</param>
    /// <param name="trieResults">The list of words found by the Trie finder.</param>
    /// <param name="radixResults">The list of words found by the Radix finder.</param>
    public static void DisplayComparison(TrieWordFinder trieFinder, Stopwatch trieTime,
                                RadixWordFinder radixFinder, Stopwatch radixTime,
                                List<string> trieResults, List<string> radixResults)
    {
        Console.WriteLine("Performance Comparison:");
        Console.WriteLine("-".PadRight(50, '-'));

        // Execution time comparison
        var speedup = (double)trieTime.ElapsedMilliseconds / radixTime.ElapsedMilliseconds;
        Console.WriteLine($"Trie Execution Time:        {trieTime.ElapsedMilliseconds,8}ms");
        Console.WriteLine($"Radix Execution Time:       {radixTime.ElapsedMilliseconds,8}ms");
        Console.WriteLine($"Radix Speedup:              {speedup,8:F2}x");
        Console.WriteLine();

        // Memory efficiency comparison
        var trieStats = trieFinder.LastSearchStats;
        var radixStats = radixFinder.LastSearchStats;
        var treeStats = radixFinder.TreeStats;

        Console.WriteLine("Memory & Efficiency Analysis:");
        Console.WriteLine($"Radix Tree Nodes:           {treeStats.TotalNodes,8}");
        Console.WriteLine($"Compression Ratio:          {treeStats.CompressionRatio,8:F2} chars/node");
        Console.WriteLine($"Max Tree Depth:             {treeStats.MaxDepth,8}");
        Console.WriteLine();

        // Throughput comparison
        var trieMaxTime = trieStats.Max(s => s.ElapsedMilliseconds);
        var radixMaxTime = radixStats.Max(s => s.ElapsedMilliseconds);
        var triePositions = trieStats.Sum(s => s.PositionsSearched);
        var radixPositions = radixStats.Sum(s => s.PositionsSearched);

        var trieThroughput = triePositions * 1000.0 / trieMaxTime;
        var radixThroughput = radixPositions * 1000.0 / radixMaxTime;

        Console.WriteLine($"Trie Throughput:            {trieThroughput,8:F0} pos/sec");
        Console.WriteLine($"Radix Throughput:           {radixThroughput,8:F0} pos/sec");
        Console.WriteLine($"Throughput Improvement:     {radixThroughput / trieThroughput,8:F2}x");
        Console.WriteLine();

        // Results accuracy comparison
        var resultsMatch = trieResults.Count == radixResults.Count &&
                          trieResults.All(radixResults.Contains);

        Console.WriteLine($"Results Match:              {(resultsMatch ? "✓ YES" : "✗ NO"),8}");
        Console.WriteLine($"Trie Results Count:         {trieResults.Count,8}");
        Console.WriteLine($"Radix Results Count:        {radixResults.Count,8}");

        if (!resultsMatch)
        {
            var trieOnly = trieResults.Except(radixResults).ToList();
            var radixOnly = radixResults.Except(trieResults).ToList();

            if (trieOnly.Any())
                Console.WriteLine($"Trie-only results: {string.Join(", ", trieOnly)}");
            if (radixOnly.Any())
                Console.WriteLine($"Radix-only results: {string.Join(", ", radixOnly)}");
        }

        Console.WriteLine();
        Console.WriteLine("Recommendation:");

        switch (speedup)
        {
            case > 1.1:
                Console.WriteLine($"Radix Tree is {speedup:F1}x faster - Recommended for this dataset");
                break;
            case < 0.9:
                Console.WriteLine($"Trie is {1 / speedup:F1}x faster - Recommended for this dataset");
                break;
            default:
                Console.WriteLine("Both implementations perform similarly");
                break;
        }

        Console.WriteLine($"Memory efficiency: Radix tree uses {treeStats.CompressionRatio:F1} chars/node compression");
    }

    /// <summary>
    /// Gets the matrix file path from command line arguments or prompts the user for input.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The matrix file path.</returns>
    /// <exception cref="ArgumentException">Thrown if the file path is not provided.</exception>
    public static string GetMatrixFilePath(string[] args)
    {
        if (args.Length >= 1 && File.Exists(args[0]))
        {
            return args[0];
        }

        Console.Write("Enter matrix file path: ");
        var path = Console.ReadLine();

        return string.IsNullOrWhiteSpace(path) ? throw new ArgumentException("Matrix file path is required") : path;
    }
    
    /// <summary>
    /// Gets the word stream file path from command line arguments or prompts the user for input.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The word stream file path.</returns>
    /// <exception cref="ArgumentException">Thrown if the file path is not provided.</exception>
    public static string GetWordStreamFilePath(string[] args)
    {
        if (args.Length >= 2 && File.Exists(args[1]))
        {
            return args[1];
        }

        Console.Write("Enter word stream file path: ");
        var path = Console.ReadLine();

        return string.IsNullOrWhiteSpace(path) ? throw new ArgumentException("Word stream file path is required") : path;
    }

    /// <summary>
    /// Displays usage instructions for the WordFinder console application.
    /// </summary>
    public static void ShowUsage()
    {
        Console.WriteLine("WordFinder Console Application");
        Console.WriteLine("Usage: WordFinder <matrixFilePath> <wordStreamFilePath>");
        Console.WriteLine();
        Console.WriteLine("matrixFilePath:   Path to the file containing the character matrix.");
        Console.WriteLine("wordStreamFilePath: Path to the file containing the word stream.");
        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}