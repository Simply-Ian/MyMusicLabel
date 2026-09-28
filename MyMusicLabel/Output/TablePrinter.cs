namespace MyMusicLabel.Output;

public static class TablePrinter
{
    public static void Print(IReadOnlyList<string> header, IReadOnlyList<string[]> rows)
    {
        var widths = header
            .Select((title, index) => Math.Max(title.Length, rows.Max(row => row[index].Length)))
            .ToArray();
        PrintRow(header, widths);
        foreach (var row in rows) PrintRow(row, widths);
    }

    private static void PrintRow(IReadOnlyList<string> cells, IReadOnlyList<int> widths) =>
        Console.WriteLine(string.Join("  ", cells.Select((cell, index) => cell.PadRight(widths[index]))).TrimEnd());
}
