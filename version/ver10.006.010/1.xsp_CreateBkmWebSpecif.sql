/***********************************************************************************************************
KHA le 15/07/2014
Procédure permettant de créer un signet web
***********************************************************************************************************/

CREATE PROC [dbo].[xsp_CreateBkmWebSpecif] @interpp bit, 
									@interpm bit, 
									@interevtdescid numeric, 
									@bkmlabel varchar(255), 
									@url varchar(500),
									@descid numeric Output 
									
AS
BEGIN

Declare @SpecifId as Numeric
EXEC dbo.xsp_AddSpecif @specType = 5, 
				@openMode = 3, 
				@label = @bkmlabel, 
				@tab=0, @Url = @url, 
				@urlParam = '', 
				@specId = @SpecifId OUT
				
EXEC dbo.esp_CreateBkmWeb @interpp, 
						@interpm, 
						@interevtdescid, 
						@bkmlabel, 
						@SpecifId, 
						@descid OUT
						
						
UPDATE SPECIFS SET Tab = @descid where SpecifId = @SpecifId						


END									