using AzureResourceTracker.Domain.Entities;

namespace AzureResourceTracker.Domain.Services;

public interface IResourceComparisonService
{
    SnapshotComparison CompareSnapshots(ResourceSnapshot baseline, ResourceSnapshot current);
}