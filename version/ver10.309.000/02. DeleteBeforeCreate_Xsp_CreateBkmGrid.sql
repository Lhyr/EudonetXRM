-- On supprime la proc si elle existe pour en créer une nouvelle
IF EXISTS(SELECT * FROM sys.objects where name = 'xsp_CreateBkmGridSpecif' and type = 'P')
	DROP PROC [dbo].[xsp_CreateBkmGridSpecif]

			