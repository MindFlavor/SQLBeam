USE [BeamDWH];
GO

SELECT * FROM [dbo].[sys_master_files]

SELECT [name] FROM sys.tables
WHERE [name] = @name


DECLARE @name NVARCHAR(MAX) = 'sys_master_files';

DECLARE @stmt NVARCHAR(MAX);

SET @stmt = N'SELECT DISTINCT [server] FROM ' + QUOTENAME(@name) + ';';

EXEC sp_executeSql @stmt;

SET STATISTICS IO ON;

DECLARE @server NVARCHAR(MAX) = 'FRCOGNOZBOOK\SQL16';

;WITH CTE AS(
SELECT MAX(InsertTime) AS 'LastInsertedTime' FROM sys_master_files
wHERE [server] = @server
)
SELECT * FROM sys_master_files MF
INNER JOIN CTE ON MF.InsertTime = CTE.LastInsertedTime
WHERE [server] = @server;

SELECT * FROM sys_master_files;

CREATE INDEX idxInsertTime ON sys_master_files(InsertTime);
DROP INDEX sys_master_files.idxInsertTime

SELECT * FROM [dbo].[sys_sql_logins]

--TRUNCATE TABLE [dbo].[sys_objects];
SELECT * FROM [dbo].[sys_objects];
SELECT COUNT_BIG(*) FROM [dbo].[sys_objects];

UPDATE sys_master_files SET server='gigio' WHERE InsertTime = '2017-08-03 15:46:02.307'

--TRUNCATE TABLE sys_master_files;

USE [BeamConfig];
GO
UPDATE [core].[Batch] SET [CreationTime] = GETDATE();

SELECT * FROM [core].[Batch];
SELECT * FROM [core].[BatchTasks];

SELECT * FROM [core].[BatchTasks] BT
INNER JOIN [core].[AllTasks] A
ON BT.Task_GUID = A.GUID
INNER JOIN [core].[Destination] D
ON A.Destination_ID = D.ID
WHERE [Status] = 'Error';

INSERT INTO [core].[BatchTasks]
SELECT '2B0C1CE4-9A88-E711-8113-00155D00A106', '166D8D43-D588-E711-8113-00155D00A106'

INSERT INTO [core].[BatchTasks] 
SELECT 'E01174F1-AC88-E711-8113-00155D00A106', [GUID] FROM [core].WaitingTasks;

USE [master];
GO



