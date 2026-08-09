CREATE TABLE [dbo].[Addresses]
(
  [Id] INT NOT NULL IDENTITY(1,1),
  [Street] NVARCHAR(200) NOT NULL,
  [City] NVARCHAR(50) NOT NULL,
  [State] NVARCHAR(50) NOT NULL,
  [Zip] NVARCHAR(20) NOT NULL,

  -- Lat, Lon, and Location are NULL because they depend on geocoding API request, which might fail.
  [Lat] FLOAT NULL,
  [Lon] FLOAT NULL,
  [Location] GEOGRAPHY NULL,
  CONSTRAINT PK_Addresses PRIMARY KEY ([Id])
)
