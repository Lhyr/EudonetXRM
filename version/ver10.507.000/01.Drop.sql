
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = 'efc_GetIndex')
    EXEC ('CREATE FUNCTION dbo.efc_GetIndex() returns table as return select 1 a  ')
 
 
 
 IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = 'efc_GetIndexScripts')
    EXEC ('CREATE FUNCTION dbo.efc_GetIndexScripts() returns table as return select 1 a  ')
 

  IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = 'efc_GetIndexScriptsNonDesc')
    EXEC ('CREATE FUNCTION dbo.efc_GetIndexScriptsNonDesc() returns table as return select 1 a  ')
 