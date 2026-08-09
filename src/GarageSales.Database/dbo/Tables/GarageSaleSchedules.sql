CREATE TABLE [dbo].[GarageSaleSchedules]
(
  [Id] INT NOT NULL IDENTITY(1,1),
  [GarageSaleId] INT NOT NULL,
  [From] DATETIME NOT NULL,
  [To] DATETIME NOT NULL,
  CONSTRAINT PK_GarageSaleSchedules PRIMARY KEY ([Id]),
  CONSTRAINT FK_GarageSaleSchedulesSales FOREIGN KEY ([GarageSaleId]) REFERENCES [GarageSales]([Id]) ON DELETE CASCADE
)