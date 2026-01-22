IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Roles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        [Description] nvarchar(200) NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id] uniqueidentifier NOT NULL,
        [EntraObjectId] nvarchar(100) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [FullName] nvarchar(150) NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserFavorites]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserFavorites] (
        [UserId] uniqueidentifier NOT NULL,
        [PromptId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_UserFavorites] PRIMARY KEY ([UserId], [PromptId]),
        CONSTRAINT [FK_UserFavorites_Prompts_PromptId] FOREIGN KEY ([PromptId]) REFERENCES [dbo].[Prompts] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserFavorites_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
    );
END
GO
