using AzureResourceTracker.Domain.Entities;
using AzureResourceTracker.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Dapper;

namespace AzureResourceTracker.Infrastructure.Repositories.Sql;

public class SqlSnapshotComparisonRepository : ISnapshotComparisonRepository
{
    private readonly string _connectionString;

    public SqlSnapshotComparisonRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task AddAsync(SnapshotComparison comparison)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Serialize changes to JSON
            var changesJson = JsonSerializer.Serialize(comparison.Changes);

            // Insert comparison
            var sql = @"
                    INSERT INTO SnapshotComparisons (Id, CreatedAt, BaselineSnapshotId, CurrentSnapshotId, ChangesJson)
                    VALUES (@Id, @CreatedAt, @BaselineSnapshotId, @CurrentSnapshotId, @ChangesJson)";

            await connection.ExecuteAsync(sql, new
            {
                comparison.Id,
                comparison.CreatedAt,
                comparison.BaselineSnapshotId,
                comparison.CurrentSnapshotId,
                ChangesJson = changesJson
            });
        }
    }

    public async Task<SnapshotComparison> GetByIdAsync(Guid id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = @"
                    SELECT Id, CreatedAt, BaselineSnapshotId, CurrentSnapshotId, ChangesJson 
                    FROM SnapshotComparisons 
                    WHERE Id = @Id";

            var dto = await connection.QueryFirstOrDefaultAsync<ComparisonDto>(sql, new { Id = id });

            if (dto == null)
                return null!;

            return new SnapshotComparison
            {
                Id = dto.Id,
                CreatedAt = dto.CreatedAt,
                BaselineSnapshotId = dto.BaselineSnapshotId,
                CurrentSnapshotId = dto.CurrentSnapshotId,
                Changes = JsonSerializer.Deserialize<List<ResourceChange>>(dto.ChangesJson)!
            };
        }
    }

    public async Task<SnapshotComparison> GetLatestAsync()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var sql = @"
                    SELECT TOP 1 Id, CreatedAt, BaselineSnapshotId, CurrentSnapshotId, ChangesJson 
                    FROM SnapshotComparisons 
                    ORDER BY CreatedAt DESC";

            var dto = await connection.QueryFirstOrDefaultAsync<ComparisonDto>(sql);

            if (dto == null)
                return null!;

            return new SnapshotComparison
            {
                Id = dto.Id,
                CreatedAt = dto.CreatedAt,
                BaselineSnapshotId = dto.BaselineSnapshotId,
                CurrentSnapshotId = dto.CurrentSnapshotId,
                Changes = JsonSerializer.Deserialize<List<ResourceChange>>(dto.ChangesJson)!
            };
        }
    }

    private class ComparisonDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid BaselineSnapshotId { get; set; }
        public Guid CurrentSnapshotId { get; set; }
        public string ChangesJson { get; set; } = string.Empty;
    }
}