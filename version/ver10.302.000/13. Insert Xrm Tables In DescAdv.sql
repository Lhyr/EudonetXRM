DECLARE @Parameter AS INT
DECLARE @Value AS VARCHAR(56)
DECLARE @Category AS INT

SELECT @Parameter = 6 /* XRMONLY */
	,@Value = 1
	,@Category = 2 /* SYSTEM */

INSERT INTO DESCADV (
	DescId
	,Parameter
	,Value
	,Category
	)
SELECT DescId
	,@Parameter
	,@Value
	,@Category
FROM (
	SELECT 102000 AS DescId /*PJ*/
	
	UNION
	
	SELECT 114000 AS DescId /*NOTIFICATION*/
	
	UNION
	
	SELECT 106000 AS DescId /*CAMPAIGN*/
	
	UNION
	
	SELECT 107000 AS DescId /*MAIL_TEMPLATE*/
	
	UNION
	
	SELECT 108000 AS DescId /*TRACKLINK*/
	
	UNION
	
	SELECT 109000 AS DescId /*TRACKLINKLOG*/
	
	UNION
	
	SELECT 110000 AS DescId /*UNSUBSCRIBEMAIL*/
	
	UNION
	
	SELECT 111000 AS DescId /*CAMPAIGNSTATS*/
	
	UNION
	
	SELECT 112000 AS DescId /*BOUNCEMAIL*/
	
	UNION
	
	SELECT 113000 AS DescId /*FORMULARXRM*/
	
	UNION
	
	SELECT 114000 AS DescId /*NOTIFICATION*/
	
	UNION
	
	SELECT 114100 AS DescId /*NOTIFICATION_TRIGGER_RES*/
	
	UNION
	
	SELECT 114200 AS DescId /*NOTIFICATION_TRIGGER*/
	
	UNION
	
	SELECT 114300 AS DescId /*NOTIFICATION_UNSUBSCRIBER*/
	
--  La partie ci-dessous est dsactivée car les tables sont supprimées et recréées ce qui provoque des bugs à cause de la contrainte de clé étrangère.
	
--	UNION
	
--	SELECT 115000 AS DescId /*XRM_HOMEPAGE*/
	
--	UNION
	
--	SELECT 115100 AS DescId /*XRM_WIDGET*/
	
	
	) Descids
WHERE NOT EXISTS (
		SELECT Id
		FROM DESCADV
		WHERE DESCADV.DescId = Descids.DescId
			AND Category = @Category
			AND Parameter = @Parameter
		)
	AND EXISTS (
		SELECT DescId
		FROM [DESC]
		WHERE [DESC].DescId = Descids.DescId
		)
