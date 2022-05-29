
IF EXISTS(SELECT * FROM sys.objects where name = 'xsp_CreateBkmWebSpecif' and type = 'P')
	DROP PROC [dbo].[xsp_CreateBkmWebSpecif]
