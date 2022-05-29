 SET NOCOUNT ON
UPDATE [RES] set 
	LANG_00 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2781 and mm.LANG_ID=0), LANG_00),
	LANG_01 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2781 and mm.LANG_ID=1), LANG_01),
	LANG_02 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2781 and mm.LANG_ID=2), LANG_02),
	LANG_03 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2781 and mm.LANG_ID=3), LANG_03),
	LANG_04 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2781 and mm.LANG_ID=4), LANG_04),
	LANG_05 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2781 and mm.LANG_ID=5), LANG_05),
	LANG_06 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2781 and mm.LANG_ID=6), LANG_06),
	LANG_07 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2781 and mm.LANG_ID=7), LANG_07),
	LANG_08 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2781 and mm.LANG_ID=8), LANG_08),
	LANG_09 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2781 and mm.LANG_ID=9), LANG_09)
WHERE RESID = 106010

 UPDATE [RES] set 
	LANG_00 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2782 and mm.LANG_ID=0), LANG_00),
	LANG_01 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2782 and mm.LANG_ID=1), LANG_01),
	LANG_02 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2782 and mm.LANG_ID=2), LANG_02),
	LANG_03 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2782 and mm.LANG_ID=3), LANG_03),
	LANG_04 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2782 and mm.LANG_ID=4), LANG_04),
	LANG_05 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2782 and mm.LANG_ID=5), LANG_05),
	LANG_06 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2782 and mm.LANG_ID=6), LANG_06),
	LANG_07 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2782 and mm.LANG_ID=7), LANG_07),
	LANG_08 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2782 and mm.LANG_ID=8), LANG_08),
	LANG_09 =   ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2782 and mm.LANG_ID=9), LANG_09)
WHERE RESID = 106025

 

 UPDATE [DESC] set ToolTipText = 
  ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2787 and mm.LANG_ID=0), ToolTipText)
WHERE DESCID = 106010

 UPDATE [DESC] set ToolTipText = 
  ISNULL( ( select top 1 nullif(lang,'') from EUDORES..RESAPP rr inner join MAPLANG mm on mm.LANG_SYSID=rr.Id_Lang where ResId = 2788 and mm.LANG_ID=0), ToolTipText)
WHERE DESCID = 106025

 SET NOCOUNT OFF