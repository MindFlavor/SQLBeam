SELECT [name] FROM sys.tables
WHERE 
	[name] = @name
	AND [type] = 'U'