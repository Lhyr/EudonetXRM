
/*********************************************************************
	CREAT :	SPH 
	DATE  : 01/07/2016
	DESC  : obtient le hash d'une chaine suivant un algo qu'il est
				possible de préciser.

    PARAM :
				@value : chaine à haser
				@algo : algorithme de hashage ('MD2', 'MD4', 'MD5', 'SHA' , 'SHA1' ,'SHA2_256' ou 'SHA2_512')
				@base64 : encode en base 64. 
				@urlEncode : bool indiquant si la chaine de retour doit être url encodé
				@bitforcecompat : si l'aglo n'est pas compatible avec la version de sql, passe en sha1

	APPEL :
		Select dbo.[efc_GetHash]('PRIO', 'SHA2_512', 1 ,1 ,1)


****************************************************************************/
CREATE FUNCTION [dbo].[efc_GetHash]
(
   @value VARCHAR(max),
   @algo VARCHAR(10),
   @base64 bit ,
   @urlEncode BIT ,
   @bitforcecompat bit
)
 
 RETURNS VARCHAR(max)
 AS
 BEGIN

	SET @algo = UPPER(@algo)
 
	-- pour thrower un message d'erreur, on doit provoquer une erreur contenant le message : on peut pas throw dans une fonction
	IF @algo not in('MD2', 'MD4', 'MD5', 'SHA' , 'SHA1' ,'SHA2_256', 'SHA2_512')
		RETURN cast( 'L''algo de hashage doit être dans la liste : ''MD2'', ''MD4'', ''MD5'', ''SHA'' , ''SHA1'' ,''SHA2_256'', ''SHA2_512'' ' as int)

	IF	(SELECT SERVERPROPERTY('ProductVersion')) < '11'
		IF @algo not in('MD2', 'MD4', 'MD5', 'SHA' , 'SHA1' )
		BEGIN
			IF @bitforcecompat = 1
				set @algo = 'SHA1'
			else
				RETURN cast( 'L''algo de hashage doit être dans la liste : ''MD2'', ''MD4'', ''MD5'', ''SHA'' , ''SHA1''   ' as int)
 		END

	-- Page et argument
	DECLARE @returnvalue VARCHAR(max)

	-- Hash
	DECLARE @hash varbinary(max)

	-- Hash Base64
	DECLARE @hashed VARCHAR(max)

	-- Hash Base 64 url encodé
	DECLARE @encodehashed VARCHAR(max)

	-- HASH 
	SET @hash =  HASHBYTES( @algo, @value)

	if @HASH is null
		return cast('Impossible de calculer le hash' as int)
	
	-- Conversion base 64 Houpa
	if @base64 = 1
		SET @hashed =(convert(VARCHAR(max), CAST(N'' AS XML).value('xs:base64Binary(xs:hexBinary(sql:variable("@hash")))', 'nVARCHAR(MAX)'),2))
	else
		SET @hashed = CONVERT(varchar(max), @hash, 2)

	SET @encodehashed = @hashed


	-- Url encode HouPa
	IF @urlEncode = 1
	BEGIN

		-- URL Encode du HASH
		DECLARE @count INT, @c NCHAR(1), @i INT, @urlReturn VARCHAR(3072)
		SET @count = LEN(@hashed)
		SET @i = 1
		SET @encodehashed = ''    
		WHILE (@i <= @count)
		 BEGIN

			SET @c = SUBSTRING(@hashed, @i, 1)

			IF @c LIKE N'[A-Za-z0-9()''*\-._!~]' COLLATE Latin1_General_BIN ESCAPE N'\' COLLATE Latin1_General_BIN
			 BEGIN
				SET @encodehashed = @encodehashed + @c
			 END
			ELSE
			 BEGIN
				SET @encodehashed = 
					   @encodehashed + '%'
					   + SUBSTRING(sys.fn_varbintohexstr(CAST(@c AS VARBINARY(MAX))),3,2)
					   + ISNULL(NULLIF(SUBSTRING(sys.fn_varbintohexstr(CAST(@c AS VARBINARY(MAX))),5,2), '00'), '')
			 END
			SET @i = @i +1
		 END

	 END

 

	RETURN @encodehashed

 end
 


 