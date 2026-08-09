CREATE TABLE [dbo].[GarageSales]
(
  [Id] INT NOT NULL IDENTITY(1,1),
  [SaleTypeId] INT NOT NULL,
  [OwnerId] INT NOT NULL,
  [AddressId] INT NOT NULL,
  [Description] NVARCHAR(MAX),
  [Draft] BIT NOT NULL,
  CONSTRAINT PK_GarageSales PRIMARY KEY ([Id]),
  CONSTRAINT FK_GarageSalesTypes FOREIGN KEY ([SaleTypeId]) REFERENCES [GarageSaleTypes]([Id]),
  CONSTRAINT FK_GarageSalesUsers FOREIGN KEY ([OwnerId]) REFERENCES [Users]([Id]),
  CONSTRAINT FK_GarageSalesAddresses FOREIGN KEY ([AddressId]) REFERENCES [Addresses]([Id])
)
