using AzureResourceTracker.Application.Services;
using AzureResourceTracker.Domain.Entities;
using AzureResourceTracker.Domain.Repositories;
using AzureResourceTracker.Domain.Services;
using AzureResourceTracker.Infrastructure.Azure;
using AzureResourceTracker.Infrastructure.Repositories.FileSystem;
using AzureResourceTracker.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzureResourceTracker.ConsoleApp;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Setup DI container
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            // Get the resource tracking service
            var resourceTrackingService = serviceProvider.GetRequiredService<ResourceTrackingService>();

            Console.WriteLine("Starting Azure Resource Tracker...");
            Console.WriteLine("Discovering resources across all subscriptions...");

            // Create a new snapshot
            var snapshot = await resourceTrackingService.CreateSnapshotAsync();
            Console.WriteLine($"Created snapshot with id {snapshot.Id} at {snapshot.CreatedAt}");
            Console.WriteLine($"Discovered {snapshot.Resources.Count} resources");

            // Compare with latest snapshot
            var comparison = await resourceTrackingService.CompareWithLatestSnapshotAsync(snapshot);
            if (comparison != null)
            {
                Console.WriteLine($"Comparison with previous snapshot completed. Snapshot with id {comparison.BaselineSnapshotId}"); 
                Console.WriteLine($"Added resources: {comparison.Changes.Count(c => c.Type == ChangeType.Added)}");
                Console.WriteLine($"Removed resources: {comparison.Changes.Count(c => c.Type == ChangeType.Removed)}");
                Console.WriteLine($"Modified resources: {comparison.Changes.Count(c => c.Type == ChangeType.Modified)}");

                // Display detailed changes
                DisplayDetailedChanges(comparison);
            }
            else
            {
                Console.WriteLine("No previous snapshot found for comparison");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private static void DisplayDetailedChanges(SnapshotComparison comparison)
    {
        Console.WriteLine("\nDetailed Changes:");

        // Display added resources
        var addedResources = comparison.Changes.Where(c => c.Type == ChangeType.Added).ToList();
        if (addedResources.Any())
        {
            Console.WriteLine("\nAdded Resources:");
            foreach (var change in addedResources)
            {
                Console.WriteLine($"+ {change.Resource.Name} ({change.Resource.Type}) in {change.Resource.Location}");
            }
        }

        // Display removed resources
        var removedResources = comparison.Changes.Where(c => c.Type == ChangeType.Removed).ToList();
        if (removedResources.Any())
        {
            Console.WriteLine("\nRemoved Resources:");
            foreach (var change in removedResources)
            {
                Console.WriteLine($"- {change.Resource.Name} ({change.Resource.Type}) in {change.Resource.Location}");
            }
        }

        // Display modified resources
        var modifiedResources = comparison.Changes.Where(c => c.Type == ChangeType.Modified).ToList();
        if (modifiedResources.Any())
        {
            Console.WriteLine("\nModified Resources:");
            foreach (var change in modifiedResources)
            {
                Console.WriteLine($"* {change.Resource.Name} ({change.Resource.Type}) in {change.Resource.Location}");
                foreach (var property in change.ChangedProperties)
                {
                    Console.WriteLine($"  - {property.Key}: {property.Value.OldValue} -> {property.Value.NewValue}");
                }
            }
        }
    }

    private static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();

        // Configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        // Register services
        services.AddSingleton<IConfiguration>(configuration);

        // Domain services
        services.AddScoped<IResourceDiscoveryService, AzureResourceDiscoveryService>();
        services.AddScoped<IResourceComparisonService, ResourceComparisonService>();

        // Infra services
        services.AddSingleton<IAzureTenantDiscoveryService, AzureTenantDiscoveryService>();


        // Repositories
        // Comment/uncomment to switch between file system and SQL repositories

        // File System Repositories
        services.AddScoped<IResourceSnapshotRepository, FileResourceSnapshotRepository>();
        services.AddScoped<ISnapshotComparisonRepository, FileSnapshotComparisonRepository>();

        // SQL Repositories
        // Uncomment to use SQL repositories
        /*
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddScoped<IResourceSnapshotRepository>(provider => 
            new SqlResourceSnapshotRepository(connectionString));
        services.AddScoped<ISnapshotComparisonRepository>(provider => 
            new SqlSnapshotComparisonRepository(connectionString));
        */

        // Application services
        services.AddScoped<ResourceTrackingService>();

        return services;
    }
}
