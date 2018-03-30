USE [BeamConfig];
GO

INSERT INTO	[core].Destination([Name], [ConnectionString])
VALUES('localhost', 'Server=FRCOGNOZBOOK\SQL16;Integrated Security=SSPI');
GO

INSERT INTO	[core].Task([Name], [Class], [Parameters], [Debug])
VALUES('wait 25 secs', 'SQLBeam.Core.Tasks.Executable.Wait.Wait', '25', 1);
GO 

INSERT INTO [core].[WaitingTasks]([Destination_ID], [Task_ID])
VALUES(1,2)
GO 5

DECLARE @guid UNIQUEIDENTIFIER;
DECLARE @guid2 UNIQUEIDENTIFIER;
DECLARE c CURSOR FOR SELECT [GUID] FROM [core].[WaitingTasks]
OPEN c

FETCH NEXT FROM c INTO @guid;
FETCH NEXT FROM c INTO @guid2;
WHILE @@FETCH_STATUS = 0
BEGIN
    INSERT INTO [core].TaskPrerequisite(Task_GUID, Requirement_GUID) VALUES(@guid, @guid2)
    --PRINT @guid
    SET @guid = @guid2;
    FETCH NEXT FROM c INTO @guid2;   
END
CLOSE c
DEALLOCATE c

INSERT INTO [core].TaskPrerequisite(Task_GUID, Requirement_GUID) VALUES('622D8C91-2434-E811-9C1B-00155D004E0A', '632D8C91-2434-E811-9C1B-00155D004E0A');
INSERT INTO [core].TaskPrerequisite(Task_GUID, Requirement_GUID) VALUES('622D8C91-2434-E811-9C1B-00155D004E0A', '642D8C91-2434-E811-9C1B-00155D004E0A');
INSERT INTO [core].TaskPrerequisite(Task_GUID, Requirement_GUID) VALUES('622D8C91-2434-E811-9C1B-00155D004E0A', '19BC5298-2434-E811-9C1B-00155D004E0A');


SELECT * FROM [core].[WaitingTasks];
SELECT * FROM  [core].[TaskPrerequisite];
SELECT * FROM [core].[Task];

--UPDATE [core].[Task] SET Parameters = 15;

DELETE FROM [core].[WaitingTasks];
DELETE FROM [core].CompletedTasks
DELETE FROM [core].[TaskPrerequisite]

DELETE TOP (@tasksToSchedule) FROM [core].[WaitingTasks]
OUTPUT DELETED.[GUID], DELETED.[Destination_ID], DELETED.[Task_ID], DELETED.[Parameters], DELETED.[WaitStartTime];
