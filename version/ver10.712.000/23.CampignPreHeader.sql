-- Mise à jour des traductions pour le champ PreHeader

declare @idCampaignPreHeader as int = 106047

begin try
	begin tran

	MERGE Res AS tgt  
		USING (SELECT @idCampaignPreHeader as ResId, 'Texte d''aperçu' as Lang_00, 'Preview text' as Lang_01) as src(ResId, Lang_00, Lang_01)  
	ON (tgt.ResId = src.ResId)  
		WHEN MATCHED THEN
			UPDATE SET Lang_00 = src.Lang_00, Lang_01=src.Lang_01
		WHEN NOT MATCHED THEN  
			INSERT (ResId, Lang_00, Lang_01)  
			VALUES (src.ResId, src.Lang_00, src.Lang_01);
	commit tran;
end try
begin catch
	rollback tran;
	
	DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();  
	DECLARE @ErrorSeverity INT = ERROR_SEVERITY();  
	DECLARE @ErrorState INT = ERROR_STATE();  

	RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState); 
end catch