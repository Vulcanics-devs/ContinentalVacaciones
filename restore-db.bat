@echo off
if not exist C:\SQLBackup mkdir C:\SQLBackup
copy "Database back\Vacaciones.bak" C:\SQLBackup\
sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q "USE master; ALTER DATABASE Vacaciones SET SINGLE_USER WITH ROLLBACK IMMEDIATE; RESTORE DATABASE Vacaciones FROM DISK = 'C:\SQLBackup\Vacaciones.bak' WITH REPLACE; ALTER DATABASE Vacaciones SET MULTI_USER;"
