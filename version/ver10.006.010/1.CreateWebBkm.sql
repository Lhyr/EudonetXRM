

/***********************************************************************************************************
KHA le 19/03/2014
Procédure permettant de créer un signet web
***********************************************************************************************************/

CREATE PROC [dbo].[esp_CreateBkmWeb] @interpp bit, 
									@interpm bit, 
									@interevtdescid numeric, 
									@bkmlabel varchar(255), 
									@url varchar(500),
									@descid numeric Output 
AS
BEGIN
	--SELECT @interpp = 1
	--	 , @interpm = 1
	--	 , @interevtdescid = 4400
	--	 , @url = 'https://www.google.fr/maps/place/Lieusaint/@48.627694,2.548974,13z/data=!3m1!4b1!4m2!3m1!1s0x47e5e3be3e4b6955:0x13e90cb0060018cf'
	--	 , @BkmLabel = 'Maps'


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
		RETURN
	END

INSERT INTO [DESC] (DescId
				  , [File]
				  , Field
				  , Type
				  , [Default]
				  , InterPp
				  , InterPm
				  , InterEvent
				  , InterEventNum)

VALUES
	(@descid, 'TEMPLATE_' + cast(cast(@descid / 100 - 10 AS NUMERIC) AS VARCHAR(10)), 'TPL', 16, @url, @interpp, @interpm, CASE
		WHEN @interevtdescid > 0 AND @interevtdescid % 100 = 0 THEN
			1
		ELSE
			0
	END, CASE
		WHEN @interevtdescid = 100 THEN
			0
		WHEN @interevtdescid > 0 AND @interevtdescid % 100 = 0 THEN
			((@interevtdescid / 100) - 10)
		ELSE
			NULL
	END)


INSERT INTO RES (ResId
			   , LANG_00)
VALUES
	(@descid, @bkmlabel)


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