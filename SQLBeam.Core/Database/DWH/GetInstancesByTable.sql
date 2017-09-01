DECLARE @stmt NVARCHAR(MAX);

SET @stmt = N'SELECT DISTINCT [server] FROM ' + QUOTENAME(@name) + ';';

EXEC sp_executeSql @stmt;