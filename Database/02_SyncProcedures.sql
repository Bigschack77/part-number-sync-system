-- =============================================
-- Stored Procedures for Part Number Management
-- =============================================

-- Check if part number exists (checks local database)
CREATE OR ALTER PROCEDURE sp_CheckPartNumberExists
    @PartNumber VARCHAR(8),
    @Exists BIT OUTPUT,
    @Location VARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM PartNumbers WHERE PartNumber = @PartNumber AND IsActive = 1)
    BEGIN
        SET @Exists = 1;
        SELECT @Location = Location FROM PartNumbers WHERE PartNumber = @PartNumber;
    END
    ELSE
    BEGIN
        SET @Exists = 0;
        SET @Location = NULL;
    END
END
GO

-- Insert new part number with sync logging
CREATE OR ALTER PROCEDURE sp_InsertPartNumber
    @PartNumber VARCHAR(8),
    @Description NVARCHAR(500),
    @Location VARCHAR(50),
    @CreatedBy NVARCHAR(100),
    @Result VARCHAR(50) OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validate 8-digit format
        IF LEN(@PartNumber) != 8 OR @PartNumber NOT LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
        BEGIN
            SET @Result = 'ERROR';
            SET @Message = 'Part number must be exactly 8 digits';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Check if already exists
        IF EXISTS (SELECT 1 FROM PartNumbers WHERE PartNumber = @PartNumber)
        BEGIN
            SET @Result = 'ERROR';
            SET @Message = 'Part number already exists in the system';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Insert the part number
        INSERT INTO PartNumbers (PartNumber, Description, Location, CreatedBy, CreatedDate, LastModified)
        VALUES (@PartNumber, @Description, @Location, @CreatedBy, GETDATE(), GETDATE());
        
        -- Log to history
        INSERT INTO PartNumberHistory (PartNumber, Description, Location, ModifiedBy, ModifiedDate, Action)
        VALUES (@PartNumber, @Description, @Location, @CreatedBy, GETDATE(), 'INSERT');
        
        -- Log sync action
        INSERT INTO SyncLog (PartNumber, Action, SourceLocation, Status)
        VALUES (@PartNumber, 'INSERT', @Location, 'SUCCESS');
        
        COMMIT TRANSACTION;
        
        SET @Result = 'SUCCESS';
        SET @Message = 'Part number created successfully';
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        SET @Result = 'ERROR';
        SET @Message = ERROR_MESSAGE();
        
        -- Log failed sync
        INSERT INTO SyncLog (PartNumber, Action, SourceLocation, Status, ErrorMessage)
        VALUES (@PartNumber, 'INSERT', @Location, 'FAILED', ERROR_MESSAGE());
    END CATCH
END
GO

-- Search part numbers
CREATE OR ALTER PROCEDURE sp_SearchPartNumbers
    @SearchTerm VARCHAR(50) = NULL,
    @Location VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        PartNumber,
        Description,
        Location,
        CreatedBy,
        CreatedDate,
        LastModified
    FROM PartNumbers
    WHERE IsActive = 1
      AND (@SearchTerm IS NULL OR PartNumber LIKE '%' + @SearchTerm + '%' OR Description LIKE '%' + @SearchTerm + '%')
      AND (@Location IS NULL OR Location = @Location)
    ORDER BY CreatedDate DESC;
END
GO
