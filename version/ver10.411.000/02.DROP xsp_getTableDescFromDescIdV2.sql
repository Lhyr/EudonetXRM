
IF EXISTS (select name from sys.sysobjects where type = 'P' and name = 'xsp_getTableDescFromDescIdV2') 
	DROP PROCEDURE dbo.xsp_getTableDescFromDescIdV2
