INSERT INTO [core].WaitingTasks([Destination_ID], [Task_ID], [Parameters])
OUTPUT INSERTED.GUID
VALUES(@Destination_ID, @Task_ID, @Parameters)