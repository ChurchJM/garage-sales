CREATE TABLE [dbo].[Notifications]
(
  [Id] INT NOT NULL IDENTITY(1,1),
  [UserId] INT NOT NULL,
  [MaxRadius] FLOAT NOT NULL,
  CONSTRAINT PK_Notifications PRIMARY KEY ([Id]),
  CONSTRAINT FK_NotificationsUsers FOREIGN KEY ([UserId]) REFERENCES [Users]([Id])
)
