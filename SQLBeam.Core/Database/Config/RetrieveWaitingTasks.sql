DELETE TOP (@tasksToSchedule) WT
OUTPUT DELETED.[GUID], DELETED.[Destination_ID], DELETED.[Task_ID], DELETED.[Parameters], DELETED.[WaitStartTime] 
-- SELECT * 
FROM [core].[WaitingTasks] WT
LEFT OUTER JOIN [core].[TaskPrerequisite] TP ON WT.[GUID] = TP.[Task_GUID]
LEFT OUTER JOIN [core].[CompletedTasks] CT ON TP.[Requirement_GUID] = CT.[GUID]
LEFT OUTER JOIN [core].[ErroredTasks] ET ON TP.[Requirement_GUID] = ET.[GUID]
WHERE
		TP.[Requirement_GUID] IS NULL
		OR CT.[GUID] IS NOT NULL
		OR ET.[GUID] IS NOT NULL