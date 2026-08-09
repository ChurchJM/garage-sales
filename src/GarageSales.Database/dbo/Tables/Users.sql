CREATE TABLE [dbo].[Users]
(
  [Id] INT NOT NULL IDENTITY(1,1),
  [AddressId] INT NOT NULL,
  [UserName] NVARCHAR(100) NOT NULL,
  [Password] NVARCHAR(100) NOT NULL,
  [Email] NVARCHAR(100) NOT NULL,
  [IsAdmin] BIT NOT NULL,
  CONSTRAINT PK_Users PRIMARY KEY ([Id]),
  CONSTRAINT FK_UsersAddresses FOREIGN KEY ([AddressId]) REFERENCES [Addresses]([Id])
)