CREATE TABLE [dbo].[FeaturedItems]
(
  [Id] INT NOT NULL IDENTITY(1,1),
  [GarageSaleId] INT NOT NULL,
  [CategoryId] INT NOT NULL,
  [Description] NVARCHAR(MAX) NOT NULL,
  CONSTRAINT PK_FeaturedItems PRIMARY KEY ([Id]),
  CONSTRAINT FK_FeaturedItemsCategories FOREIGN KEY ([CategoryId]) REFERENCES [ItemCategories]([Id]),
  CONSTRAINT FK_FeaturedItemsGarageSales FOREIGN KEY ([GarageSaleId]) REFERENCES [GarageSales]([Id]) ON DELETE CASCADE
)