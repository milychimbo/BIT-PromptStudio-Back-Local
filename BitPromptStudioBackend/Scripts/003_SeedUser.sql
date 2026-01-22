-- 1. Ensure Roles Exist
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = 'Admin')
BEGIN
    INSERT INTO dbo.Roles (Name, Description) VALUES ('Admin', 'Total Access');
END
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = 'Editor')
BEGIN
    INSERT INTO dbo.Roles (Name, Description) VALUES ('Editor', 'Can create/edit');
END
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = 'Viewer')
BEGIN
    INSERT INTO dbo.Roles (Name, Description) VALUES ('Viewer', 'Read Only');
END
GO

-- 2. Ensure User Exists
INSERT INTO dbo.Users (Id, EntraObjectId, Email, FullName, RoleId, IsActive)
SELECT 
    '11111111-1111-1111-1111-111111111111', 
    'test-entra-id-001', 
    'tester@bit.com', 
    'Test Administrator', 
    Id, 
    1
FROM dbo.Roles 
WHERE Name = 'Admin'
AND NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = '11111111-1111-1111-1111-111111111111');
GO
