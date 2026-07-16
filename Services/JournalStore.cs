using System.Text.Json;
using MindfulJournal.Models;

namespace MindfulJournal.Services;

public class JournalStore
{
    private readonly string _dataPath;
    private List<JournalEntry> _entries = [];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JournalStore()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var dir = Path.Combine(appData, ".mindful-journal");
        Directory.CreateDirectory(dir);
        _dataPath = Path.Combine(dir, "entries.json");
        Load();
    }

    public IReadOnlyList<JournalEntry> All => _entries.AsReadOnly();

    public void Add(JournalEntry entry)
    {
        _entries.Add(entry);
        Save();
    }

    public void Delete(Guid id)
    {
        _entries.RemoveAll(e => e.Id == id);
        Save();
    }

    public JournalEntry? FindById(Guid id) =>
        _entries.FirstOrDefault(e => e.Id == id);

    public List<JournalEntry> Search(string query)
    {
        var q = query.ToLowerInvariant();
        return _entries
            .Where(e =>
                e.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                e.Content.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                e.Tags.Any(t => t.Contains(q, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(e => e.CreatedAt)
            .ToList();
    }

    public List<JournalEntry> ByDateRange(DateTime from, DateTime to) =>
        _entries
            .Where(e => e.CreatedAt >= from && e.CreatedAt <= to)
            .OrderByDescending(e => e.CreatedAt)
            .ToList();

    private void Load()
    {
        if (!File.Exists(_dataPath)) return;
        try
        {
            var json = File.ReadAllText(_dataPath);
            _entries = JsonSerializer.Deserialize<List<JournalEntry>>(json, JsonOptions) ?? [];
        }
        catch
        {
            _entries = [];
        }
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_entries, JsonOptions);
        File.WriteAllText(_dataPath, json);
    }
}
