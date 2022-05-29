DECLARE @ConfParam as varchar(255), @Value as varchar(255)

Select @ConfParam = 'DEFAULT_CURRENCY', @Value = CurrenCyCode 
FROM EDNCOUNTRIES Inner Join CONFIGADV on cast(EDNCOUNTRIES.Id as Varchar(255)) = CONFIGADV.Value
WHERE Parameter = 'COUNTRY'

IF NOT EXISTS (SELECT Id FROM CONFIGADV WHERE Parameter = @ConfParam)  AND (@Value IS NOT NULL)
	INSERT INTO CONFIGADV (Parameter, Value)
	SELECT @ConfParam, @Value 