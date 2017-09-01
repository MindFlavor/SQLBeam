SET NOCOUNT ON;

CREATE TABLE #sys_master_files(
	[database_id] INT,
	[database_name] SYSNAME,
	[file_id] INT,
	[type] BIT,
	[data_space_id] INT,
	[name] SYSNAME,
	[physical_name] NVARCHAR(MAX),
	[state] INT,
	[size] INT,
	[max_size] INT,
	[growth] INT,
	[is_read_only] BIT,
	[is_percent_growth] BIT,
	[space_used] INT
);

DECLARE @stmt NVARCHAR(MAX);

DECLARE curs CURSOR READ_ONLY FOR 
	SELECT [name] FROM sys.databases;


OPEN curs;

DECLARE @name SYSNAME;

FETCH NEXT FROM curs INTO @name;

WHILE @@FETCH_STATUS = 0
BEGIN

	SET @stmt = N'USE ' + QUOTENAME(@name)  + N';

		INSERT INTO #sys_master_files
			SELECT
				DB_ID(),
				DB_NAME() AS ''database_name'',
				[file_id],
				[type],
				[data_space_id],
				[name],
				[physical_name],
				[state],
				[size],
				[max_size],
				[growth],
				[is_read_only],
				[is_percent_growth],		
				FILEPROPERTY(name, ''SpaceUsed'') 
			FROM sys.database_files;
		';

	EXEC sp_executeSQL @stmt;

	FETCH NEXT FROM curs INTO @name;
END

CLOSE curs;
DEALLOCATE curs;

SELECT 
	* 
FROM #sys_master_files;

DROP TABLE #sys_master_files;