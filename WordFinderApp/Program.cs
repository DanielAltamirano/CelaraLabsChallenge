using WordFinderApp;

Console.WriteLine("WordFinder Implementation Comparison: Trie vs Radix Tree");
Console.WriteLine("=========================================================");

try
{
    // Get file paths
    var matrixFile = Helpers.GetMatrixFilePath(args);
    var wordStreamFile = Helpers.GetWordStreamFilePath(args);

    Console.WriteLine($"Loading matrix from: {matrixFile}");
    Console.WriteLine($"Loading word stream from: {wordStreamFile}");
    Console.WriteLine();

    // Load files
    var matrix = FileLoader.LoadMatrix(matrixFile);
    var wordStream = FileLoader.LoadWordStream(wordStreamFile);

    Console.WriteLine($"Matrix: {matrix.Count} x {matrix.First().Length}");
    Console.WriteLine($"Word stream: {wordStream.Count} words");
    Console.WriteLine();

    // Test both implementations
    Helpers.RunComparison(matrix, wordStream);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();