USE [BeamConfig];
GO



INSERT INTO	[core].Destination([Name], [ConnectionString])
VALUES('localhost', 'Server=FRCOGNOZBOOK\SQL16;Integrated Security=SSPI');
GO
INSERT INTO	[core].Destination([Name], [ConnectionString])
VALUES('FRCOGNOZBOOK\SQL16', 'Server=FRCOGNOZBOOK\SQL16;Integrated Security=SSPI');
GO
INSERT INTO	[core].Destination([Name], [ConnectionString])
VALUES('localhost2', 'Server=localhost\SQL16;Integrated Security=SSPI');
GO
INSERT INTO	[core].Destination([Name], [ConnectionString])
VALUES('localhost3', 'Server=.\SQL16;Integrated Security=SSPI');
GO
INSERT INTO	[core].Destination([Name], [ConnectionString])
VALUES('VSQL16A.mindflavor.it', 'Server=VSQL16A.mindflavor.it;Integrated Security=SSPI');
GO
INSERT INTO	[core].Destination([Name], [ConnectionString])
VALUES('VSQL14A', 'Server=VSQL14A.mindflavor.it;Integrated Security=SSPI');
GO
INSERT INTO	[core].Destination([Name], [ConnectionString])
VALUES('VSQL16A', 'Server=VSQL16A.mindflavor.it;Integrated Security=SSPI');
GO
INSERT INTO	[core].Destination([Name], [ConnectionString])
VALUES('V212R2SQL282.mindflavor.it', 'Server=V212R2SQL282.mindflavor.it;Integrated Security=SSPI');
GO


INSERT INTO	[core].Task([Name], [Class], [Parameters], [Debug])
VALUES('sys.objects', 'SQLBeam.Core.Tasks.Executable.TSQLETL.TSQLETL', 
	'{
		"TSQL": "SELECT [name] FROM sys.objects", 
		"DestinationTable": "sys_objects"
	}', 0);
GO

-- "Query": "Get-WmiObject -Class Win32_Volume | select DeviceID, Caption, Capacity, FreeSpace", 
-- DELETE [core].Task WHERE [Name] = 'wmi_win32_volume'
INSERT INTO	[core].Task([Name], [Class], [Parameters], [Debug])
VALUES('wmi_win32_volume', 'SQLBeam.Core.Tasks.Windows.WMI.SimpleQuery.Query',
	'{
		"Query": "SELECT DeviceID, Caption, Capacity, FreeSpace FROM Win32_Volume", 
		"DestinationTable": "wmi_win32_volume",
		"CalculatedFields": [{"Key":"Server", "Value":"SERVER_NAME"}, {"Key": "InsertTime", "Value":"INSERT_TIME"}]
	}', 0);
GO


-- DELETE [core].Task WHERE [Name] = 'sys.sql_logins'
INSERT INTO	[core].Task([Name], [Class], [Parameters], [Debug])
VALUES('sys.sql_logins', 'SQLBeam.Core.Tasks.Executable.TSQLETL.TSQLETL', 
	'{
		"TSQL": "SELECT [name], [sid] FROM sys.sql_logins", 
		"DestinationTable": "sys_sql_logins",
		"CalculatedFields": [{"Key":"Server", "Value":"SERVER_NAME"}, {"Key": "InsertTime", "Value":"INSERT_TIME"}]
	}', 0);
GO

INSERT INTO	[core].Task([Name], [Class], [Parameters], [Debug])
VALUES('exception immediate', 'SQLBeam.Core.Tasks.Executable.Exception.Exception', NULL, 1);
GO

INSERT INTO	[core].Task([Name], [Class], [Parameters], [Debug])
VALUES('wait 30 secs', 'SQLBeam.Core.Tasks.Executable.Wait.Wait', '30', 1);
GO 

-- insert file size script
--
-- .\SQLBeam.TaskLoader\bin\Debug\SQLBeam.TaskLoader.exe sys.master_files .\database\file_size_and_usage.sql sys_master_files "Initial Catalog=BeamConfig;Integrated Security=SSPI;Server=FRCOGNOZBOOK\SQL16" "Server:SERVER_NAME" "InsertTime:INSERT_TIME"
-- DELETE FROM [core].[Task] WHERE [Name] = 'sys.master_files';

SELECT * FROM [core].[Task]
SELECT * FROM [core].[Destination]

--DELETE [core].[Batch];
SELECT * FROM [core].[Batch]; 
SELECT * FROM [core].[BatchTasks] BT 
INNER JOIN [core].[Batch] B 
	ON BT.[Batch_GUID] = B.[GUID]
INNER JOIN [core].[AllTasks] ATS 
	ON BT.[Task_GUID] = ATS.GUID;
	
-- Insert more duplicate destinations
SET NOCOUNT ON;
DECLARE @stmt NVARCHAR(MAX);
DECLARE @cnt INT = 10;

WHILE @cnt < 500
BEGIN
	INSERT INTO	[core].Destination([Name], [ConnectionString])
	VALUES('VSQL16A_' + CONVERT(NVARCHAR, @cnt), 'Server=VSQL16A.mindflavor.it;Integrated Security=SSPI');

	SET @cnt += 1;
END
-- End duplicate destinations

INSERT INTO [core].WaitingTasks([Destination_ID], [Task_ID])
SELECT [ID], 5 FROM [core].[Destination]
WHERE [Name] LIKE 'VSQL16A_%';


INSERT INTO [core].[Batch] 
OUTPUT INSERTED.[GUID], INSERTED.[CreationTime]
DEFAULT VALUES

INSERT INTO [core].WaitingTasks([Destination_ID], [Task_ID], [Parameters])
VALUES(8, 7, '{
	"ConstantFields": [{"Key": "ExecutionID", "Value":100}]
}');

INSERT INTO [core].WaitingTasks([Destination_ID], [Task_ID], [Parameters])
VALUES(2,3, '{Message: "Exception from ' + @@SERVERNAME + '!"}')
GO 15

INSERT INTO [core].WaitingTasks([Destination_ID], [Task_ID])
VALUES(5,5)
GO 5

INSERT INTO [core].WaitingTasks([Destination_ID], [Task_ID])
VALUES(4,4)
GO 7

SELECT * FROM [core].[WaitingTasks];
SELECT * FROM [core].[ScheduledTasks];
SELECT * FROM [core].[RunningTasks];
SELECT * FROM [core].[CompletedTasks];
SELECT * FROM [core].[ErroredTasks];


SELECT 
* 
FROM [core].[AllTasks]
WHERE [WaitStartTime] > DATEADD(MINUTE, -15, GETDATE())
ORDER BY [WaitStartTime] DESC;

SELECT DATEDIFF(SECOND, StartTime, CompleteTime) FROM [core].[AllTasks]
ORDER BY 1 DESC

/*
DELETE [core].[Batch];
TRUNCATE TABLE [core].[WaitingTasks];
TRUNCATE TABLE [core].[ScheduledTasks];
TRUNCATE TABLE [core].[RunningTasks];
TRUNCATE TABLE [core].[CompletedTasks];
TRUNCATE TABLE [core].[ErroredTasks];
*/

USE [master];
GO
