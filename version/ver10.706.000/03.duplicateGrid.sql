
ALTER PROCEDURE [dbo].[xsp_DuplicateGridFromGrid] (
	@iTab AS INT,
	@iGrid AS INT,
	@nAddTitre AS NVARCHAR(255),
	@IFile AS INT = NULL
) AS
BEGIN
	DECLARE @iNewGrid AS INT;
	DECLARE @tabWidget AS TABLE(idOldWidget INT, idNewWidget INT)
	DECLARE @iViewPermId AS INT, @iUpdatePermId AS INT, @iDisplayOrder AS INT;
	DECLARE @iViewPermIdOld AS INT, @iUpdatePermIdOld AS INT;

	BEGIN TRY
		BEGIN TRAN

		SELECT @iViewPermIdOld = ISNULL(ViewPermId,0), @iUpdatePermIdOld = ISNULL(UpdatePermId,0) FROM [dbo].[XrmGrid]
		WHERE XrmGridId = @iGrid

		/** Si ViewPermId est différent de 0, on indente celui-ci pour éviter la modification
		  * des permissions de plusieurs onglets en même temps. */
		IF(@iViewPermIdOld <> 0)
			BEGIN
				INSERT INTO [dbo].[PERMISSION]([Level],Mode,[User]) 
				SELECT [Level],Mode,[User] FROM [dbo].[PERMISSION]
				WHERE PermissionId = @iViewPermIdOld
				SELECT @iViewPermId = IDENT_CURRENT('PERMISSION')
			END

		/** Si UpdatePermId est différent de 0, on indente celui-ci pour éviter la modification
		  * des permissions de plusieurs onglets en même temps. */
		IF(@iUpdatePermIdOld <> 0)
			BEGIN
				INSERT INTO [dbo].[PERMISSION]([Level],Mode,[User]) 
				SELECT [Level],Mode,[User] FROM [dbo].[PERMISSION]
				WHERE PermissionId = @iUpdatePermIdOld
				SELECT @iUpdatePermId = IDENT_CURRENT('PERMISSION')
			END
		
		/** Si iFile est supérieure à 0, on modifie le display order avec le ParentTab et ParentFileId,
		  *	utile pour les signets, sinon c'est utile pour les onglets qui n'ont pas de parent. */
		IF(@IFile > 0)
			BEGIN
				SELECT @iDisplayOrder = ISNULL(MAX(DisplayOrder),0) + 1 FROM [dbo].[XrmGrid]
				WHERE ParentTab = @iTab AND ParentFileId = @IFile
			END
		ELSE
			BEGIN
				SELECT @iDisplayOrder = ISNULL(MAX(DisplayOrder),0) + 1 FROM [dbo].[XrmGrid]
				WHERE ParentTab = @iTab
			END

		/** insertion de la nouvelle grille sur la base de l'ancienne. */
		INSERT INTO  [dbo].[XrmGrid] ([ParentTab], [ParentFileId], [Title], [Tooltip], [DisplayOrder]
							  ,[ViewPermId], [UpdatePermId], [XrmGrid84], [XrmGrid88], 
							  [XrmGrid92], [XrmGrid95],[XrmGrid96], [XrmGrid97],
							  [XrmGrid98], [XrmGrid99])
			SELECT @iTab, @IFile, @nAddTitre, [Tooltip], @iDisplayOrder, 
					@iViewPermId, @iUpdatePermId, [XrmGrid84], [XrmGrid88], [XrmGrid92],
					GETDATE(), GETDATE(), [XrmGrid97], [XrmGrid98], [XrmGrid99]
				FROM [dbo].[XrmGrid]
			WHERE XrmGridId = @iGrid;

		/** ID de la grille nouvellement insérée. @@identity a toujours eu l'air douteux. */
		SELECT @iNewGrid = IDENT_CURRENT('XrmGrid');


		/** insertion des nouveaux widgets avec les données des anciens et récupération des ID.
		  * des anciens ET des nouveaux. D'ou un Meeeeeerge. */
		MERGE [dbo].[XrmWidget] AS XrmWidgetDest
			USING (SELECT	xw.[XrmWidgetId], [Title], [SubTitle], [Tooltip], [Type], [PictoIcon],
							[PictoColor], [Move], [Resize], [ManualRefresh], [DisplayOption], [DefaultPosX], 
							[DefaultPosY], [DefaultWidth], [DefaultHeight], [ContentSource], [ContentType], 
							[ContentParam], [ViewPermId], [ShowHeader], [XrmWidget84], [XrmWidget88], 
							[XrmWidget92], [XrmWidget95], [XrmWidget96], [XrmWidget97], [XrmWidget98], 
							[XrmWidget99], [ShowWidgetTitle] 
						FROM [dbo].[XrmWidget] AS xw
							INNER JOIN [dbo].[XrmGridWidget] AS xgw ON xw.XrmWidgetId = xgw.XrmWidgetId
					WHERE xgw.[XrmGridId] = @iGrid) AS XrmWidgetSrc
			ON 1=0			
		WHEN NOT matched THEN
			INSERT ([Title], [SubTitle], [Tooltip],
					[Type], [PictoIcon],
					[PictoColor], [Move], [Resize],
					[ManualRefresh], [DisplayOption], [DefaultPosX], 
					[DefaultPosY], [DefaultWidth], [DefaultHeight],
					[ContentSource], [ContentType], 
					[ContentParam], [ViewPermId], [ShowHeader],
					[XrmWidget84], [XrmWidget88], 
					[XrmWidget92], [XrmWidget95], [XrmWidget96],
					[XrmWidget97], [XrmWidget98], 
					[XrmWidget99], [ShowWidgetTitle])
				VALUES (XrmWidgetSrc.[Title], XrmWidgetSrc.[SubTitle], XrmWidgetSrc.[Tooltip],
						XrmWidgetSrc.[Type], XrmWidgetSrc.[PictoIcon], XrmWidgetSrc.[PictoColor],
						XrmWidgetSrc.[Move], XrmWidgetSrc.[Resize], XrmWidgetSrc.[ManualRefresh],
						XrmWidgetSrc.[DisplayOption], XrmWidgetSrc.[DefaultPosX], 
						XrmWidgetSrc.[DefaultPosY], XrmWidgetSrc.[DefaultWidth], XrmWidgetSrc.[DefaultHeight],
						XrmWidgetSrc.[ContentSource], XrmWidgetSrc.[ContentType], 
						XrmWidgetSrc.[ContentParam], XrmWidgetSrc.[ViewPermId], XrmWidgetSrc.[ShowHeader],
						XrmWidgetSrc.[XrmWidget84], XrmWidgetSrc.[XrmWidget88], 
						XrmWidgetSrc.[XrmWidget92], XrmWidgetSrc.[XrmWidget95], XrmWidgetSrc.[XrmWidget96],
						XrmWidgetSrc.[XrmWidget97], XrmWidgetSrc.[XrmWidget98], 
						XrmWidgetSrc.[XrmWidget99], XrmWidgetSrc.[ShowWidgetTitle])
			OUTPUT XrmWidgetSrc.[XrmWidgetId] AS idOldWidget, inserted.[XrmWidgetId] AS idNewWidget INTO @tabWidget;

				
		/** insertion des widgets de la grille. */
		INSERT INTO [dbo].[XrmGridWidget] ([XrmWidgetId], [XrmGridId])
			SELECT idNewWidget, @iNewGrid
				FROM @tabWidget;

		/** insertion des paramètres des nouveaux widgets à l'aide des anciens */
		INSERT INTO [dbo].[XrmWidgetParam]([XrmWidgetId], [ParamName], [ParamValue])
			SELECT tw.idNewWidget, xwp.[ParamName], xwp.[ParamValue]
				FROM [dbo].[XrmWidgetParam] AS xwp
					INNER JOIN @tabWidget AS tw ON xwp.XrmWidgetId = tw.idOldWidget
					where xwp.[ParamName] <> 'libelle' 

		INSERT INTO [dbo].[XrmWidgetParam]([XrmWidgetId], [ParamName], [ParamValue])
			SELECT tw.idNewWidget, xwp.[ParamName], isnull((SELECT top 1 ResValue from RESCODE where '<#[' + cast( Code  as varchar(10))+ ']#>' = xwp.[ParamValue] ),  xwp.[ParamValue])
				FROM [dbo].[XrmWidgetParam] AS xwp
					INNER JOIN @tabWidget AS tw ON xwp.XrmWidgetId = tw.idOldWidget
					where xwp.[ParamName] = 'libelle' 

		/** insertion des préfèrences des nouveaux widgets à l'aide des anciens */
		INSERT INTO [dbo].[XrmWidgetPref]([XrmGridWidgetId], [UserId], [PosX],
										  [PosY], [Width], [Height],[Visible])
			SELECT	xgwNew.Id, [UserId], [PosX], [PosY],
					[Width], [Height],[Visible]
				FROM [dbo].[XrmWidgetPref] AS xwp
					INNER JOIN [dbo].[XrmGridWidget] AS xgwOld ON xgwOld.Id = xwp.XrmGridWidgetId
					INNER JOIN @tabWidget AS tw ON tw.idOldWidget = xgwOld.XrmWidgetId
					INNER JOIN [dbo].[XrmGridWidget] AS xgwNew ON tw.idNewWidget = xgwNew.Id

		COMMIT TRAN;
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN;

		DECLARE @ErrorMessage NVARCHAR(4000);  
		DECLARE @ErrorSeverity INT;  
		DECLARE @ErrorState INT;  
  
		SELECT  @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
		RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);  

	END CATCH
END
