DECLARE @stmt NVARCHAR(MAX);

SET @stmt = N'
;WITH CTE AS(
SELECT MAX(InsertTime) AS ''LastInsertedTime'' FROM ' + QUOTENAME(@name) + N'
WHERE [server] = @server
)
SELECT * FROM ' + QUOTENAME(@name) + N' MF
INNER JOIN CTE ON MF.InsertTime = CTE.LastInsertedTime
WHERE [server] = @server;'

DECLARE @ParmDefinition NVARCHAR(MAX) = N'@server NVARCHAR(MAX)';

EXEC sp_executeSql @stmt, @ParmDefinition, @server = @serverName;

