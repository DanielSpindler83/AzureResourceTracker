namespace AzureResourceTracker.Domain.Entities;

public record ResourceSnapshot
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<AzureResource> Resources { get; init; } = new List<AzureResource>();
}