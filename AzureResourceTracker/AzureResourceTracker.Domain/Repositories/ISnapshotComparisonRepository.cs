using AzureResourceTracker.Domain.Entities;

namespace AzureResourceTracker.Domain.Repositories;

public interface ISnapshotComparisonRepository
{
    Task<SnapshotComparison> GetByIdAsync(Guid id);
    Task<SnapshotComparison> GetLatestAsync();
    Task AddAsync(SnapshotComparison comparison);
}