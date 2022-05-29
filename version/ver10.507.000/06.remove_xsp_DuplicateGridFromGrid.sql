IF EXISTS (select name from sys.sysobjects where type = 'P' and name = 'xsp_DuplicateGridFromGrid')
BEGIN
    DROP PROCEDURE xsp_DuplicateGridFromGrid;
END
