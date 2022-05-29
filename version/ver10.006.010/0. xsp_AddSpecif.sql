
IF EXISTS(SELECT * FROM sys.objects where name = 'xsp_AddSpecif' and type = 'P')
	DROP PROC [dbo].[xsp_AddSpecif]
