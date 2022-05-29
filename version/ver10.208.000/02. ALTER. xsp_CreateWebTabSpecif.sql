 
CREATE PROC [dbo].[xsp_CreateWebTabSpecif] 
                                    @specType numeric, -- type de specif
								    @specLabel varchar(255), -- libellé de sous-onglet 
									@nTab numeric, -- table de la specif,  null si creation 
									@webTabLabel varchar(255), -- si tab null , ajouter le libellé de l'ongle	                                  															
									@url varchar(500),-- url de la specif
									@urlParam varchar(500),-- url de la specif
									@descid numeric Output -- descid de la table crée si nTab=0
									
AS
BEGIN

Declare @SpecifId as Numeric
Declare @Param as Varchar 

set @Param = @urlParam
if(@urlParam IS NULL)
	set @Param = ''

-- On cree la specif
EXEC dbo.xsp_AddSpecif @specType, 
				@openMode = 3, 
				@label = @specLabel, 
				@tab = @nTab, 
				@Url = @url, 
				@urlParam = @Param , 
				@specId = @SpecifId OUT
		

 
	
IF(@nTab IS NULL)
BEGIN
 
  
  -- On crée une table virtuelle	
  EXEC dbo.esp_CreateVirtualTemplate 0, 
						0, 
						0,
                        19, -- Onglet web		
						@webTabLabel, 
						@SpecifId, 
						@descid OUT
						
    --On attache la table virtuelle a la specifs et on l'active dans desc
	UPDATE SPECIFS SET Tab = @descid where SpecifId = @SpecifId	
	UPDATE [DESC] SET ActiveTab = 1 where DescId = @descid	
END		
	

-- Specif de type web sera ouverte dans une iframe 12 : corespond a une url externe
if(@specType = 12 and CHARINDEX('http://', @url) > 0)
     PRINT 'AVERTISSMENT : Les URLs en http sont systématiquement bloqués par les navigateurs dans une application host utilisant du protocole SSL.'     

END	

-- Pour les onglets WEB, on retourne l'id de la spécifs
IF @specType=11 OR @specType = 12
	set @descid = @SpecifId
