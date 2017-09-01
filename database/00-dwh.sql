CREATE DATABASE [BeamDWH];
GO

USE [BeamDWH];
GO

CREATE SCHEMA [tsql];
GO

SELECT * FROM sys.objects

SELECT * FROM sys.server_principals

CREATE TABLE [sys_sql_logins] (
	[Server] NVARCHAR(255),
	[InsertTime] DATETIME,
	[name] NVARCHAR(255),
	[sid] VARBINARY(MAX),
);

CREATE TABLE [sys_objects] (
	[Server] NVARCHAR(255),
	[InsertTime] DATETIME,
	[name] NVARCHAR(255),
);


--DROP TABLE [sys_master_files];
--GO
CREATE TABLE [sys_master_files] (
	[Server] NVARCHAR(255),	
	[InsertTime] DATETIME,
	[ExecutionID] INT,
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
GO

CREATE INDEX idx_sys_master_files_InsertTime ON [sys_master_files]([InsertTime]);
GO


SELECT * FROM [BeamDWH].[dbo].sys_objects;

SELECT * FROM [BeamDWH].[dbo].sys_sql_logins;

SELECT * FROM [BeamDWH].[dbo].[sys_master_files];


USE [master];
GO

SELECT DISTINCT [database_name] FROM [BeamDWH].dbo.[sys_master_files];

DELETE FROM [BeamDWH].dbo.[sys_master_files] WHERE [database_name] NOT IN ('AdventureWorks2016CTP3', 'alwaysEnc');