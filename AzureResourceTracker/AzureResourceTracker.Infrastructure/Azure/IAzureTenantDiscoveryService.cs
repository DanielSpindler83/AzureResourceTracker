namespace AzureResourceTracker.Infrastructure.Azure;

public interface IAzureTenantDiscoveryService
{
    Task<List<string>> GetAccessibleTenantsAsync();
    Task<List<string>> GetFilteredTenantsAsync();
}
