USE [DeviceTrackr];
GO

-- USERS (idempotent)
MERGE dbo.Users AS T
USING (VALUES
    (N'Ana Popescu',   N'ana@example.com',   N'IT Admin',  N'Bucuresti', N'6CgnsAssqGIL6zf4eXeMCCspKlInA5DP81tv4xV/Tos='),
    (N'Mihai Ionescu', N'mihai@example.com', N'Developer', N'Cluj',      N'TNgjp7OQcK4MsDTyHezHxtiAbkRpVD6N8zTaCGXx930='),
    (N'Elena Georgescu', N'elena@example.com', N'Manager', N'Timisoara', N'Chyw461hYqvM7nY49FsVpj+Ky7uGKF6kh79iSHIPWMw=')
) AS S (Name, Email, Role, Location, PasswordHash)
ON T.Email = S.Email
WHEN NOT MATCHED THEN
    INSERT (Name, Email, Role, Location, PasswordHash)
    VALUES (S.Name, S.Email, S.Role, S.Location, S.PasswordHash)
WHEN MATCHED THEN
    UPDATE SET
        T.Name = S.Name,
        T.Role = S.Role,
        T.Location = S.Location,
        T.PasswordHash = S.PasswordHash;
GO

-- DEVICES (idempotent)
MERGE dbo.Devices AS T
USING (VALUES
    (N'iPhone 15',    N'Apple',   1, N'iOS',     N'18.2', N'A17 Pro',              8,  N'Device pentru echipa Sales'),
    (N'Galaxy Tab S9',N'Samsung', 2, N'Android', N'14',   N'Snapdragon 8 Gen 2',  12, N'Tableta pentru demo'),
    (N'Pixel 8',      N'Google',  1, N'Android', N'15',   N'Tensor G3',             8, N'Device pentru testare')
) AS S (Name, Manufacturer, Type, OperatingSystem, OsVersion, Processor, RamAmountGb, Description)
ON T.Name = S.Name
WHEN NOT MATCHED THEN
    INSERT (Name, Manufacturer, Type, OperatingSystem, OsVersion, Processor, RamAmountGb, Description)
    VALUES (S.Name, S.Manufacturer, S.Type, S.OperatingSystem, S.OsVersion, S.Processor, S.RamAmountGb, S.Description);
GO

-- demo assignment: iPhone 15 -> Ana
UPDATE D
SET D.AssignedUserId = U.Id
FROM dbo.Devices D
INNER JOIN dbo.Users U ON U.Email = 'ana@example.com'
WHERE D.Name = 'iPhone 15' AND D.AssignedUserId IS NULL;
GO

-- demo login credentials:
-- ana@example.com   / ana123
-- mihai@example.com / mihai123
-- elena@example.com / elena123
