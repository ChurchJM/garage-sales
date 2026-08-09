SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

BEGIN TRY
    PRINT 'Clearing table data...';

    -- 1. Delete leaf child tables first
    DELETE FROM [dbo].[FeaturedItems];
    DELETE FROM [dbo].[GarageSaleSchedules];
    DELETE FROM [dbo].[Notifications];

    -- 2. Delete middle-tier dependent tables
    DELETE FROM [dbo].[GarageSales];
    DELETE FROM [dbo].[Users];

    -- 3. Delete top-level parent and lookup tables
    DELETE FROM [dbo].[ItemCategories];
    DELETE FROM [dbo].[GarageSaleTypes];
    DELETE FROM [dbo].[Addresses];

    PRINT 'Reseeding identity values...';

    -- Reseed identities so the next non-explicit INSERT starts at ID 1
    DBCC CHECKIDENT ('[dbo].[FeaturedItems]', RESEED, 0);
    DBCC CHECKIDENT ('[dbo].[GarageSaleSchedules]', RESEED, 0);
    DBCC CHECKIDENT ('[dbo].[Notifications]', RESEED, 0);
    DBCC CHECKIDENT ('[dbo].[GarageSales]', RESEED, 0);
    DBCC CHECKIDENT ('[dbo].[Users]', RESEED, 0);
    DBCC CHECKIDENT ('[dbo].[ItemCategories]', RESEED, 0);
    DBCC CHECKIDENT ('[dbo].[GarageSaleTypes]', RESEED, 0);
    DBCC CHECKIDENT ('[dbo].[Addresses]', RESEED, 0);

    COMMIT TRANSACTION;
    PRINT 'Database reset completed successfully.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    
    PRINT 'Reset script failed. Error details:';
    SELECT 
        ERROR_NUMBER() AS ErrorNumber,
        ERROR_MESSAGE() AS ErrorMessage;
END CATCH;