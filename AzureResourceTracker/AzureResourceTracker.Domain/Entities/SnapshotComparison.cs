namespace AzureResourceTracker.Domain.Entities;

public record SnapshotComparison
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid BaselineSnapshotId { get; init; }
    public Guid CurrentSnapshotId { get; init; }
    public List<ResourceChange> Changes { get; init; } = new List<ResourceChange>();
}