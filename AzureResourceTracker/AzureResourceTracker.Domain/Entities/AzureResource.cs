namespace AzureResourceTracker.Domain.Entities;

public record AzureResource
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public Dictionary<string, string> Tags { get; init; } = new Dictionary<string, string>();
    public string SubscriptionId { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string SubscriptionName { get; init; } = string.Empty;
    public DateTime DiscoveredAtUtc { get; init; }
}