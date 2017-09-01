SELECT 
	* 
FROM [core].[AllTasks]
WHERE [WaitStartTime] > DATEADD(MINUTE, @minutesAgo * -1, GETDATE())