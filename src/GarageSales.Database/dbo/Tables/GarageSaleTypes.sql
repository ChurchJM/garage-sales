CREATE TABLE [dbo].[GarageSaleTypes]
(
  [Id] INT NOT NULL IDENTITY(1,1),
  [Name] NVARCHAR(50) NOT NULL,
  [Description] NVARCHAR(MAX) NOT NULL,
  CONSTRAINT PK_GarageSaleTypes PRIMARY KEY ([Id])
)