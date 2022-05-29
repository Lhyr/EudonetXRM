IF NOT EXISTS (
		SELECT descid
		FROM [desc]
		WHERE descid = 102014
		)
BEGIN
	ALTER TABLE PJ ADD SecureLink VARCHAR(1) /*varchar 1 parce que c'est un champ factice donc pas la peine de prendre de la place pour rien*/

	INSERT INTO [DESC] (
		Descid
		,[File]
		,Field
		,format
		,Length
		,[Case]
		,ReadOnly
		,DispOrder
		,x
		,y
		,Colspan
		)
	SELECT 102014
		,'PJ'
		,'SecureLink'
		,7
		,255
		,0
		,0
		,16
		,0
		,9
		,2

	INSERT INTO RES (
		ResId
		,LANG_00
		,LANG_01
		,LANG_02
		,LANG_03
		,LANG_04
		,LANG_05
		)
	SELECT 102014
		,'Lien sécurisé'
		,'Secure link'
		,'Gesicherte Link'
		,'Beveiligde link'
		,'Vínculo seguro'
		,'Link sicuro'
END
