using AzureResourceTracker.Domain.Entities;
using AzureResourceTracker.Domain.Repositories;
using AzureResourceTracker.Domain.Services;

namespace AzureResourceTracker.Application.Services;

public class ResourceTrackingService
{
    private readonly IResourceDiscoveryService _discoveryService;
    private readonly IResourceSnapshotRepository _snapshotRepository;
    private readonly ISnapshotComparisonRepository _comparisonRepository;
    private readonly IResourceComparisonService _comparisonService;

    public ResourceTrackingService(
        IResourceDiscoveryService discoveryService,
        IResourceSnapshotRepository snapshotRepository,
        ISnapshotComparisonRepository comparisonRepository,
        IResourceComparisonService comparisonService)
    {
        _discoveryService = discoveryService;
        _snapshotRepository = snapshotRepository;
        _comparisonRepository = comparisonRepository;
        _comparisonService = comparisonService;
    }

    public async Task<ResourceSnapshot> CreateSnapshotAsync()
    {
        var resources = await _discoveryService.DiscoverAllResourcesAsync();

        var snapshot = new ResourceSnapshot
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Resources = resources
        };

        await _snapshotRepository.AddAsync(snapshot);
        return snapshot;
    }

    public async Task<SnapshotComparison> CompareWithLatestSnapshotAsync(ResourceSnapshot currentSnapshot)
    {
        var baselineSnapshot = await _snapshotRepository.GetLatestAsync();
        if (baselineSnapshot == null || baselineSnapshot.Id == currentSnapshot.Id)
        {
            // No previous snapshot to compare with
            return null!;
        }

        var comparison = _comparisonService.CompareSnapshots(baselineSnapshot, currentSnapshot);
        await _comparisonRepository.AddAsync(comparison);

        return comparison;
    }
}