namespace AzureResourceTracker.Domain.Entities;

public record ResourceChange
{

    public required ChangeType Type { get; set; }
    public required AzureResource Resource { get; set; }
    public Dictionary<string, (string OldValue, string NewValue)> ChangedProperties { get; set; }
        = new Dictionary<string, (string OldValue, string NewValue)>();
}