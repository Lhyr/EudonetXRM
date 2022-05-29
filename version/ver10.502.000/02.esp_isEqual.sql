/********************************************************************* 
* PROCEDURE : 	esp_isEqual 
* DESCRIPTION : Teste l'égalité entre deux valeurs suivant leur format 
* CREATION : 	Nicolas Berrez le 21/05/2003
* MODIFICATION : CNA le 13/02/2019 - Pour les formats AUTOINC/MONEY/NUMERIC, on cast en numeric(38,19) afin de couvrir tous les cas possibles de configuration de rubriques (money correspond à un numeric(19,4) )
**********************************************************************/ 
ALTER procedure [dbo].[esp_isEqual]( @strVal1 varchar(8000), @strVal2 varchar(8000), @nFormat numeric(18,0) )
AS
declare @strValue1 varchar(8000)
declare @strValue2 varchar(8000)
SET @strValue1 = ISNULL( @strVal1, '' )
SET @strValue2 = ISNULL( @strVal2, '' )
--DATE
if @nFormat = 2
	BEGIN
	if @strValue1 = @strValue2
		return 1
	END
--BIT
else if @nFormat = 3
	BEGIN
	if CAST( @strValue1 AS BIT ) = CAST( @strValue2 AS BIT )
		return 1
	END
--AUTOINC, MONEY, NUMERIC
else if @nFormat = 4 OR	@nFormat = 5 OR @nFormat = 10
	BEGIN
	if IsNumeric( @strValue1 ) = 1 AND IsNumeric( @strValue2 ) = 1 
		BEGIN 
		if CAST( @strValue1 AS NUMERIC(38,19) ) = CAST( @strValue2 AS NUMERIC(38,19) ) 
			return 1 
		END 
	else if NOT ( IsNumeric( @strValue1 ) = 1 AND IsNumeric( @strValue2 ) = 1 ) 
		return 1 
	END      
else     
	if @strValue1 = @strValue2 
		return 1 
return 0 