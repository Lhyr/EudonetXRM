
IF EXISTS (select name from sys.sysobjects where type = 'P' and name = 'xsp_getTableDescFromDescId') 
	DROP PROCEDURE dbo.xsp_getTableDescFromDescId
