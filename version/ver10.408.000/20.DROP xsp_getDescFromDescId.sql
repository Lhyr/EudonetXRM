
IF EXISTS (select name from sys.sysobjects where type = 'P' and name = 'xsp_getDescFromDescId') 
	DROP PROCEDURE dbo.xsp_getDescFromDescId
