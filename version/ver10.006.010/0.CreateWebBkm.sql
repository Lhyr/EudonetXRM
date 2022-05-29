
IF EXISTS(SELECT * FROM sys.objects where name = 'esp_CreateBkmWeb' and type = 'P')
	DROP PROC [dbo].[esp_CreateBkmWeb]
