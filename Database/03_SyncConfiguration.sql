-- =============================================
-- Configuration for Multi-Server Synchronization
-- =============================================

CREATE TABLE SyncConfiguration (
    ConfigId INT IDENTITY(1,1) PRIMARY KEY,
    ServerName NVARCHAR(200) NOT NULL,
    ServerRole VARCHAR(20) NOT NULL, -- PRIMARY or SECONDARY
    ConnectionString NVARCHAR(500) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    LastSyncDate DATETIME2 NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- Insert configuration for both SQL Servers
-- NOTE: Update these connection strings with your actual server details
INSERT INTO SyncConfiguration (ServerName, ServerRole, ConnectionString, IsActive)
VALUES 
    ('SQL-Server-Shared', 'PRIMARY', 'Server=YOUR_SHARED_SERVER;Database=PartNumberDB;Trusted_Connection=True;', 1),
    ('SQL-Server-Location3', 'SECONDARY', 'Server=YOUR_LOCATION3_SERVER;Database=PartNumberDB;Trusted_Connection=True;', 1);

-- View to monitor sync status
CREATE OR ALTER VIEW vw_SyncStatus AS
SELECT 
    sc.ServerName,
    sc.ServerRole,
    sc.LastSyncDate,
    sc.IsActive,
    COUNT(sl.SyncLogId) as TotalSyncs,
    SUM(CASE WHEN sl.Status = 'SUCCESS' THEN 1 ELSE 0 END) as SuccessfulSyncs,
    SUM(CASE WHEN sl.Status = 'FAILED' THEN 1 ELSE 0 END) as FailedSyncs
FROM SyncConfiguration sc
LEFT JOIN SyncLog sl ON sl.SyncDate >= DATEADD(HOUR, -24, GETDATE())
GROUP BY sc.ServerName, sc.ServerRole, sc.LastSyncDate, sc.IsActive;
GO
