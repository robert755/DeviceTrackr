IF DB_ID(N'DeviceTrackr') IS NULL
BEGIN
    CREATE DATABASE [DeviceTrackr];
END;
GO

USE [DeviceTrackr];
GO

IF OBJECT_ID(N'dbo.Devices', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Devices]
    (
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Devices] PRIMARY KEY,
        [Name] NVARCHAR(200) NOT NULL,
        [Manufacturer] NVARCHAR(200) NOT NULL,
        [Type] INT NOT NULL,
        [OperatingSystem] NVARCHAR(100) NOT NULL,
        [OsVersion] NVARCHAR(50) NOT NULL,
        [Processor] NVARCHAR(200) NOT NULL,
        [RamAmountGb] INT NOT NULL,
        [Description] NVARCHAR(2000) NOT NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Users]
    (
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Users] PRIMARY KEY,
        [Name] NVARCHAR(200) NOT NULL,
        [Role] NVARCHAR(200) NOT NULL,
        [Location] NVARCHAR(200) NOT NULL
    );
END;
GO
