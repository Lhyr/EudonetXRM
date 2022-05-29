ALTER  function [dbo].[getAutoBuildName]
(
	@d datetime,			-- Date crÃ©er le
	@sPM01 varchar(max),	-- SociÃ©tÃ©
	@sPP01 varchar(max),	-- Contacts.Nom
	@sPP02 varchar(max),	-- Contacts.PrÃ©nom
	@sPP03 varchar(max),	-- Contacts.PrÃ©nom
	@sMask varchar(50),		-- DisplayMask de la tabel [filedataparam]
	@sEVT01 varchar(max),
	@nEVTID numeric
) 
returns varchar(max)
as
begin
return [dbo].[getAutoBuildNameCI]( @d, @sPM01, @sPP01, @sPP02, @sPP03, @sMask, @sEVT01, @nEVTID, 103)
end




