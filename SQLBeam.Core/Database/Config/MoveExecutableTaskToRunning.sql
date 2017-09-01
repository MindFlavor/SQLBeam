BEGIN TRANSACTION

INSERT INTO [core].[RunningTasks]
SELECT 
	[GUID], [Server], [Destination_ID], [Task_ID], [Parameters], [WaitStartTime], [ScheduledTime], GETDATE() 
FROM [core].[ScheduledTasks] WITH(XLOCK)
WHERE
	GUID = @GUID;
	
DELETE FROM [core].[ScheduledTasks]
WHERE
	GUID = @GUID;

COMMIT TRANSACTION