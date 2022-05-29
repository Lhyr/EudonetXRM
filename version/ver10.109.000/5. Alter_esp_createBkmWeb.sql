ALTER PROCEDURE [dbo].[esp_CreateBkmWeb] @interpp bit, 
									@interpm bit, 
									@interevtdescid numeric, 									
									@bkmlabel varchar(255), 
									@url varchar(500),
									@descid numeric Output 
AS 
BEGIN
     -- 16 : type sugnet web
	EXEC esp_CreateVirtualTemplate @interpp, @interpm, @interevtdescid, 16, @bkmlabel, @url, @descid Output 
END
