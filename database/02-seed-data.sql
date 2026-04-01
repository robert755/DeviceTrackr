USE [DeviceTrackr];
GO

MERGE [dbo].[Users] AS T
USING (VALUES
    (N'Ana Popescu', N'IT Admin', N'Bucuresti'),
    (N'Mihai Ionescu', N'Developer', N'Cluj'),
    (N'Elena Georgescu', N'Manager', N'Timisoara')
) AS S ([Name], [Role], [Location])
ON T.[Name] = S.[Name]
WHEN NOT MATCHED THEN
    INSERT ([Name], [Role], [Location])
    VALUES (S.[Name], S.[Role], S.[Location]);
GO

MERGE [dbo].[Devices] AS T
USING (VALUES
    (N'iPhone 15', N'Apple', 1, N'iOS', N'18.2', N'A17 Pro', 8, N'Device pentru echipa Sales'),
    (N'Galaxy Tab S9', N'Samsung', 2, N'Android', N'14', N'Snapdragon 8 Gen 2', 12, N'Tableta pentru demo'),
    (N'Pixel 8', N'Google', 1, N'Android', N'15', N'Tensor G3', 8, N'Device pentru testare')
) AS S ([Name], [Manufacturer], [Type], [OperatingSystem], [OsVersion], [Processor], [RamAmountGb], [Description])
ON T.[Name] = S.[Name]
WHEN NOT MATCHED THEN
    INSERT ([Name], [Manufacturer], [Type], [OperatingSystem], [OsVersion], [Processor], [RamAmountGb], [Description])
    VALUES (S.[Name], S.[Manufacturer], S.[Type], S.[OperatingSystem], S.[OsVersion], S.[Processor], S.[RamAmountGb], S.[Description]);
GO
