-- One-time, administrator-reviewed staging setup. Register your own account first.
-- Set the exact account email below; never add a password to this file.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @AdminEmail nvarchar(256) = NULL;
IF @AdminEmail IS NULL THROW 50000, 'Set the registered administrator email before running.', 1;
IF (SELECT COUNT(*) FROM dbo.AspNetUsers WHERE NormalizedEmail = UPPER(@AdminEmail)) <> 1
    THROW 50001, 'Expected exactly one registered account.', 1;
DECLARE @UserId nvarchar(450), @RoleId nvarchar(450);
SELECT @UserId=Id FROM dbo.AspNetUsers WHERE NormalizedEmail=UPPER(@AdminEmail);
SELECT @RoleId=Id FROM dbo.AspNetRoles WHERE NormalizedName='ADMIN';
IF @RoleId IS NULL THROW 50002, 'Run application role initialization first.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles WHERE UserId=@UserId AND RoleId=@RoleId)
    INSERT dbo.AspNetUserRoles(UserId,RoleId) VALUES (@UserId,@RoleId);
UPDATE dbo.AspNetUsers SET SecurityStamp=CONVERT(nvarchar(36),NEWID()),
    ConcurrencyStamp=CONVERT(nvarchar(36),NEWID()) WHERE Id=@UserId;
COMMIT;
SELECT u.Email,r.Name FROM dbo.AspNetUsers u JOIN dbo.AspNetUserRoles ur ON ur.UserId=u.Id
JOIN dbo.AspNetRoles r ON r.Id=ur.RoleId WHERE u.Id=@UserId;
