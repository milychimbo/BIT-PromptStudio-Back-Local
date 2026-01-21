-- Eliminar la tabla de Categorías (Legacy)
-- Intentamos eliminar posibles nombres que haya podido tener
-- Delete the Categories table (legacy)
-- Attempt to drop any possible legacy names the table may have had
IF OBJECT_ID('dbo.Categoria', 'U') IS NOT NULL DROP TABLE dbo.Categoria;
IF OBJECT_ID('dbo.Categorias', 'U') IS NOT NULL DROP TABLE dbo.Categorias;
IF OBJECT_ID('dbo.Category', 'U') IS NOT NULL DROP TABLE dbo.Category;
IF OBJECT_ID('dbo.CategoriaEntity', 'U') IS NOT NULL DROP TABLE dbo.CategoriaEntity;
GO
