using AzureResourceTracker.Domain.Entities;

namespace AzureResourceTracker.Domain.Services;

public interface IResourceDiscoveryService
{
    Task<List<AzureResource>> DiscoverAllResourcesAsync();
}