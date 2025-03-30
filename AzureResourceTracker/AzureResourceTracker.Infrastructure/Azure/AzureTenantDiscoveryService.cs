using Azure.Identity;
using Azure.ResourceManager;
using Microsoft.Extensions.Configuration;

namespace AzureResourceTracker.Infrastructure.Azure;

public class AzureTenantDiscoveryService : IAzureTenantDiscoveryService
{
    private readonly DefaultAzureCredential _credential;
    private readonly IConfiguration _configuration;

    public AzureTenantDiscoveryService(IConfiguration configuration)
    {
        _credential = new DefaultAzureCredential();
        _configuration = configuration;
    }

    public async Task<List<string>> GetAccessibleTenantsAsync()
    {
        var tenants = new List<string>();
        var armClient = new ArmClient(_credential);

        await foreach (var tenant in armClient.GetTenants().GetAllAsync())
        {
            tenants.Add(tenant.Data.TenantId.ToString()!);
        }

        return tenants;
    }

    public async Task<List<string>> GetFilteredTenantsAsync()
    {
        // Get configured tenant IDs from appsettings
        var configuredTenantIds = _configuration.GetSection("AzureResourceTracker:TenantIds")
            .Get<List<string>>() ?? new List<string>();

        // If no tenant IDs are configured, return all accessible tenants
        if (!configuredTenantIds.Any())
        {
            return await GetAccessibleTenantsAsync();
        }

        // Get all tenants the user has access to
        var accessibleTenants = await GetAccessibleTenantsAsync();

        // Find tenants that are both configured and accessible
        var filteredTenants = configuredTenantIds.Intersect(accessibleTenants).ToList();

        // Check if any configured tenants are not accessible
        var inaccessibleTenants = configuredTenantIds.Except(accessibleTenants).ToList();
        if (inaccessibleTenants.Any())
        {
            throw new UnauthorizedAccessException(
                $"User does not have access to the following configured tenant(s): {string.Join(", ", inaccessibleTenants)}");
        }

        // Check if any filtered tenants were found
        if (!filteredTenants.Any())
        {
            throw new InvalidOperationException("None of the configured tenant IDs are accessible by the current user.");
        }

        return filteredTenants!;
    }
}
