using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using AzureResourceTracker.Domain.Entities;
using AzureResourceTracker.Domain.Services;

namespace AzureResourceTracker.Infrastructure.Azure;

public class AzureResourceDiscoveryService : IResourceDiscoveryService
{
    private readonly DefaultAzureCredential _credential;
    private readonly IAzureTenantDiscoveryService _tenantDiscoveryService;

    public AzureResourceDiscoveryService(IAzureTenantDiscoveryService tenantDiscoveryService)
    {
        _credential = new DefaultAzureCredential();
        _tenantDiscoveryService = tenantDiscoveryService;
    }

    public async Task<List<AzureResource>> DiscoverAllResourcesAsync()
    {
        // Use a dictionary to prevent duplicates based on resource ID
        var resourceDict = new Dictionary<string, AzureResource>();

        // Get filtered tenants based on configuration
        var tenants = await _tenantDiscoveryService.GetFilteredTenantsAsync();
        foreach (var tenantId in tenants)
        {
            try
            {
                // Create a client specific to this tenant
                var armClient = new ArmClient(_credential, tenantId);
                var subscriptions = await GetAllSubscriptionsAsync(armClient);
                foreach (var subscription in subscriptions)
                {
                    var subscriptionId = subscription.Id.ToString();
                    var subscriptionName = subscription.Data.DisplayName;
                    var azureResources = await GetSubscriptionResourcesAsync(subscription);
                    foreach (var resource in azureResources)
                    {
                        // Use resource ID as the key to avoid duplicates
                        var resourceId = resource.Id.ToString();
                        if (!resourceDict.ContainsKey(resourceId))
                        {
                            resourceDict[resourceId] = new AzureResource
                            {
                                Id = resourceId,
                                Name = resource.Data.Name,
                                Type = resource.Data.ResourceType.ToString(),
                                Location = resource.Data.Location!.DisplayName ?? "location unknown",
                                Tags = resource.Data.Tags != null ?
                                    new Dictionary<string, string>(resource.Data.Tags) :
                                    new Dictionary<string, string>(),
                                SubscriptionId = subscriptionId,
                                SubscriptionName = subscriptionName,
                                TenantId = tenantId,
                                DiscoveredAtUtc = DateTime.UtcNow
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception but continue with other tenants
                Console.WriteLine($"Error processing tenant {tenantId}: {ex.Message}");
            }
        }

        // Return the values as a list
        return resourceDict.Values.ToList();
    }

    private async Task<List<SubscriptionResource>> GetAllSubscriptionsAsync(ArmClient armClient)
    {
        var subscriptions = new List<SubscriptionResource>();
        await foreach (var subscription in armClient.GetSubscriptions().GetAllAsync())
        {
            subscriptions.Add(subscription);
        }
        return subscriptions;
    }

    private async Task<List<GenericResource>> GetSubscriptionResourcesAsync(SubscriptionResource subscription)
    {
        var resources = new List<GenericResource>();
        await foreach (var resource in subscription.GetGenericResourcesAsync())
        {
            resources.Add(resource);
        }
        return resources;
    }
}
