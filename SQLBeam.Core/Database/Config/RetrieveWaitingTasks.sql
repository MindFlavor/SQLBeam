DELETE TOP (@tasksToSchedule) FROM [core].[WaitingTasks]
OUTPUT DELETED.[GUID], DELETED.[Destination_ID], DELETED.[Task_ID], DELETED.[Parameters], DELETED.[WaitStartTime];
