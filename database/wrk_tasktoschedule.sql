BEGIN TRANSACTION

DECLARE @tasksToSchedule INT = 3

--DELETE TOP (@tasksToSchedule) FROM [core].[WaitingTasks]
--OUTPUT DELETED.[GUID], DELETED.[Destination_ID], DELETED.[Task_ID], DELETED.[Parameters], DELETED.[WaitStartTime];

--DELETE TOP (@tasksToSchedule) WT
--OUTPUT DELETED.[GUID], DELETED.[Destination_ID], DELETED.[Task_ID], DELETED.[Parameters], DELETED.[WaitStartTime] 
SELECT * 
FROM [core].[WaitingTasks] WT
LEFT OUTER JOIN [core].[TaskPrerequisite] TP ON WT.[GUID] = TP.[Task_GUID]
LEFT OUTER JOIN [core].[CompletedTasks] CT ON TP.[Requirement_GUID] = CT.[GUID]
LEFT OUTER JOIN [core].[ErroredTasks] ET ON TP.[Requirement_GUID] = ET.[GUID]
WHERE
		TP.[Requirement_GUID] IS NULL
		OR CT.[GUID] IS NOT NULL
		OR ET.[GUID] IS NOT NULL

ROLLBACK

1 - TP.Requirement_ID IS NULL
2 - CT.GUID IS NOT NULL
3 - ET.GUID IS NOT NULL

DELETE FROM [core].[WaitingTasks]
OUTPUT DELETED.[GUID], DELETED.[Destination_ID], DELETED.[Task_ID], DELETED.[Parameters], DELETED.[WaitStartTime]
WHERE [GUID] IN (
SELECT 
	WT.[GUID] 
	--,SUM(CASE WHEN CT.[GUID] IS NULL THEN 0 ELSE 1 END) AS Completed
	--,SUM(CASE WHEN ET.[GUID] IS NULL THEN 0 ELSE 1 END) AS Errored
	--,COUNT(*) AS Cnt
FROM [core].[WaitingTasks] WT
LEFT OUTER JOIN [core].[TaskPrerequisite] TP ON WT.[GUID] = TP.[Task_GUID]
LEFT OUTER JOIN [core].[CompletedTasks] CT ON TP.[Requirement_GUID] = CT.[GUID]
LEFT OUTER JOIN [core].[ErroredTasks] ET ON TP.[Requirement_GUID] = ET.[GUID]
WHERE
	TP.[Requirement_GUID] IS NOT NULL
GROUP BY 
	WT.[GUID]
HAVING 
	SUM(CASE WHEN CT.[GUID] IS NULL THEN 0 ELSE 1 END) + SUM(CASE WHEN ET.[GUID] IS NULL THEN 0 ELSE 1 END) = COUNT(*) 
UNION ALL
SELECT 
	WT.[GUID]
FROM [core].[WaitingTasks] WT
LEFT OUTER JOIN [core].[TaskPrerequisite] TP ON WT.[GUID] = TP.[Task_GUID]
WHERE
	TP.[Requirement_GUID] IS NULL
);

