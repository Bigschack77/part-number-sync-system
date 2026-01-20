-- =============================================
-- Part Number Synchronization System
-- Database Schema for SQL Server
-- =============================================

-- Main Part Numbers Table
CREATE TABLE PartNumbers (
    PartNumber VARCHAR(8) PRIMARY KEY,
    Description NVARCHAR(500) NULL,
    Location VARCHAR(50) NOT NULL,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    LastModified DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1
);

-- Index for faster lookups
CREATE INDEX IX_PartNumbers_Location ON PartNumbers(Location);
CREATE INDEX IX_PartNumbers_CreatedDate ON PartNumbers(CreatedDate);

-- Sync Log Table (tracks synchronization between servers)
CREATE TABLE SyncLog (
    SyncLogId INT IDENTITY(1,1) PRIMARY KEY,
    PartNumber VARCHAR(8) NOT NULL,
    Action VARCHAR(20) NOT NULL, -- INSERT, UPDATE, DELETE
    SourceLocation VARCHAR(50) NOT NULL,
    SyncDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status VARCHAR(20) NOT NULL, -- SUCCESS, FAILED, PENDING
    ErrorMessage NVARCHAR(1000) NULL
);

-- Index for sync monitoring
CREATE INDEX IX_SyncLog_SyncDate ON SyncLog(SyncDate DESC);
CREATE INDEX IX_SyncLog_Status ON SyncLog(Status);

-- Part Number History (audit trail)
CREATE TABLE PartNumberHistory (
    HistoryId INT IDENTITY(1,1) PRIMARY KEY,
    PartNumber VARCHAR(8) NOT NULL,
    Description NVARCHAR(500) NULL,
    Location VARCHAR(50) NOT NULL,
    ModifiedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Action VARCHAR(20) NOT NULL -- INSERT, UPDATE, DELETE
);

CREATE INDEX IX_PartNumberHistory_PartNumber ON PartNumberHistory(PartNumber);