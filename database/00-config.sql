USE [master];
ALTER DATABASE BeamConfig SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE BeamConfig;

CREATE DATABASE BeamConfig;
GO
USE BeamConfig;
GO

CREATE SCHEMA core;
GO

CREATE TYPE [core].[ArrayGUID]
AS TABLE
(
  [GUID] UNIQUEIDENTIFIER
);
GO

CREATE TABLE [core].[Destination](
	[ID] INT IDENTITY(1,1) PRIMARY KEY CLUSTERED,
	[Name] NVARCHAR(255) NOT NULL,
	[ConnectionString] NVARCHAR(MAX)
);
GO

CREATE UNIQUE INDEX idxDestinationName ON [core].[Destination]([Name]);
GO

CREATE TABLE [core].[Task](
	[ID] INT IDENTITY(1,1) PRIMARY KEY CLUSTERED,
	[Name] NVARCHAR(255) NOT NULL,
	[Class] NVARCHAR(MAX),
	[Parameters] NVARCHAR(MAX),
	[Debug] BIT DEFAULT(0)
);
GO

CREATE UNIQUE INDEX idxTaskName ON [core].[Task]([Name]);
GO

CREATE TABLE [core].[TaskPrerequisite] (
		[Task_GUID] UNIQUEIDENTIFIER,
		[Requirement_GUID] UNIQUEIDENTIFIER
);

CREATE UNIQUE CLUSTERED INDEX pki_TaskPrerequisite
    ON [core].[TaskPrerequisite] ([Task_GUID], [Requirement_GUID]);

CREATE TABLE [core].[WaitingTasks](
	[GUID] UNIQUEIDENTIFIER PRIMARY KEY CLUSTERED DEFAULT(NEWSEQUENTIALID()),
	[Destination_ID] INT REFERENCES [core].[Destination]([ID]),
	[Task_ID] INT REFERENCES [core].[Task]([ID]),
	[Parameters] NVARCHAR(MAX) NULL,
	[WaitStartTime] DATETIME2 NOT NULL DEFAULT(GETDATE()),
);
GO

CREATE TABLE [core].[ScheduledTasks](
	[GUID] UNIQUEIDENTIFIER PRIMARY KEY CLUSTERED,
	[Server] UNIQUEIDENTIFIER NOT NULL,
	[Destination_ID] INT NOT NULL REFERENCES [core].[Destination]([ID]),
	[Task_ID] INT NOT NULL REFERENCES [core].[Task]([ID]),
	[Parameters] NVARCHAR(MAX) NULL,
	[WaitStartTime] DATETIME2 NOT NULL DEFAULT(GETDATE()),
	[ScheduledTime] DATETIME2 NOT NULL DEFAULT(GETDATE())
);
GO

CREATE TABLE [core].[RunningTasks](
	[GUID] UNIQUEIDENTIFIER PRIMARY KEY CLUSTERED,
	[Server] UNIQUEIDENTIFIER NOT NULL,
	[Destination_ID] INT NOT NULL REFERENCES [core].[Destination]([ID]),
	[Task_ID] INT NOT NULL REFERENCES [core].[Task]([ID]),
	[Parameters] NVARCHAR(MAX) NULL,
	[WaitStartTime] DATETIME2 NOT NULL DEFAULT(GETDATE()),
	[ScheduledTime] DATETIME2 NOT NULL,
	[StartTime] DATETIME2 NOT NULL DEFAULT(GETDATE())
);
GO

CREATE TABLE [core].[CompletedTasks](
	[GUID] UNIQUEIDENTIFIER PRIMARY KEY CLUSTERED,
	[Server] UNIQUEIDENTIFIER NOT NULL,
	[Destination_ID] INT NOT NULL REFERENCES [core].[Destination]([ID]),
	[Task_ID] INT NOT NULL REFERENCES [core].[Task]([ID]),
	[Parameters] NVARCHAR(MAX) NULL,
	[WaitStartTime] DATETIME2 NOT NULL DEFAULT(GETDATE()),
	[ScheduledTime] DATETIME2 NOT NULL,
	[StartTime] DATETIME2 NOT NULL,
	[CompleteTime] DATETIME2 NOT NULL DEFAULT(GETDATE())
);
GO

CREATE TABLE [core].[ErroredTasks](
	[GUID] UNIQUEIDENTIFIER PRIMARY KEY CLUSTERED,
	[Server] UNIQUEIDENTIFIER NOT NULL,
	[Destination_ID] INT NOT NULL REFERENCES [core].[Destination]([ID]),
	[Task_ID] INT NOT NULL REFERENCES [core].[Task]([ID]),
	[Parameters] NVARCHAR(MAX) NULL,
	[WaitStartTime] DATETIME2 NOT NULL DEFAULT(GETDATE()),
	[ScheduledTime] DATETIME2 NOT NULL,
	[StartTime] DATETIME2 NOT NULL,
	[ErrorTime] DATETIME2 NOT NULL DEFAULT(GETDATE()),
	[ErrorText] NVARCHAR(MAX)
);
GO

CREATE TABLE [core].[Batch](
	[GUID] UNIQUEIDENTIFIER PRIMARY KEY CLUSTERED DEFAULT(NEWSEQUENTIALID()),
	[CreationTime] DATETIME2 NOT NULL DEFAULT(GETDATE()),
);
GO

CREATE TABLE [core].[BatchTasks](
	[Batch_GUID] UNIQUEIDENTIFIER NOT NULL REFERENCES [core].[Batch]([GUID]) ON DELETE CASCADE,
	[Task_GUID] UNIQUEIDENTIFIER NOT NULL
);
GO

ALTER TABLE [core].[BatchTasks]  
ADD CONSTRAINT PK_BatchTasks_BatchAndTask PRIMARY KEY CLUSTERED ([Batch_GUID], [Task_GUID]);  
GO

CREATE VIEW [core].[AllTasks] WITH SCHEMABINDING AS
SELECT
	'Waiting' AS   [Status] ,
	               [GUID] ,
	NULL AS		   [Server], 
	               [Destination_ID] ,
	               [Task_ID],
				   [Parameters],
				   [WaitStartTime],
	NULL AS        [ScheduledTime] ,
	NULL AS        [StartTime] ,
	NULL AS        [CompleteTime] ,
	NULL AS        [ErrorTime] ,
	NULL AS        [ErrorText]
FROM               [core] .[WaitingTasks]
UNION ALL
SELECT
	'Scheduled' AS [Status] ,
	               [GUID] ,
				   [Server], 
				   [Destination_ID] ,
	               [Task_ID],
				   [Parameters],
				   [WaitStartTime],	               
				   [ScheduledTime] ,
	NULL AS        [StartTime] ,
	NULL AS        [CompleteTime] ,
	NULL AS        [ErrorTime] ,
	NULL AS        [ErrorText]
FROM               [core] .[ScheduledTasks]
UNION ALL
SELECT
	'Running' AS   [Status] ,
	               [GUID] ,
				   [Server], 
	               [Destination_ID] ,
	               [Task_ID],
				   [Parameters],
				   [WaitStartTime],	               
				   [ScheduledTime] ,
	               [StartTime] ,
	NULL AS        [CompleteTime] ,
	NULL AS        [ErrorTime] ,
	NULL AS        [ErrorText]
FROM               [core] .[RunningTasks]
UNION ALL
SELECT
	'Completed' AS [Status] ,
	               [GUID] ,
				   [Server], 
	               [Destination_ID] ,
	               [Task_ID],
				   [Parameters],
				   [WaitStartTime],	               
				   [ScheduledTime] ,
	               [StartTime] ,
	               [CompleteTime] ,
	NULL AS        [ErrorTime] ,
	NULL AS        [ErrorText]
FROM               [core] .[CompletedTasks]
UNION ALL
SELECT
	'Error' AS     [Status] ,
	               [GUID] ,
				   [Server], 
	               [Destination_ID] ,
	               [Task_ID],
				   [Parameters],
				   [WaitStartTime],
				   [ScheduledTime] ,
	               [StartTime] ,
	NULL AS        [CompleteTime] ,
	               [ErrorTime] ,
 	               [ErrorText]
FROM               [core] .[ErroredTasks];
GO

-- DROP PROCEDURE [core].[GetBatchWithTaskInStatesByGUIDs] 
CREATE PROCEDURE [core].[GetBatchWithTaskInStatesByGUIDs] 
	@guids AS [core].[ArrayGUID] READONLY
AS
SELECT * FROM [core].[BatchTasks] BT 
INNER JOIN [core].[Batch] B 
	ON BT.[Batch_GUID] = B.[GUID]
INNER JOIN [core].[AllTasks] ATS ON 
	BT.[Task_GUID] = ATS.GUID
WHERE BT.[Batch_GUID] IN (SELECT [GUID] FROM @guids)
ORDER BY BT.[Batch_GUID];
GO

USE [master];
GO
