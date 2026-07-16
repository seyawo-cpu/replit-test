using MindfulJournal.Models;
using MindfulJournal.Services;
using MindfulJournal.UI;

var store = new JournalStore();
var engine = new InsightEngine();

RunApp();

// ─── Main Loop ───────────────────────────────────────────────────────────────

void RunApp()
{
    while (true)
    {
        Display.Header();
        ShowTodayStatus();
        Console.WriteLine();
        Display.WriteBold("  What would you like to do?");
        Console.WriteLine();
        Display.WriteLine("    1  ✍️   Write a new entry", ConsoleColor.Cyan);
        Display.WriteLine("    2  📖  Browse past entries", ConsoleColor.Cyan);
        Display.WriteLine("    3  🔍  Search entries", ConsoleColor.Cyan);
        Display.WriteLine("    4  📊  View insights & mood trends", ConsoleColor.Cyan);
        Display.WriteLine("    5  💭  Today's reflection prompt", ConsoleColor.Cyan);
        Display.WriteLine("    6  🗑️   Delete an entry", ConsoleColor.Cyan);
        Display.WriteLine("    0  👋  Exit", ConsoleColor.DarkGray);
        Console.WriteLine();

        var choice = Display.ReadLine("→", ConsoleColor.Yellow);

        switch (choice)
        {
            case "1": WriteNewEntry(); break;
            case "2": BrowseEntries(); break;
            case "3": SearchEntries(); break;
            case "4": ViewInsights(); break;
            case "5": ShowReflectionPrompt(); break;
            case "6": DeleteEntry(); break;
            case "0":
                Display.Header();
                Display.WriteLine("\n  Thanks for journaling today. Keep showing up. 🌱\n", ConsoleColor.Green);
                return;
            default:
                Display.WriteWarning("  Not a valid option — try again.");
                System.Threading.Thread.Sleep(800);
                break;
        }
    }
}

// ─── Today Status ────────────────────────────────────────────────────────────

void ShowTodayStatus()
{
    var todayEntries = store.All.Where(e => e.CreatedAt.Date == DateTime.Today).ToList();
    var insights = engine.Analyze(store.All);

    if (todayEntries.Count == 0)
    {
        Display.WriteWarning("  📝 You haven't journaled today yet.");
    }
    else
    {
        var avgMood = todayEntries.Average(e => e.Mood);
        Display.Write($"  ✅ {todayEntries.Count} entr{(todayEntries.Count == 1 ? "y" : "ies")} today  •  Mood: ", ConsoleColor.DarkGray);
        Display.MoodBar(avgMood, 15);
        Display.Write($" {avgMood:0.0}", ConsoleColor.White);
        Console.WriteLine();
    }

    if (insights.CurrentStreak > 1)
        Display.WriteSuccess($"  🔥 {insights.CurrentStreak}-day streak! Keep it going.");

    Display.HRule();
}

// ─── Write New Entry ─────────────────────────────────────────────────────────

void WriteNewEntry()
{
    Display.Header();
    Display.Box("✍️  New Journal Entry");
    Console.WriteLine();

    // Offer today's prompt
    var prompt = engine.GetDailyPrompt();
    Display.WriteDim($"  💭 Today's prompt: \"{prompt}\"");
    Display.WriteDim("     (Use it or write freely — it's your journal.)");
    Console.WriteLine();

    var title = Display.ReadLine("Title (or leave blank):", ConsoleColor.DarkCyan);
    if (string.IsNullOrWhiteSpace(title))
        title = $"Entry — {DateTime.Now:MMM d, h:mm tt}";

    var content = Display.ReadMultiLine("Your entry:");

    if (string.IsNullOrWhiteSpace(content))
    {
        Display.WriteWarning("\n  Entry was empty — nothing saved.");
        Display.PressAnyKey();
        return;
    }

    Console.WriteLine();
    Display.WriteBold("  How are you feeling? (1–5)");
    Display.WriteLine("    1 😞 Rough day   2 😐 Meh   3 🙂 Alright   4 😊 Good   5 🌟 Amazing",
        ConsoleColor.DarkGray);
    var mood = Display.ReadInt("Mood:", 1, 5);

    var tagsRaw = Display.ReadLine("Tags (comma-separated, or leave blank):", ConsoleColor.DarkCyan);
    var tags = tagsRaw
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(t => t.Trim().ToLower())
        .Where(t => t.Length > 0)
        .ToList();

    var entry = new JournalEntry
    {
        Title = title,
        Content = content,
        Mood = mood,
        Tags = tags
    };

    store.Add(entry);

    Console.WriteLine();
    Display.WriteSuccess("  ✅ Entry saved!");
    Console.WriteLine();

    // Show mood quote
    var quote = engine.GetMoodQuote(mood);
    Display.WriteDim($"  \"{quote}\"");
    Console.WriteLine();
    Display.WriteDim($"  Word count: {entry.WordCount} words");

    Display.PressAnyKey();
}

// ─── Browse Entries ──────────────────────────────────────────────────────────

void BrowseEntries()
{
    Display.Header();
    Display.Box("📖  Past Entries");

    var all = store.All.OrderByDescending(e => e.CreatedAt).ToList();

    if (all.Count == 0)
    {
        Display.WriteWarning("\n  No entries yet. Start writing!");
        Display.PressAnyKey();
        return;
    }

    Console.WriteLine();
    Display.WriteBold("  Filter by:");
    Display.WriteLine("    1  All entries", ConsoleColor.Cyan);
    Display.WriteLine("    2  This week", ConsoleColor.Cyan);
    Display.WriteLine("    3  This month", ConsoleColor.Cyan);
    Console.WriteLine();

    var filter = Display.ReadLine("→", ConsoleColor.Yellow);
    List<JournalEntry> entries = filter switch
    {
        "2" => store.ByDateRange(DateTime.Now.AddDays(-7), DateTime.Now),
        "3" => store.ByDateRange(DateTime.Now.AddDays(-30), DateTime.Now),
        _ => all
    };

    if (entries.Count == 0)
    {
        Display.WriteWarning("\n  No entries in that range.");
        Display.PressAnyKey();
        return;
    }

    Console.Clear();
    Display.Header();
    Display.Box($"📖  {entries.Count} Entr{(entries.Count == 1 ? "y" : "ies")} Found");
    Console.WriteLine();

    // Paginate
    const int pageSize = 5;
    int page = 0;
    int totalPages = (entries.Count + pageSize - 1) / pageSize;

    while (true)
    {
        Console.Clear();
        Display.Header();
        Display.Box($"📖  Entries (page {page + 1}/{totalPages})");
        Console.WriteLine();

        var slice = entries.Skip(page * pageSize).Take(pageSize).ToList();
        for (int i = 0; i < slice.Count; i++)
        {
            var e = slice[i];
            var idx = page * pageSize + i + 1;
            Display.Write($"  {idx,2}. ", ConsoleColor.DarkGray);
            Display.Write($"{e.MoodEmoji} ", ConsoleColor.White);
            Display.Write($"{e.Title}", ConsoleColor.White);
            Console.WriteLine();
            Display.Write($"      ", ConsoleColor.DarkGray);
            Display.Write($"{e.CreatedAt:ddd, MMM d yyyy  h:mm tt}", ConsoleColor.DarkGray);
            if (e.Tags.Count > 0)
                Display.Write($"   #{string.Join(" #", e.Tags)}", ConsoleColor.DarkBlue);
            Display.Write($"   {e.WordCount} words", ConsoleColor.DarkGray);
            Console.WriteLine();
            Display.HRule('·', 56, ConsoleColor.DarkGray);
        }

        Console.WriteLine();
        Display.WriteDim("  Enter entry number to read  •  N=next  •  P=prev  •  Q=back");
        var cmd = Display.ReadLine("→", ConsoleColor.Yellow).ToLower();

        if (cmd == "q") break;
        if (cmd == "n" && page < totalPages - 1) { page++; continue; }
        if (cmd == "p" && page > 0) { page--; continue; }

        if (int.TryParse(cmd, out var num) && num >= 1 && num <= entries.Count)
        {
            ReadEntry(entries[num - 1]);
        }
    }
}

void ReadEntry(JournalEntry entry)
{
    Console.Clear();
    Display.Header();
    Console.WriteLine();
    Display.Write($"  {entry.MoodEmoji}  ", ConsoleColor.White);
    Display.WriteLine(entry.Title, ConsoleColor.White);
    Display.WriteDim($"      {entry.CreatedAt:dddd, MMMM d, yyyy  •  h:mm tt}");
    if (entry.Tags.Count > 0)
        Display.WriteDim($"      #{string.Join("  #", entry.Tags)}");
    Display.WriteDim($"      {entry.WordCount} words");
    Console.WriteLine();
    Display.HRule();
    Console.WriteLine();
    Display.WriteWrapped(entry.Content, 4, 72, ConsoleColor.Gray);
    Console.WriteLine();
    Display.HRule();

    var quote = engine.GetMoodQuote(entry.Mood);
    Console.WriteLine();
    Display.WriteDim($"  \"{quote}\"");

    Display.PressAnyKey();
}

// ─── Search ──────────────────────────────────────────────────────────────────

void SearchEntries()
{
    Display.Header();
    Display.Box("🔍  Search Entries");
    Console.WriteLine();

    var query = Display.ReadLine("Search for:", ConsoleColor.DarkCyan);
    if (string.IsNullOrWhiteSpace(query)) return;

    var results = store.Search(query);

    Console.WriteLine();
    if (results.Count == 0)
    {
        Display.WriteWarning($"  No entries found for \"{query}\".");
        Display.PressAnyKey();
        return;
    }

    Display.WriteSuccess($"  Found {results.Count} entr{(results.Count == 1 ? "y" : "ies")}:");
    Console.WriteLine();

    for (int i = 0; i < results.Count; i++)
    {
        var e = results[i];
        Display.Write($"  {i + 1,2}. {e.MoodEmoji} ", ConsoleColor.White);
        Display.WriteLine(e.Title, ConsoleColor.White);
        Display.WriteDim($"      {e.CreatedAt:MMM d, yyyy  •  h:mm tt}   {e.WordCount} words");
        Display.HRule('·', 56, ConsoleColor.DarkGray);
    }

    Console.WriteLine();
    Display.WriteDim("  Enter number to read, or press Enter to go back.");
    var choice = Display.ReadLine("→", ConsoleColor.Yellow);
    if (int.TryParse(choice, out var n) && n >= 1 && n <= results.Count)
        ReadEntry(results[n - 1]);
}

// ─── Insights ────────────────────────────────────────────────────────────────

void ViewInsights()
{
    Display.Header();
    Display.Box("📊  Your Insights");
    Console.WriteLine();

    var insights = engine.Analyze(store.All);

    if (insights.TotalEntries == 0)
    {
        Display.WriteWarning("  No entries yet — start writing to see your insights!");
        Display.PressAnyKey();
        return;
    }

    // Summary row
    Display.WriteLine("  ─── Overview ────────────────────────────────────────", ConsoleColor.DarkCyan);
    Stat("Total entries", $"{insights.TotalEntries}");
    Stat("Total words written", $"{insights.TotalWords:N0}");
    Stat("Avg words per entry", $"{(insights.TotalEntries > 0 ? insights.TotalWords / insights.TotalEntries : 0):N0}");
    Stat("Current streak", $"{insights.CurrentStreak} day{(insights.CurrentStreak != 1 ? "s" : "")} 🔥");
    Console.WriteLine();

    // Mood overview
    Display.WriteLine("  ─── Mood ────────────────────────────────────────────", ConsoleColor.DarkCyan);

    void MoodRow(string label, double mood)
    {
        Display.Write($"  {label,-20}", ConsoleColor.DarkGray);
        Display.MoodBar(mood, 20);
        Display.Write($"  {mood:0.0} / 5.0", ConsoleColor.White);
        Console.WriteLine();
    }

    if (insights.OverallAvgMood > 0) MoodRow("All time avg", insights.OverallAvgMood);
    if (insights.MonthAvgMood > 0) MoodRow("This month avg", insights.MonthAvgMood);
    if (insights.WeekAvgMood > 0) MoodRow("This week avg", insights.WeekAvgMood);
    if (insights.HappiestDayOfWeek.HasValue)
        Display.WriteDim($"\n  😊 You tend to feel best on {insights.HappiestDayOfWeek}s.");

    Console.WriteLine();

    // Activity
    Display.WriteLine("  ─── Activity ────────────────────────────────────────", ConsoleColor.DarkCyan);
    Stat("Entries this week", $"{insights.EntriesThisWeek}");
    Stat("Entries this month", $"{insights.EntriesThisMonth}");
    if (insights.LongestEntry != null)
    {
        Stat("Longest entry",
            $"\"{insights.LongestEntry.Title}\" ({insights.LongestEntry.WordCount} words)");
    }
    Console.WriteLine();

    // Tags
    if (insights.TopTags.Count > 0)
    {
        Display.WriteLine("  ─── Your Top Tags ───────────────────────────────────", ConsoleColor.DarkCyan);
        foreach (var (tag, count) in insights.TopTags)
            Display.WriteDim($"  #{tag,-18} {count} time{(count != 1 ? "s" : "")}");
        Console.WriteLine();
    }

    // Mood distribution
    Display.WriteLine("  ─── Mood Distribution ───────────────────────────────", ConsoleColor.DarkCyan);
    var moodGroups = store.All
        .GroupBy(e => e.Mood)
        .OrderBy(g => g.Key)
        .ToDictionary(g => g.Key, g => g.Count());
    var maxCount = moodGroups.Values.DefaultIfEmpty(1).Max();

    for (int m = 1; m <= 5; m++)
    {
        var count = moodGroups.TryGetValue(m, out var c) ? c : 0;
        var bar = new string('▓', (int)((double)count / maxCount * 20));
        var dummy = new JournalEntry { Mood = m };
        Display.Write($"  {dummy.MoodEmoji} {dummy.MoodLabel,-12}", ConsoleColor.DarkGray);
        Display.Write($" {bar,-20}", ConsoleColor.Cyan);
        Display.Write($" {count}", ConsoleColor.White);
        Console.WriteLine();
    }

    Display.PressAnyKey();

    void Stat(string label, string value)
    {
        Display.Write($"  {label,-26}", ConsoleColor.DarkGray);
        Display.WriteLine(value, ConsoleColor.White);
    }
}

// ─── Reflection Prompt ───────────────────────────────────────────────────────

void ShowReflectionPrompt()
{
    Display.Header();
    Display.Box("💭  Today's Reflection Prompt");
    Console.WriteLine();

    var prompt = engine.GetDailyPrompt();
    Console.WriteLine();
    Display.Write("  \u201C", ConsoleColor.DarkCyan);
    Display.Write(prompt, ConsoleColor.White);
    Display.WriteLine("\u201D", ConsoleColor.DarkCyan);
    Console.WriteLine();
    Display.WriteDim("  Take a moment to sit with this question.");
    Display.WriteDim("  When you're ready, go write your entry.");
    Console.WriteLine();
    Display.HRule();
    Console.WriteLine();
    Display.WriteDim("  Press W to write now, or any other key to go back.");
    var key = Console.ReadKey(intercept: true).KeyChar;
    if (key == 'w' || key == 'W') WriteNewEntry();
}

// ─── Delete Entry ────────────────────────────────────────────────────────────

void DeleteEntry()
{
    Display.Header();
    Display.Box("🗑️  Delete an Entry");
    Console.WriteLine();

    var all = store.All.OrderByDescending(e => e.CreatedAt).Take(20).ToList();
    if (all.Count == 0)
    {
        Display.WriteWarning("  No entries to delete.");
        Display.PressAnyKey();
        return;
    }

    for (int i = 0; i < all.Count; i++)
    {
        var e = all[i];
        Display.Write($"  {i + 1,2}. {e.MoodEmoji} ", ConsoleColor.White);
        Display.Write($"{e.Title}", ConsoleColor.White);
        Display.WriteDim($"   ({e.CreatedAt:MMM d, yyyy})");
    }

    Console.WriteLine();
    Display.WriteWarning("  Enter number to delete, or press Enter to cancel.");
    var choice = Display.ReadLine("→", ConsoleColor.Yellow);

    if (!int.TryParse(choice, out var n) || n < 1 || n > all.Count) return;

    var target = all[n - 1];
    Console.WriteLine();
    Display.WriteWarning($"  Delete \"{target.Title}\"? This cannot be undone.");

    if (Display.Confirm("  Confirm?"))
    {
        store.Delete(target.Id);
        Display.WriteSuccess("\n  Entry deleted.");
    }
    else
    {
        Display.WriteDim("\n  Cancelled.");
    }
    Display.PressAnyKey();
}
