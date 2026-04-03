IF DB_ID(N'DeviceTrackr') IS NULL
BEGIN
    CREATE DATABASE [DeviceTrackr];
END;
GO

USE [DeviceTrackr];
GO

-- USERS TABLE
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Email NVARCHAR(255) NULL,
        PasswordHash NVARCHAR(255) NULL,
        Role NVARCHAR(200) NOT NULL,
        Location NVARCHAR(200) NOT NULL
    );
END;
GO

-- DEVICES TABLE
IF OBJECT_ID(N'dbo.Devices', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Devices
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Manufacturer NVARCHAR(200) NOT NULL,
        Type INT NOT NULL,
        OperatingSystem NVARCHAR(100) NOT NULL,
        OsVersion NVARCHAR(50) NOT NULL,
        Processor NVARCHAR(200) NOT NULL,
        RamAmountGb INT NOT NULL,
        Description NVARCHAR(2000) NOT NULL,
        AssignedUserId INT NULL
    );
END;
GO

-- if DB already existed, ensure new columns exist
IF COL_LENGTH('dbo.Users', 'Email') IS NULL
    ALTER TABLE dbo.Users ADD Email NVARCHAR(255) NULL;
GO

IF COL_LENGTH('dbo.Users', 'PasswordHash') IS NULL
    ALTER TABLE dbo.Users ADD PasswordHash NVARCHAR(255) NULL;
GO

IF COL_LENGTH('dbo.Devices', 'AssignedUserId') IS NULL
    ALTER TABLE dbo.Devices ADD AssignedUserId INT NULL;
GO


-- foreign key devices -> users
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Devices_Users_AssignedUserId'
)
BEGIN
    ALTER TABLE dbo.Devices
    ADD CONSTRAINT FK_Devices_Users_AssignedUserId
    FOREIGN KEY (AssignedUserId) REFERENCES dbo.Users(Id);
END;
GO
