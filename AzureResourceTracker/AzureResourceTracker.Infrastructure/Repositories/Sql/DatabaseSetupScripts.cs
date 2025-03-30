// -----------------------------
// SQL Scripts for Database Setup
// -----------------------------

/*
-- Create ResourceSnapshots table
CREATE TABLE ResourceSnapshots (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    CreatedAt DATETIME2 NOT NULL,
    ResourcesJson NVARCHAR(MAX) NOT NULL
);

-- Create SnapshotComparisons table
CREATE TABLE SnapshotComparisons (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    CreatedAt DATETIME2 NOT NULL,
    BaselineSnapshotId UNIQUEIDENTIFIER NOT NULL,
    CurrentSnapshotId UNIQUEIDENTIFIER NOT NULL,
    ChangesJson NVARCHAR(MAX) NOT NULL,
    FOREIGN KEY (BaselineSnapshotId) REFERENCES ResourceSnapshots(Id),
    FOREIGN KEY (CurrentSnapshotId) REFERENCES ResourceSnapshots(Id)
);

-- Create index for faster lookup
CREATE INDEX IX_ResourceSnapshots_CreatedAt ON ResourceSnapshots(CreatedAt);
CREATE INDEX IX_SnapshotComparisons_CreatedAt ON SnapshotComparisons(CreatedAt);
*/