namespace MindfulJournal.UI;

public static class Display
{
    // ─── Colors ──────────────────────────────────────────────────────────────

    public static void Write(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }

    public static void WriteLine(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static void WriteSuccess(string text) => WriteLine(text, ConsoleColor.Green);
    public static void WriteError(string text) => WriteLine(text, ConsoleColor.Red);
    public static void WriteWarning(string text) => WriteLine(text, ConsoleColor.Yellow);
    public static void WriteDim(string text) => WriteLine(text, ConsoleColor.DarkGray);
    public static void WriteAccent(string text) => WriteLine(text, ConsoleColor.Cyan);
    public static void WriteBold(string text) => WriteLine(text, ConsoleColor.White);

    // ─── Boxes & Lines ───────────────────────────────────────────────────────

    public static void HRule(char ch = '─', int width = 60, ConsoleColor color = ConsoleColor.DarkGray)
    {
        WriteLine(new string(ch, width), color);
    }

    public static void Box(string title, ConsoleColor borderColor = ConsoleColor.DarkCyan)
    {
        int w = Math.Max(title.Length + 4, 40);
        var bar = new string('─', w - 2);
        Write("┌" + bar + "┐", borderColor);
        Console.WriteLine();
        Write("│ ", borderColor);
        Write(title.PadRight(w - 3), ConsoleColor.White);
        Write("│", borderColor);
        Console.WriteLine();
        Write("└" + bar + "┘", borderColor);
        Console.WriteLine();
    }

    public static void Header()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
  ╔══════════════════════════════════════════╗
  ║        ✦  MINDFUL JOURNAL  ✦             ║
  ║    Your thoughts, your growth, your life ║
  ╚══════════════════════════════════════════╝");
        Console.ResetColor();
        WriteDim($"  Today is {DateTime.Now:dddd, MMMM d, yyyy}  •  {DateTime.Now:h:mm tt}");
        Console.WriteLine();
    }

    // ─── Mood bar ────────────────────────────────────────────────────────────

    public static void MoodBar(double mood, int maxWidth = 20)
    {
        var filled = (int)Math.Round(mood / 5.0 * maxWidth);
        var empty = maxWidth - filled;
        var color = mood switch
        {
            >= 4.5 => ConsoleColor.Green,
            >= 3.5 => ConsoleColor.Yellow,
            >= 2.5 => ConsoleColor.DarkYellow,
            _ => ConsoleColor.Red,
        };
        Write("[", ConsoleColor.DarkGray);
        Write(new string('█', filled), color);
        Write(new string('░', empty), ConsoleColor.DarkGray);
        Write("]", ConsoleColor.DarkGray);
    }

    // ─── Prompts ─────────────────────────────────────────────────────────────

    public static string ReadLine(string prompt, ConsoleColor promptColor = ConsoleColor.DarkCyan)
    {
        Write($"  {prompt} ", promptColor);
        Console.ForegroundColor = ConsoleColor.White;
        var input = Console.ReadLine() ?? string.Empty;
        Console.ResetColor();
        return input.Trim();
    }

    public static string ReadMultiLine(string prompt)
    {
        WriteLine($"\n  {prompt}", ConsoleColor.DarkCyan);
        WriteDim("  (Type your entry. Press Enter twice when done.)");
        Console.WriteLine();

        var lines = new List<string>();
        string? prev = null;

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.White;
            var line = Console.ReadLine();
            Console.ResetColor();

            if (line == string.Empty && prev == string.Empty)
                break;

            if (line != null)
                lines.Add(line);

            prev = line;
        }

        // Remove trailing blank line if any
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1]))
            lines.RemoveAt(lines.Count - 1);

        return string.Join("\n", lines);
    }

    public static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            var raw = ReadLine(prompt);
            if (int.TryParse(raw, out var n) && n >= min && n <= max)
                return n;
            WriteError($"  Please enter a number between {min} and {max}.");
        }
    }

    public static bool Confirm(string prompt)
    {
        var answer = ReadLine($"{prompt} [y/N]").ToLower();
        return answer == "y" || answer == "yes";
    }

    public static void PressAnyKey(string message = "Press any key to continue...")
    {
        Console.WriteLine();
        WriteDim($"  {message}");
        Console.ReadKey(intercept: true);
    }

    // ─── Word wrap ───────────────────────────────────────────────────────────

    public static void WriteWrapped(string text, int indent = 4, int maxWidth = 72,
        ConsoleColor color = ConsoleColor.Gray)
    {
        var words = text.Split(' ');
        var line = new System.Text.StringBuilder();
        var pad = new string(' ', indent);

        foreach (var word in words)
        {
            if (line.Length + word.Length + 1 > maxWidth - indent)
            {
                WriteLine(pad + line.ToString().TrimEnd(), color);
                line.Clear();
            }
            if (line.Length > 0) line.Append(' ');
            line.Append(word);
        }

        if (line.Length > 0)
            WriteLine(pad + line.ToString(), color);
    }
}
