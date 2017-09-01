BEGIN TRANSACTION

INSERT INTO [core].[CompletedTasks]
SELECT 
	[GUID], [Server], [Destination_ID], [Task_ID], [Parameters], [WaitStartTime], [ScheduledTime], [StartTime], GETDATE() 
FROM [core].[RunningTasks] WITH(XLOCK)
WHERE
	GUID = @GUID;
	
DELETE FROM [core].[RunningTasks]
WHERE
	GUID = @GUID;

COMMIT TRANSACTION