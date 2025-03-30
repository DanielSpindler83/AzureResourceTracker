using AzureResourceTracker.Domain.Entities;
using AzureResourceTracker.Domain.Services;

namespace AzureResourceTracker.Infrastructure.Services;

public class ResourceComparisonService : IResourceComparisonService
{
    public SnapshotComparison CompareSnapshots(ResourceSnapshot baseline, ResourceSnapshot current)
    {
        var comparison = new SnapshotComparison
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            BaselineSnapshotId = baseline.Id,
            CurrentSnapshotId = current.Id,
            Changes = new List<ResourceChange>()
        };

        try
        {
            var baselineResources = baseline.Resources.ToDictionary(r => r.Id);
            var currentResources = current.Resources.ToDictionary(r => r.Id);


        // Find added resources
        foreach (var resource in current.Resources)
        {
            if (!baselineResources.ContainsKey(resource.Id))
            {
                comparison.Changes.Add(new ResourceChange
                {
                    Type = ChangeType.Added,
                    Resource = resource
                });
            }
        }

        // Find removed resources
        foreach (var resource in baseline.Resources)
        {
            if (!currentResources.ContainsKey(resource.Id))
            {
                comparison.Changes.Add(new ResourceChange
                {
                    Type = ChangeType.Removed,
                    Resource = resource
                });
            }
        }

        // Find modified resources
        foreach (var resource in current.Resources)
        {
            if (baselineResources.TryGetValue(resource.Id, out var baselineResource))
            {
                var changes = CompareResourceProperties(baselineResource, resource);
                if (changes.Count > 0)
                {
                    comparison.Changes.Add(new ResourceChange
                    {
                        Type = ChangeType.Modified,
                        Resource = resource,
                        ChangedProperties = changes
                    });
                }
            }
        }
        }
        catch (Exception ex)
        {

        }

        return comparison;
    }

    private Dictionary<string, (string OldValue, string NewValue)> CompareResourceProperties(
        AzureResource baseline, AzureResource current)
    {
        var changes = new Dictionary<string, (string OldValue, string NewValue)>();

        // Compare simple properties
        if (baseline.Name != current.Name)
            changes["Name"] = (baseline.Name, current.Name);

        if (baseline.Type != current.Type)
            changes["Type"] = (baseline.Type, current.Type);

        if (baseline.Location != current.Location)
            changes["Location"] = (baseline.Location, current.Location);

        // Compare tags
        var baselineTags = baseline.Tags ?? new Dictionary<string, string>();
        var currentTags = current.Tags ?? new Dictionary<string, string>();

        // Added or modified tags
        foreach (var tag in currentTags)
        {
            if (!baselineTags.TryGetValue(tag.Key, out var baselineValue) || baselineValue != tag.Value)
            {
                changes[$"Tag:{tag.Key}"] = (baselineTags.ContainsKey(tag.Key) ? baselineTags[tag.Key] : "null", tag.Value);
            }
        }

        // Removed tags
        foreach (var tag in baselineTags)
        {
            if (!currentTags.ContainsKey(tag.Key))
            {
                changes[$"Tag:{tag.Key}"] = (tag.Value, "null");
            }
        }

        return changes;
    }
}