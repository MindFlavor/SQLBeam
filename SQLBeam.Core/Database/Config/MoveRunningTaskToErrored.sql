BEGIN TRANSACTION

INSERT INTO [core].[ErroredTasks]
SELECT 
	[GUID], [Server], [Destination_ID], [Task_ID], [Parameters], [WaitStartTime], [ScheduledTime], [StartTime], GETDATE(), @ErrorText 
FROM [core].[RunningTasks] WITH(XLOCK)
WHERE
	GUID = @GUID;
	
DELETE FROM [core].[RunningTasks]
WHERE
	GUID = @GUID;

COMMIT TRANSACTION