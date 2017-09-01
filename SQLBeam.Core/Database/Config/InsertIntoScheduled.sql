INSERT INTO [core].[ScheduledTasks]
	([GUID], [Server], [Destination_ID], [Task_ID], [Parameters], [WaitStartTime])
VALUES(@GUID, @Server, @Destination_ID, @Task_ID, @Parameters, @WaitStartTime)