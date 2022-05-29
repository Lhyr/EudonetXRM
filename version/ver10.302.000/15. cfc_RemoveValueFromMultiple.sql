

/*******************************************************************************************************
KHA le 16/02/2017
Permet de retirer une valeur d'une liste en précisant le séparateur
********************************************************************************************************/
CREATE FUNCTION [dbo].[efc_RemoveValueFromMultiple] (
	@MainString AS VARCHAR(MAX)
	,@Value AS VARCHAR(255)
	,@Separator AS VARCHAR(5) = ';'
	)
RETURNS VARCHAR(MAX)
AS
BEGIN
	DECLARE @return AS VARCHAR(MAX)

	SET @return = REPLACE(@Separator + @MainString + @Separator, @Separator + @Value + @Separator, @Separator)

	IF LEN(@return) >= 2
		SET @return = SUBSTRING(@return, 2, LEN(@return) - 2)
	ELSE
		SET @return = NULL

	RETURN @return
END
