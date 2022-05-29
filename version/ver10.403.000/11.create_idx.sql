
/****************************************************
 Création d'index d'optimisation
les index nes sont pas créés si un index simimlaire existe déjà (même colonnes indexé et même colonnes incluse)

*/

select '1'

/*
-- IDX USER
IF NOT EXISTS (SELECT 1 FROM SYS.indexes where name='USER_IDX_XRMBASEIDX' AND  OBJECT_NAME(object_id) = 'USER')
	AND  [dbo].[efc_IsIndexExists]('USER','USERID','GROUPID;USERLEVEL;USERLOGIN') = 0
BEGIN 

 

	CREATE NONCLUSTERED INDEX [USER_IDX_XRMBASEIDX] ON [dbo].[USER]
		([UserId] ASC)
		INCLUDE ([GroupId],	[UserLevel],[UserLogin]) 
 END
 

 -- IDX TRAIT
 IF NOT EXISTS (SELECT 1 FROM SYS.indexes where name='TRAIT_IDX_XRMBASEIDX' AND  OBJECT_NAME(object_id) = 'TRAIT')
	AND  [dbo].[efc_IsIndexExists]('TRAIT','TraitId','RulesId') = 0
 BEGIN
	 CREATE NONCLUSTERED INDEX TRAIT_IDX_XRMBASEIDX ON [dbo].[TRAIT]
	([TraitId] ASC)
	INCLUDE ([RulesId]) 
END

 -- IDX USERVALUE
IF  EXISTS (SELECT 1 FROM SYS.indexes where name='IDX_USERVALUE_TAB_TYPE_USERID_V7408' AND  OBJECT_NAME(object_id) = 'USERVALUE') 
	DROP INDEX [IDX_USERVALUE_TAB_TYPE_USERID_V7408] ON [dbo].[USERVALUE]
 

IF NOT EXISTS (SELECT 1 FROM SYS.indexes where name='IDX_USERVALUE_XRMBASEIDX' AND  OBJECT_NAME(object_id) = 'USERVALUE') 
		AND    [dbo].[efc_IsIndexExists]('USERVALUE','Tab;UserId;Type','Label;Value;Index')=0
BEGIN
	CREATE NONCLUSTERED INDEX [IDX_USERVALUE_XRMBASEIDX] ON [dbo].[USERVALUE]
	([Tab] ASC,	[Type] ASC,	[UserId] ASC)
	INCLUDE ([Value],[Label],[Index]) 
END


IF NOT EXISTS (SELECT 1 FROM SYS.indexes where name='IDX_CONFIG_XRMBASEIDX' AND  OBJECT_NAME(object_id) = 'CONFIG') 
BEGIN

	CREATE NONCLUSTERED INDEX IDX_CONFIG_XRMBASEIDX ON [dbo].[CONFIG]
		([UserId] ASC)
		INCLUDE (
		[Version],[Group],[Product],[KeyCommon],[SystemDate],[SessionTimeout],
		[SessionResetPref],[LogoName],[LogSimultEnabled],[LogSimult],[SMTPServerName],
		[AppName],[FeedBackEnabled],[HtmlEnabled],[SynchroEnabled],[HelpDeskCopy],
		[DatasPath],[ConditionalSendEnabled],[TotalCount],[Theme],[SmtpFeedBack],
		[SearchUnicodeDisabled],[BannerName],[MarkedFileEnabled]
	)

END
*/