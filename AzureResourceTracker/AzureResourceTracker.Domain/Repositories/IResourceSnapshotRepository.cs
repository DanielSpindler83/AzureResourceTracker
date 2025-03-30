using AzureResourceTracker.Domain.Entities;

namespace AzureResourceTracker.Domain.Repositories;

public interface IResourceSnapshotRepository
{
    Task<ResourceSnapshot> GetByIdAsync(Guid id);
    Task<ResourceSnapshot> GetLatestAsync();
    Task<List<ResourceSnapshot>> GetAllAsync();
    Task AddAsync(ResourceSnapshot snapshot);
    Task<bool> ExistsAsync(Guid id);
}