namespace MindfulJournal.Models;

public class JournalEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Mood { get; set; } // 1–5
    public List<string> Tags { get; set; } = [];

    public string MoodEmoji => Mood switch
    {
        1 => "😞",
        2 => "😐",
        3 => "🙂",
        4 => "😊",
        5 => "🌟",
        _ => "❓"
    };

    public string MoodLabel => Mood switch
    {
        1 => "Rough day",
        2 => "Meh",
        3 => "Alright",
        4 => "Good",
        5 => "Amazing",
        _ => "Unknown"
    };

    public int WordCount => string.IsNullOrWhiteSpace(Content)
        ? 0
        : Content.Trim().Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;
}
