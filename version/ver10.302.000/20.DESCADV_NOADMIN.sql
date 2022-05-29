
-- Campage
if not exists (select 1 from [descadv] where Parameter=8 and descid=106000)
BEGIN
	INSERT INTO [DESCADV] (Value, Parameter,Category,DescId)
	select 1, 8, 2, 106000
END
ELSE
BEGIN
	UPDATE [DESCADV] SET Value = 1 WHERE [DESCID] = 106000 AND Parameter = 8

END

-- Statistique campage
if not exists (select 1 from [descadv] where Parameter=8 and descid=111000)
BEGIN

	INSERT INTO [DESCADV] (Value, Parameter,Category,DescId)
	SELECT 1, 8, 2, 111000

END
ELSE
BEGIN
	UPDATE [DESCADV] SET Value = 1 WHERE [DESCID] = 111000 AND Parameter = 8
END


-- Notification
 if not exists (select 1 from [descadv] where Parameter=8 and descid=114000)
BEGIN

	INSERT INTO [DESCADV] (Value, Parameter,Category,DescId)
	SELECT 1, 8, 2, 114000

END
ELSE
BEGIN
	UPDATE [DESCADV] SET Value = 1 WHERE [DESCID] = 114000 AND Parameter = 8
END


 


-- NOTIFICATION_UNSUBSCRIBER
 if not exists (select 1 from [descadv] where Parameter=8 and descid=114300)
BEGIN

	INSERT INTO [DESCADV] (Value, Parameter,Category,DescId)
	SELECT 1, 8, 2, 114300

END
ELSE
BEGIN
	UPDATE [DESCADV] SET Value = 1 WHERE [DESCID] = 114300 AND Parameter = 8
END

/*

--XRM_HOMEPAGE
 if not exists (select 1 from [descadv] where Parameter=8 and descid=115000)
BEGIN

	INSERT INTO [DESCADV] (Value, Parameter,Category,DescId)
	SELECT 1, 8, 2, 115000

END
ELSE
BEGIN
	UPDATE [DESCADV] SET Value = 1 WHERE [DESCID] = 115000 AND Parameter = 8
END



--XRM_WIDGET
 if not exists (select 1 from [descadv] where Parameter=8 and descid=115100)
BEGIN

	INSERT INTO [DESCADV] (Value, Parameter,Category,DescId)
	SELECT 1, 8, 2, 115100

END
ELSE
BEGIN
	UPDATE [DESCADV] SET Value = 1 WHERE [DESCID] = 115100 AND Parameter = 8
END
*/

--FORMULARXRM
 if not exists (select 1 from [descadv] where Parameter=8 and descid=113000)
BEGIN

	INSERT INTO [DESCADV] (Value, Parameter,Category,DescId)
	SELECT 1, 8, 2, 113000

END
ELSE
BEGIN
	UPDATE [DESCADV] SET Value = 1 WHERE [DESCID] = 113000 AND Parameter = 8
END


--TRACKLINK
 if not exists (select 1 from [descadv] where Parameter=8 and descid=108000)
BEGIN

	INSERT INTO [DESCADV] (Value, Parameter,Category,DescId)
	SELECT 1, 8, 2, 108000

END
ELSE
BEGIN
	UPDATE [DESCADV] SET Value = 1 WHERE [DESCID] = 108000 AND Parameter = 8
END

 
--FILTER
 if not exists (select 1 from [descadv] where Parameter=8 and descid=104000)
BEGIN

	INSERT INTO [DESCADV] (Value, Parameter,Category,DescId)
	SELECT 1, 8, 2, 104000

END
ELSE
BEGIN
	UPDATE [DESCADV] SET Value = 1 WHERE [DESCID] = 104000 AND Parameter = 8
END


--REPORT
 if not exists (select 1 from [descadv] where Parameter=8 and descid=105000)
BEGIN

	INSERT INTO [DESCADV] (Value, Parameter,Category,DescId)
	SELECT 1, 8, 2, 105000

END
ELSE
BEGIN
	UPDATE [DESCADV] SET Value = 1 WHERE [DESCID] = 105000 AND Parameter = 8
END

--PJ
 if not exists (select 1 from [descadv] where Parameter=8 and descid=102000)
BEGIN

	INSERT INTO [DESCADV] (Value, Parameter,Category,DescId)
	SELECT 1, 8, 2, 102000

END
ELSE
BEGIN
	UPDATE [DESCADV] SET Value = 1 WHERE [DESCID] = 102000 AND Parameter = 8
END


--USER
 if not exists (select 1 from [descadv] where Parameter=8 and descid=101000)
BEGIN

	INSERT INTO [DESCADV] (Value, Parameter,Category,DescId)
	SELECT 1, 8, 2, 101000

END
ELSE
BEGIN
	UPDATE [DESCADV] SET Value = 1 WHERE [DESCID] = 101000 AND Parameter = 8
END

--BOUNCE
 if not exists (select 1 from [descadv] where Parameter=8 and descid=112000)
BEGIN

	INSERT INTO [DESCADV] (Value, Parameter,Category,DescId)
	SELECT 1, 8, 2, 112000

END
ELSE
BEGIN
	UPDATE [DESCADV] SET Value = 1 WHERE [DESCID] = 112000 AND Parameter = 8
END


--Notif Triger
 if not exists (select 1 from [descadv] where Parameter=8 and descid=114200)
BEGIN

	INSERT INTO [DESCADV] (Value, Parameter,Category,DescId)
	SELECT 1, 8, 2, 114200

END
ELSE
BEGIN
	UPDATE [DESCADV] SET Value = 1 WHERE [DESCID] = 114200 AND Parameter = 8
END



--Unsub Mail
 if not exists (select 1 from [descadv] where Parameter=8 and descid=110000)
BEGIN

	INSERT INTO [DESCADV] (Value, Parameter,Category,DescId)
	SELECT 1, 8, 2, 110000

END
ELSE
BEGIN
	UPDATE [DESCADV] SET Value = 1 WHERE [DESCID] = 110000 AND Parameter = 8
END



