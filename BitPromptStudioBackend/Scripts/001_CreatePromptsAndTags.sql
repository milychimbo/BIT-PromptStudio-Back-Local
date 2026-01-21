-- 1. Limpieza (Solo limpiar lo que crea este script para ser reentrante)
IF OBJECT_ID('dbo.UserFavorites', 'U') IS NOT NULL DROP TABLE dbo.UserFavorites;
IF OBJECT_ID('dbo.PromptTags', 'U') IS NOT NULL DROP TABLE dbo.PromptTags;
IF OBJECT_ID('dbo.PromptVersions', 'U') IS NOT NULL DROP TABLE dbo.PromptVersions;
IF OBJECT_ID('dbo.Prompts', 'U') IS NOT NULL DROP TABLE dbo.Prompts;
IF OBJECT_ID('dbo.Tags', 'U') IS NOT NULL DROP TABLE dbo.Tags;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL DROP TABLE dbo.Roles;
GO

-- 2. Roles
CREATE TABLE dbo.Roles (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200) NULL
);
GO
INSERT INTO dbo.Roles (Name, Description) VALUES ('Admin', 'Total Access'), ('Editor', 'Can create/edit'), ('Viewer', 'Read Only');
GO

-- 3. Users
CREATE TABLE dbo.Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    EntraObjectId NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(150) NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id)
);
GO

-- 4. Tags
CREATE TABLE dbo.Tags (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Color NVARCHAR(20) NULL,
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_Tags_Users FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id)
);
GO

-- 5. Prompts
CREATE TABLE dbo.Prompts (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    ImageUrl NVARCHAR(500) NULL, -- Red Social: Imagen de portada
    
    -- Estadísticas Sociales
    ViewCount INT DEFAULT 0,
    UseCount INT DEFAULT 0,
    -- FavoriteCount podría ser calculado o cacheado, aquí lo calcularemos dinámicamente o agregaremos campo si hay mucho tráfico.
    
    -- Cache de la MEJOR versión
    BestVersionId UNIQUEIDENTIFIER NULL, 
    BestVersionScore INT DEFAULT 0,
    
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    LastUpdatedBy UNIQUEIDENTIFIER NOT NULL, 
    UpdatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_Prompts_Users_Created FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Prompts_Users_Updated FOREIGN KEY (LastUpdatedBy) REFERENCES dbo.Users(Id)
);
GO

-- 6. PromptVersions
CREATE TABLE dbo.PromptVersions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PromptId UNIQUEIDENTIFIER NOT NULL,
    VersionNumber INT NOT NULL,
    Content NVARCHAR(MAX) NOT NULL, 
    QualityScore INT DEFAULT 0, 
    AnatomyAnalysisJson NVARCHAR(MAX) NULL,
    DetectedIssuesJson NVARCHAR(MAX) NULL,
    SuggestionsJson NVARCHAR(MAX) NULL,
    AuthorId UNIQUEIDENTIFIER NOT NULL, 
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_PromptVersions_Prompts FOREIGN KEY (PromptId) REFERENCES dbo.Prompts(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PromptVersions_Users FOREIGN KEY (AuthorId) REFERENCES dbo.Users(Id)
);
GO

ALTER TABLE dbo.Prompts ADD CONSTRAINT FK_Prompts_BestVersion 
FOREIGN KEY (BestVersionId) REFERENCES dbo.PromptVersions(Id);
GO

-- 7. PromptTags
CREATE TABLE dbo.PromptTags (
    PromptId UNIQUEIDENTIFIER NOT NULL,
    TagId UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY (PromptId, TagId),
    CONSTRAINT FK_PromptTags_Prompts FOREIGN KEY (PromptId) REFERENCES dbo.Prompts(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PromptTags_Tags FOREIGN KEY (TagId) REFERENCES dbo.Tags(Id) ON DELETE CASCADE
);
GO

-- 8. UserFavorites (Nueva tabla social: Mis Favoritos)
CREATE TABLE dbo.UserFavorites (
    UserId UNIQUEIDENTIFIER NOT NULL,
    PromptId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (UserId, PromptId),
    CONSTRAINT FK_UserFavorites_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE NO ACTION, -- Evitar ciclos
    CONSTRAINT FK_UserFavorites_Prompts FOREIGN KEY (PromptId) REFERENCES dbo.Prompts(Id) ON DELETE CASCADE
);
GO
