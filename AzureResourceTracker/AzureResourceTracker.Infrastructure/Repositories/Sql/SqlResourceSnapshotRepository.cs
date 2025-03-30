using AzureResourceTracker.Domain.Entities;
using AzureResourceTracker.Domain.Repositories;
using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Dapper;

namespace AzureResourceTracker.Infrastructure.Repositories.Sql;

public class SqlResourceSnapshotRepository : IResourceSnapshotRepository
{
    private readonly string _connectionString;

    public SqlResourceSnapshotRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task AddAsync(ResourceSnapshot snapshot)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Serialize resources to JSON
            var resourcesJson = JsonSerializer.Serialize(snapshot.Resources);

            // Insert snapshot
            var sql = @"
                    INSERT INTO ResourceSnapshots (Id, CreatedAt, ResourcesJson)
                    VALUES (@Id, @CreatedAt, @ResourcesJson)";

            await connection.ExecuteAsync(sql, new
            {
                snapshot.Id,
                snapshot.CreatedAt,
                ResourcesJson = resourcesJson
            });
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = "SELECT COUNT(1) FROM ResourceSnapshots WHERE Id = @Id";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });

            return count > 0;
        }
    }

    public async Task<List<ResourceSnapshot>> GetAllAsync()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = "SELECT Id, CreatedAt, ResourcesJson FROM ResourceSnapshots ORDER BY CreatedAt DESC";
            var result = await connection.QueryAsync<SnapshotDto>(sql);

            return result.Select(dto => new ResourceSnapshot
            {
                Id = dto.Id,
                CreatedAt = dto.CreatedAt,
                Resources = JsonSerializer.Deserialize<List<AzureResource>>(dto.ResourcesJson)!
            }).ToList();
        }
    }

    public async Task<ResourceSnapshot> GetByIdAsync(Guid id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = "SELECT Id, CreatedAt, ResourcesJson FROM ResourceSnapshots WHERE Id = @Id";
            var dto = await connection.QueryFirstOrDefaultAsync<SnapshotDto>(sql, new { Id = id });

            if (dto == null)
                return null!;

            return new ResourceSnapshot
            {
                Id = dto.Id,
                CreatedAt = dto.CreatedAt,
                Resources = JsonSerializer.Deserialize<List<AzureResource>>(dto.ResourcesJson)!
            };
        }
    }

    public async Task<ResourceSnapshot> GetLatestAsync()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = "SELECT TOP 1 Id, CreatedAt, ResourcesJson FROM ResourceSnapshots ORDER BY CreatedAt DESC";
            var dto = await connection.QueryFirstOrDefaultAsync<SnapshotDto>(sql);

            if (dto == null)
                return null!;

            return new ResourceSnapshot
            {
                Id = dto.Id,
                CreatedAt = dto.CreatedAt,
                Resources = JsonSerializer.Deserialize<List<AzureResource>>(dto.ResourcesJson)!
            };
        }
    }

    private class SnapshotDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ResourcesJson { get; set; } = string.Empty;
    }
}