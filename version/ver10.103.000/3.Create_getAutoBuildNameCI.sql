CREATE  function [dbo].[getAutoBuildNameCI]
(
	@d datetime,			-- Date crÃ©er le
	@sPM01 varchar(max),	-- SociÃ©tÃ©
	@sPP01 varchar(max),	-- Contacts.Nom
	@sPP02 varchar(max),	-- Contacts.PrÃ©nom
	@sPP03 varchar(max),	-- Contacts.PrÃ©nom
	@sMask varchar(50),		-- DisplayMask de la tabel [filedataparam]
	@sEVT01 varchar(max),
	@nEVTID numeric,
	@nDateFormat int  -- Culture info
) 
returns varchar(max)
as
begin

if @nEVTID IS NULL
	return null


declare @resultat varchar(max)
declare @tmp varchar(max)

If ISNULL(@sEVT01,'')<>''
	return @sEVT01

set @resultat = @sMask
-- Reprend le Nom Prenom du contact
set @resultat = replace(@resultat, '$201$', ltrim(rtrim(isnull(@sPP03 + ' ','') +  isnull(@sPP01,'') +' '+isnull(@sPP02,''))))
-- Reprend la SociÃ©tÃ©
set @resultat = replace(@resultat, '$301$', isnull(@sPM01,''))

set @tmp = @resultat
set @tmp = replace(@tmp, '-', '')
set @tmp = replace(@tmp, '$95$', '')
set @tmp = rtrim(ltrim(@tmp))

-- Supprime les '-' en trop
set @resultat = replace(ltrim(rtrim(@resultat)), '  ', ' ')
IF left(@resultat,1) = '-'
	set @resultat = ltrim(rtrim(right(@resultat, len(@resultat)-1)))
IF left(@resultat,1) = '-'
	set @resultat = ltrim(rtrim(right(@resultat, len(@resultat)-1)))
IF right(@resultat,1) = '-'
	set @resultat = ltrim(rtrim(left(@resultat, len(@resultat)-1)))
IF right(@resultat,1) = '-'
	set @resultat = ltrim(rtrim(left(@resultat, len(@resultat)-1)))
set @resultat = replace(ltrim(rtrim(@resultat)), '- -', '-')

-- Si la rubrique "crÃ©er le" est null ou la rubrique n'est pas prÃ©sente dans le masque)
IF @d IS NULL OR charindex('$95$',@sMask) = 0
BEGIN
	IF @tmp <> ''
		return rtrim(ltrim(@tmp))
	ELSE
		return ''
END

-- Retour la date crÃ©er le si PP et PM vide
IF @tmp = ''
	return convert(varchar(10),@d,@nDateFormat)

set @resultat = replace(@resultat, '$95$', convert(varchar(10),@d,@nDateFormat))
return @resultat

end

