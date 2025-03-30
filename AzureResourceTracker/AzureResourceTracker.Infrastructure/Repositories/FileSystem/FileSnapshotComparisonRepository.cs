using AzureResourceTracker.Domain.Entities;
using AzureResourceTracker.Domain.Repositories;
using System.Text.Json;

namespace AzureResourceTracker.Infrastructure.Repositories.FileSystem;

public class FileSnapshotComparisonRepository : ISnapshotComparisonRepository
{
    private readonly string _directory;
    private readonly JsonSerializerOptions _jsonOptions;

    public FileSnapshotComparisonRepository(string directory = "Comparisons")
    {
        _directory = directory;
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        // Ensure directory exists
        if (!Directory.Exists(_directory))
        {
            Directory.CreateDirectory(_directory);
        }
    }

    public async Task AddAsync(SnapshotComparison comparison)
    {
        var filePath = Path.Combine(_directory, $"{comparison.Id}.json");
        var json = JsonSerializer.Serialize(comparison, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<SnapshotComparison> GetByIdAsync(Guid id)
    {
        var filePath = Path.Combine(_directory, $"{id}.json");
        if (!File.Exists(filePath))
        {
            return null!;
        }

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<SnapshotComparison>(json)!;
    }

    public async Task<SnapshotComparison> GetLatestAsync()
    {
        var files = Directory.GetFiles(_directory, "*.json");
        if (files.Length == 0)
        {
            return null!;
        }

        var latestFile = files
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTime)
            .First();

        var json = await File.ReadAllTextAsync(latestFile.FullName);
        return JsonSerializer.Deserialize<SnapshotComparison>(json)!;
    }
}