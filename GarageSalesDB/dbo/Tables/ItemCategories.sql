CREATE TABLE [dbo].[ItemCategories]
(
  [Id] INT NOT NULL IDENTITY(1,1),
  [Name] NVARCHAR(50) NOT NULL,
  [Description] NVARCHAR(MAX),
  CONSTRAINT PK_ItemCategories PRIMARY KEY ([Id])
)