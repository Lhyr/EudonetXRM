create procedure dbo.xsp_DuplicateGridFromGrid (
	@iTab as int,
	@iGrid as int,
	@nAddTitre as nvarchar(255),
	@iFile as int = null
) as
begin
	declare @iNewGrid as int;
	declare @tabWidget as Table(idOldWidget int, idNewWidget int)

	begin try
		begin tran

		/** Insertion de la nouvelle grille sur la base de l'ancienne. */
		insert into  [dbo].[XrmGrid] ([ParentTab], [ParentFileId], [Title], [Tooltip], [DisplayOrder]
							  ,[ViewPermId], [UpdatePermId], [XrmGrid84], [XrmGrid88], 
							  [XrmGrid92], [XrmGrid95],[XrmGrid96], [XrmGrid97],
							  [XrmGrid98], [XrmGrid99])
			select @iTab, @iFile, @nAddTitre, [Tooltip], 
					(select isnull(max([DisplayOrder]), 0) + 1
							from [dbo].[XrmGrid] 
						where ParentTab = @iTab 
							and ParentFileId = @iFile), 
					[ViewPermId], [UpdatePermId], [XrmGrid84], [XrmGrid88], [XrmGrid92],
					[XrmGrid95], [XrmGrid96], [XrmGrid97], [XrmGrid98], [XrmGrid99]
				from [dbo].[XrmGrid]
			where XrmGridId = @iGrid;

		/** ID de la grille nouvellement insérée. @@identity a toujours eu l'air douteux. */
		select @iNewGrid = IDENT_CURRENT('XrmGrid');

		/** insertion des nouveaux widgets avec les données des anciens et récupération des ID.
		  * des anciens ET des nouveaux. D'ou un Meeeeeerge. */
		merge [dbo].[XrmWidget] as XrmWidgetDest
			using (select	xw.[XrmWidgetId], [Title], [SubTitle], [Tooltip], [Type], [PictoIcon],
							[PictoColor], [Move], [Resize], [ManualRefresh], [DisplayOption], [DefaultPosX], 
							[DefaultPosY], [DefaultWidth], [DefaultHeight], [ContentSource], [ContentType], 
							[ContentParam], [ViewPermId], [ShowHeader], [XrmWidget84], [XrmWidget88], 
							[XrmWidget92], [XrmWidget95], [XrmWidget96], [XrmWidget97], [XrmWidget98], 
							[XrmWidget99], [ShowWidgetTitle] 
						from [dbo].[XrmWidget] as xw
							inner join [dbo].[XrmGridWidget] as xgw on xw.XrmWidgetId = xgw.XrmWidgetId
					where xgw.[XrmGridId] = @iGrid) as XrmWidgetSrc
			on 1=0			
		when not matched then
			insert ([Title], [SubTitle], [Tooltip],
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
				values (XrmWidgetSrc.[Title], XrmWidgetSrc.[SubTitle], XrmWidgetSrc.[Tooltip],
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
			output XrmWidgetSrc.[XrmWidgetId] as idOldWidget, Inserted.[XrmWidgetId] as idNewWidget into @tabWidget;

				
		/** insertion des widgets de la grille. */
		insert into [dbo].[XrmGridWidget] ([XrmWidgetId], [XrmGridId])
			select idNewWidget, @iNewGrid
				from @tabWidget;

		/** insertion des paramètres des nouveaux widgets à l'aide des anciens */
		insert into [dbo].[XrmWidgetParam]([XrmWidgetId], [ParamName], [ParamValue])
			select tw.idNewWidget, xwp.[ParamName], xwp.[ParamValue]
				from [dbo].[XrmWidgetParam] as xwp
					inner join @tabWidget as tw on xwp.XrmWidgetId = tw.idOldWidget
		
		/** insertion des préférences des nouveaux widgets à l'aide des anciens */
		insert into [dbo].[XrmWidgetPref]([XrmGridWidgetId], [UserId], [PosX],
										  [PosY], [Width], [Height],[Visible])
			select	xgwNew.Id, [UserId], [PosX], [PosY],
					[Width], [Height],[Visible]
				from [dbo].[XrmWidgetPref] as xwp
					inner join [dbo].[XrmGridWidget] as xgwOld on xgwOld.Id = xwp.XrmGridWidgetId
					inner join @tabWidget as tw on tw.idOldWidget = xgwOld.XrmWidgetId
					inner join [dbo].[XrmGridWidget] as xgwNew on tw.idNewWidget = xgwNew.Id

		commit tran;
	end try
	begin catch
		rollback tran;

		declare @ErrorMessage nvarchar(4000);  
		declare @ErrorSeverity int;  
		declare @ErrorState int;  
  
		select  @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
		raiserror (@ErrorMessage, @ErrorSeverity, @ErrorState);  

	end catch
end
