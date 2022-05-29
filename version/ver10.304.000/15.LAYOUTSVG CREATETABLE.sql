
IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[LayOutSvg]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	CREATE TABLE [dbo].[LayoutSvg] (
		SvgId Int Identity PRIMARY KEY,

	     -- tab
		Tab Int NOT NULL,  
		
		-- libellé de la sauvegarde
		Label VARCHAR(255) NOT NULL,
		
		--Date de sauvegarde de la mise en page
		CreatedOn DateTime,
		
		-- Widget pour un utilisateur
		UserId numeric(18, 0) NULL,
		
		
	) ON [PRIMARY]	
END


IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[LayoutSvgFields]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	CREATE TABLE [dbo].[LayoutSvgFields] (
		SvgId Int,

        -- descid
		DescId Int NOT NULL,  

		-- Position du widget pour le user	
        X int NULL,	
        Y int NULL,
        Disporder int NULL,
		
		-- largeur/hauteur pour le user en cours	
        Colspan int NULL,	
        Rowspan int NULL, 
		
	) ON [PRIMARY]	
END
