using AzureResourceTracker.Domain.Entities;
using AzureResourceTracker.Domain.Repositories;
using System.Text.Json;

namespace AzureResourceTracker.Infrastructure.Repositories.FileSystem;

public class FileResourceSnapshotRepository : IResourceSnapshotRepository
{
    private readonly string _directory;
    private readonly JsonSerializerOptions _jsonOptions;

    public FileResourceSnapshotRepository(string directory = "Snapshots")
    {
        _directory = directory;
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        // Ensure directory exists
        if (!Directory.Exists(_directory))
        {
            Directory.CreateDirectory(_directory);
        }
    }

    public async Task AddAsync(ResourceSnapshot snapshot)
    {
        var filePath = Path.Combine(_directory, $"{snapshot.Id}.json");
        var json = JsonSerializer.Serialize(snapshot, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var filePath = Path.Combine(_directory, $"{id}.json");
        return await Task.Run(() => File.Exists(filePath));
    }

    public async Task<List<ResourceSnapshot>> GetAllAsync()
    {
        var snapshots = new List<ResourceSnapshot>();
        var files = Directory.GetFiles(_directory, "*.json");

        foreach (var file in files)
        {
            var json = await File.ReadAllTextAsync(file);
            var snapshot = JsonSerializer.Deserialize<ResourceSnapshot>(json);
            if (snapshot != null)
            {
                snapshots.Add(snapshot);
            }
        }

        return snapshots.OrderByDescending(s => s.CreatedAt).ToList();
    }

    public async Task<ResourceSnapshot> GetByIdAsync(Guid id)
    {
        var filePath = Path.Combine(_directory, $"{id}.json");
        if (!File.Exists(filePath))
        {
            return null!;
        }

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<ResourceSnapshot>(json)!;
    }

    public async Task<ResourceSnapshot> GetLatestAsync()
    {
        var files = Directory.GetFiles(_directory, "*.json");
        if (files.Length < 2) // Ensure at least two files exist
        {
            return null!;
        }

        // Note we just added a new file so the latest is the second latest
        var secondLatestFile = files
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTime)
            .Skip(1) // Skip the newest file
            .First(); // Take the second newest file

        var json = await File.ReadAllTextAsync(secondLatestFile.FullName);
        return JsonSerializer.Deserialize<ResourceSnapshot>(json)!;
    }

}
