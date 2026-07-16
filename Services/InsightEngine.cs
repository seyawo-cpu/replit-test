using MindfulJournal.Models;

namespace MindfulJournal.Services;

public class InsightEngine
{
    private static readonly string[] ReflectionPrompts =
    [
        "What's one thing you're genuinely proud of today?",
        "Describe a moment today where you felt fully present.",
        "What's one thing you'd do differently if you could replay today?",
        "Who made a positive impact on your day, and why?",
        "What's something small that brought you unexpected joy?",
        "What challenge did you face today, and what did it teach you?",
        "Describe your energy levels today. What influenced them?",
        "What's one thing you're looking forward to tomorrow?",
        "What conversation mattered most to you today?",
        "If today were a weather forecast, what would it be and why?",
        "What did your body need today that you may have ignored?",
        "Name three things that exist in your life right now that you take for granted.",
        "What belief about yourself did you question today?",
        "What would you tell your past self about today?",
        "Who are you becoming, and does today reflect that?",
    ];

    private static readonly Dictionary<int, string[]> MoodQuotes = new()
    {
        [1] = [
            "Even the darkest night will end and the sun will rise. — Victor Hugo",
            "You don't have to be positive all the time. It's perfectly okay to feel sad, angry, or overwhelmed.",
            "Rock bottom became the solid foundation on which I rebuilt my life. — J.K. Rowling",
            "The wound is the place where the light enters you. — Rumi",
        ],
        [2] = [
            "It's okay to be a work in progress. You are still worthy.",
            "Some days are just harder than others. That's allowed.",
            "Not every day has to be amazing. Surviving counts too.",
            "There's quiet strength in just showing up on the days it's hard.",
        ],
        [3] = [
            "Consistency is more important than perfection.",
            "A good enough day is still a gift.",
            "Progress, not perfection. Keep going.",
            "Ordinary days make up an extraordinary life.",
        ],
        [4] = [
            "Good days remind us what we're working toward.",
            "Hold on to this feeling — it's proof that good things happen.",
            "A good day is worth writing down. You just did.",
            "Gratitude turns what we have into enough. — Melody Beattie",
        ],
        [5] = [
            "This is what it feels like to thrive. Soak it in.",
            "Let this be evidence that amazing days are possible.",
            "You deserve every bit of this joy. Remember it.",
            "The best moments in life aren't planned — they just happen. Today was one.",
        ],
    };

    public string GetDailyPrompt()
    {
        var dayIndex = DateTime.Today.DayOfYear % ReflectionPrompts.Length;
        return ReflectionPrompts[dayIndex];
    }

    public string GetMoodQuote(int mood)
    {
        if (!MoodQuotes.TryGetValue(mood, out var quotes)) return string.Empty;
        var index = DateTime.Now.Second % quotes.Length;
        return quotes[index];
    }

    public JournalInsights Analyze(IReadOnlyList<JournalEntry> entries)
    {
        if (entries.Count == 0)
            return new JournalInsights();

        var now = DateTime.Now;
        var weekEntries = entries.Where(e => e.CreatedAt >= now.AddDays(-7)).ToList();
        var monthEntries = entries.Where(e => e.CreatedAt >= now.AddDays(-30)).ToList();

        var allTags = entries.SelectMany(e => e.Tags).ToList();
        var tagFreq = allTags
            .GroupBy(t => t.ToLower())
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => (Tag: g.Key, Count: g.Count()))
            .ToList();

        // Streak: consecutive days ending today
        var distinctDays = entries
            .Select(e => e.CreatedAt.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        int streak = 0;
        var checkDay = DateTime.Today;
        foreach (var day in distinctDays)
        {
            if (day == checkDay || day == checkDay.AddDays(-1))
            {
                streak++;
                checkDay = day;
            }
            else break;
        }

        // Happiest day of week
        var byDow = entries
            .GroupBy(e => e.CreatedAt.DayOfWeek)
            .Select(g => (Day: g.Key, AvgMood: g.Average(e => e.Mood)))
            .OrderByDescending(x => x.AvgMood)
            .FirstOrDefault();

        return new JournalInsights
        {
            TotalEntries = entries.Count,
            TotalWords = entries.Sum(e => e.WordCount),
            OverallAvgMood = Math.Round(entries.Average(e => e.Mood), 1),
            WeekAvgMood = weekEntries.Count > 0 ? Math.Round(weekEntries.Average(e => e.Mood), 1) : 0,
            MonthAvgMood = monthEntries.Count > 0 ? Math.Round(monthEntries.Average(e => e.Mood), 1) : 0,
            EntriesThisWeek = weekEntries.Count,
            EntriesThisMonth = monthEntries.Count,
            CurrentStreak = streak,
            TopTags = tagFreq,
            HappiestDayOfWeek = distinctDays.Count > 0 ? byDow.Day : null,
            LongestEntry = entries.MaxBy(e => e.WordCount),
        };
    }
}

public class JournalInsights
{
    public int TotalEntries { get; set; }
    public int TotalWords { get; set; }
    public double OverallAvgMood { get; set; }
    public double WeekAvgMood { get; set; }
    public double MonthAvgMood { get; set; }
    public int EntriesThisWeek { get; set; }
    public int EntriesThisMonth { get; set; }
    public int CurrentStreak { get; set; }
    public List<(string Tag, int Count)> TopTags { get; set; } = [];
    public DayOfWeek? HappiestDayOfWeek { get; set; }
    public JournalEntry? LongestEntry { get; set; }
}
