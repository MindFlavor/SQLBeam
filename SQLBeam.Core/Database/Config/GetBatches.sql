SELECT 
	* 
FROM [core].[Batch]
WHERE [CreationTime] > DATEADD(MINUTE, @minutesAgo * -1, GETDATE())