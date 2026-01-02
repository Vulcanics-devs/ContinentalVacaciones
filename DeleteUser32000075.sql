-- Delete user CANO VALLADOLID MIGUEL ANGEL (Nomina: 32000075)
-- This user doesn't have Empleado or Sindicalizado records

DECLARE @UserId INT;
SELECT @UserId = Id FROM Users WHERE Nomina = 32000075;

IF @UserId IS NOT NULL
BEGIN
    PRINT 'Deleting user with ID: ' + CAST(@UserId AS VARCHAR(10));

    -- Delete from UserRoles
    DELETE FROM UserRoles WHERE UserId = @UserId;
    PRINT 'Deleted from UserRoles';

    -- Delete from Users
    DELETE FROM Users WHERE Id = @UserId;
    PRINT 'Deleted from Users';

    PRINT 'User 32000075 (CANO VALLADOLID MIGUEL ANGEL) deleted successfully';
END
ELSE
BEGIN
    PRINT 'User with Nomina 32000075 not found';
END

-- Verify deletion
SELECT COUNT(*) as RemainingUsers FROM Users WHERE Nomina = 32000075;
