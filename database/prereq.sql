USE [BeamConfig];
GO

INSERT INTO	[core].Destination([Name], [ConnectionString])
VALUES('localhost', 'Server=FRCOGNOZBOOK\SQL16;Integrated Security=SSPI');
GO

INSERT INTO	[core].Task([Name], [Class], [Parameters], [Debug])
VALUES('wait 30 secs', 'SQLBeam.Core.Tasks.Executable.Wait.Wait', '30', 1);
GO 

INSERT INTO [core].[WaitingTasks]([Destination_ID], [Task_ID])
VALUES(1,1)
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


SELECT * FROM [core].[WaitingTasks];
SELECT * FROM  [core].[TaskPrerequisite];
