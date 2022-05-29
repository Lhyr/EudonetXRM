CREATE PROCEDURE [dbo].[esp_CreateVirtualTemplate] @interpp bit, 
									@interpm bit, 
									@interevtdescid numeric, 
									@fileType numeric, 
									@label varchar(255), 
									@url varchar(500),
									@descid numeric Output 
AS
BEGIN
	--SELECT @interpp = 1
	--	 , @interpm = 1
	--	 , @interevtdescid = 4400
	--	 , @url = 'https://www.google.fr/maps/place/Lieusaint/@48.627694,2.548974,13z/data=!3m1!4b1!4m2!3m1!1s0x47e5e3be3e4b6955:0x13e90cb0060018cf'
	--	 , @BkmLabel = 'Maps'



PRINT 'Execution de esp_CreateVirtualTemplate'

IF @interevtdescid > 0  AND NOT EXISTS (SELECT DescId FROM [DESC] WHERE DescId = @interevtdescid and [File] like 'EVENT%') 
	SET @interevtdescid = 0
		
SELECT TOP 1 @descid = DescId + 100
FROM
	[DESC]
WHERE
	DescId % 100 = 0
	AND DescId + 100 BETWEEN 1100 AND 99000
	AND NOT EXISTS (SELECT DescId
					FROM
						[DESC] d2
					WHERE
						d2.DescId = [DESC].DescId + 100)

	IF ISNULL(@descid,0) = 0 
	BEGIN
	    PRINT 'Pas de descid disponible entre 1100 et 99000'
		RETURN
	END

PRINT 'NOUVEAU DescId ' 

-- On nétoie les pref, res et desc
PRINT 'Nétoyage des table PREF, RES, DESC'
delete  from pref where tab = @descid
delete  from res where resid = @descid
delete  from [DESC] where descid = @descid

PRINT 'Insertion DESC'
INSERT INTO [DESC] (DescId
				  , [File]
				  , Field
				  , [Type]
				  , [Default]
				  , InterPp
				  , InterPm
				  , InterEvent
				  , InterEventNum)

VALUES
	(@descid, 'TEMPLATE_' + cast(cast(@descid / 100 - 10 AS NUMERIC) AS VARCHAR(10)), 'TPL', @fileType, @url, @interpp, @interpm, CASE
		WHEN @interevtdescid = 100 THEN
			1
		WHEN @interevtdescid > 1000 AND @interevtdescid % 100 = 0 THEN
			1
		ELSE
			0
	END, CASE
		WHEN @interevtdescid = 100 THEN
			0
		WHEN @interevtdescid > 1000 AND @interevtdescid % 100 = 0 THEN
			((@interevtdescid / 100) - 10)
		ELSE
			NULL
	END)

	PRINT 'Insertion RES'
INSERT INTO RES (ResId
			   , LANG_00)
VALUES
	(@descid, @label)

PRINT 'Insertion PREF'
INSERT INTO PREF (Tab
				, UserId)
SELECT d
	 , u
FROM
	((SELECT @descid d
		   , UserId u
	  FROM
		  [USER]) UNION
	 (SELECT @descid d
		   , 0 u)) AS T	
	
END
PRINT 'Execution de esp_CreateVirtualTemplate terminée'
