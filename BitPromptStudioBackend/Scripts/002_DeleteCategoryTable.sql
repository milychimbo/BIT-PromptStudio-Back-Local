-- Eliminar la tabla de Categorías (Legacy)
-- Intentamos eliminar posibles nombres que haya podido tener
IF OBJECT_ID('dbo.Categoria', 'U') IS NOT NULL DROP TABLE dbo.Categoria;
GO
