/*    HLA/GCH
      Création du champ de liaison vers la campagne et des champs de fusions
      04/11/2013
*/
set nocount on;
declare @nTab int,
      @tabName as varchar(100)
      
declare curs cursor for
      SELECT [DescId], [FILE] FROM [DESC] WHERE [TYPE] = 3
open curs
fetch next from curs
      into @nTab, @tabName
while @@FETCH_STATUS = 0
begin
	  --CampaingID
      if not exists (SELECT 1
                  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
                  where sys.tables.name like @tabName and syscolumns.name like 'TPL17')
      BEGIN
            
            exec('ALTER TABLE ['+@tabName+'] ADD [TPL17] [numeric] NULL');

            DELETE FROM [FileDataParam] WHERE [DescId] = @nTab+17
            DELETE FROM [MRU] WHERE Userid > 0 and DescId = @nTab+17
            DELETE FROM [UserValue] WHERE ISNULL( [Type], 0 ) in( 6,18) AND [Tab] = @nTab AND [DescId] = @nTab+17
            
            exec('ALTER TABLE [dbo].['+@tabName+'] ADD CONSTRAINT [FK_'+@tabName+'_CAMPAIGN FOREIGN KEY ( [Tpl17] ) REFERENCES [dbo].[CAMPAIGN] ( [CampaignId] ) ');
            exec('CREATE  INDEX [IX_'+@tabName+'_4] ON [dbo].['+@tabName+']([TPL17]) WITH  FILLFACTOR = 90 ON [PRIMARY]');
      END      
      if((select COUNT(*) from [DESC] where DescId = @nTab+17 ) <= 0)
      begin
            INSERT INTO [Desc] ([DescId],[File],[Field],[DispOrder]) VALUES (@nTab+17,@tabName,'TPL17',17);
            UPDATE [Desc] SET [computedfieldenabled]=N'0',[fulluserlist]=N'0',[changerulesid]=NULL,[bounddescid]=NULL,[readonly]=N'0',[format]=1,
                  [mask]=N'*.doc;*.xls',[default]='',[defaultformat]=0,[labelalign]=NULL,[multiple]=0,[viewrulesid]=NULL,[tooltiptext]=NULL,[italic]=N'0',
                  [relation]=N'1',[viewpermid]=NULL,[nodefaultclone]=N'0',[obligatrulesid]=NULL,[unicode]=N'0',[rowspan]=1,[case]=0,[formula]=NULL,[bold]=N'0',
                  [underline]=N'0',[popup]=2,[html]=N'0',[sizelimit]=NULL,[scrolling]=N'0',[forecolor]=NULL,[popupdescid]=106001,[obligat]=N'0',[updatepermid]=NULL,
                  [prospectenabled]=NULL,[flat]=N'0',[storage]=N'0',[colspan]=N'1',[treeviewuserlist]=N'0',[length]=0,[parameters]=NULL 
            WHERE DescId = @nTab+17
      end
      if((select COUNT(*) from [Res] where ResId = @nTab+17 ) <= 0)
      begin
            INSERT INTO [Res] (ResId,LANG_00) VALUES (@nTab+17,N'Campagne');
      end     
      if((select COUNT(*) from [FileDataParam] where DescId = @nTab+17 ) <= 0)
      begin
            DELETE FROM [FileDataParam] WHERE [DescId] = @nTab+17
            DELETE FROM [MRU] WHERE Userid > 0 and DescId = @nTab+17
            DELETE FROM [UserValue] WHERE ISNULL( [Type], 0 ) in( 6,18) AND [Tab] = @nTab AND [DescId] = @nTab+17
            INSERT INTO [UserValue] ([Type],[Enabled],[Tab],[DescId],[Value]) VALUES(6,1,@nTab,@nTab+17,'')
            INSERT INTO [FileDataParam] ([DescId],[DataEnabled],[SortEnabled],[DisplayMask],[SortBy],[LangUsed],[AddPermission],[UpdatePermission],
                  [DeletePermission],[SynchroPermission],[Treeview],[DataAutoEnabled],[DataAutoStart],[DataAutoFormula],[NoAutoLoad], [SearchLimit], 
                  [TreeViewOnlyLastChildren]) VALUES (@nTab+17,0,0,'[TEXT]','[TEXT]','0',NULL,NULL,NULL,NULL,0,0,0,NULL,'0',0,'0')
      end	   
      if not exists ( select [DESCId] from [DESC] where [DESCId] = @nTab +17 and isnull([Relation],0) = 1)
      begin
            update [DESC] set [Relation] = 1 where [DESCId] = @nTab +17
      end
	  
      --Date de première lecture
      if not exists (SELECT 1
                  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
                  where sys.tables.name like @tabName and syscolumns.name like 'TPL18')
      BEGIN
            exec('ALTER TABLE ['+@tabName+'] ADD [TPL18] [DateTime] NULL');

            insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder], [readonly]) select @nTab+18, @tabName, 'TPL18', 2, 0, 18, 1
            insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab+18, 'Date de première lecture', 'Date of First Reading', 'Date of First Reading', 'Date of First Reading', 'Date of First Reading'
      END
      if((select COUNT(*) from [DESC] where DescId = @nTab+18 ) <= 0)
      begin
            insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder], [readonly]) select @nTab+18, @tabName, 'TPL18', 2, 0, 18, 1
      end
      if((select COUNT(*) from [Res] where ResId = @nTab+18 ) <= 0)
      begin
            insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab+18, 'Date de première lecture', 'Date of First Reading', 'Date of First Reading', 'Date of First Reading', 'Date of First Reading'
      end      
      
      --Navigateur
      if not exists (SELECT 1
                  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
                  where sys.tables.name like @tabName and syscolumns.name like 'TPL19')
      BEGIN
            exec('ALTER TABLE ['+@tabName+'] ADD [TPL19] [varchar](500) NULL');
      END
      
      if((select COUNT(*) from [DESC] where DescId = @nTab+19 ) <= 0)
      begin
            insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder], [readonly]) select @nTab+19, @tabName, 'TPL19', 1, 500, 19, 1
      end
      if((select COUNT(*) from [Res] where ResId = @nTab+19 ) <= 0)
      begin
            insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab+19, 'Navigateur', 'Browser', 'Browser', 'Browser', 'Browser'
      end
      
      --Liste des liens cliqués
      if not exists (SELECT 1
                  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
                  where sys.tables.name like @tabName and syscolumns.name like 'TPL22')
      BEGIN
            exec('ALTER TABLE ['+@tabName+'] ADD [TPL22] [varchar](1500) NULL');
      END      
      if((select COUNT(*) from [DESC] where DescId = @nTab+22 ) <= 0)
      begin
            insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder], [readonly]) select @nTab+22, @tabName, 'TPL22', 1, 1500, 22, 1
      end
      if((select COUNT(*) from [Res] where ResId = @nTab+22 ) <= 0)
      begin
            insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab+22, 'Liste des liens cliqués', 'List of links clicked', 'List of links clicked', 'List of links clicked', 'List of links clicked'
      end
      
      --Navigateur de première ouverture
      if not exists (SELECT 1
                  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
                  where sys.tables.name like @tabName and syscolumns.name like 'TPL23')
      BEGIN
            exec('ALTER TABLE ['+@tabName+'] ADD [TPL23] [varchar](500) NULL');
      END      
      if((select COUNT(*) from [DESC] where DescId = @nTab+23 ) <= 0)
      begin
            insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder], [readonly]) select @nTab+23, @tabName, 'TPL23', 1, 500, 23, 1
      end
      if((select COUNT(*) from [Res] where ResId = @nTab+23 ) <= 0)
      begin
            insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab+23, 'Navigateur détaillé', 'Detail Browser', 'Detail Browser', 'Detail Browser', 'Detail Browser'   
      end
      
      if not exists (SELECT 1
                  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
                  where sys.tables.name like @tabName and syscolumns.name like 'TPL24')
      BEGIN
            --UserAgent de première ouverture
            exec('ALTER TABLE ['+@tabName+'] ADD [TPL24] [varchar](500) NULL');
      END      
      if((select COUNT(*) from [DESC] where DescId = @nTab+24 ) <= 0)
      begin
            insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder], [readonly]) select @nTab+24, @tabName, 'TPL24', 1, 500, 24, 1      
      end
      if((select COUNT(*) from [Res] where ResId = @nTab+24 ) <= 0)
      begin
            insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab+24, 'UserAgent', 'UserAgent', 'UserAgent', 'UserAgent', 'UserAgent'  
      end
      
      --MergeFields
      if not exists (SELECT 1
                  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
                  where sys.tables.name like @tabName and syscolumns.name like 'MergeFields')
      BEGIN
            exec('ALTER TABLE ['+@tabName+'] ADD [MergeFields] [varbinary](MAX) NULL');
      END
      if (        Not EXISTS ( select * from [desc] where (descid = @nTab+25 and [Field] = 'MergeFields' and [disporder] = 25) )            )
      BEGIN
            delete [res] where resid = @nTab+25
            delete [desc] where descid = @nTab+25
      END      
      if((select COUNT(*) from [DESC] where DescId = @nTab+25 ) <= 0)
      begin
            insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder], [readonly]) select @nTab+25, @tabName, 'MergeFields', 21, -1, 25, 1  
      end
      if((select COUNT(*) from [Res] where ResId = @nTab+25 ) <= 0)
      begin
            insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab+25, 'Champs de fusion', 'Merge Fields', 'Merge Fields', 'Merge Fields', 'Merge Fields'    
      end      
      
      --Reason    --GCH : Car pas dans les nouveau template mail et fait planté XRM, depuis version 7.204.000 : JBE - AJOUTE LA RUBRIQUE TPL14 : Motif de non envoi du mail
      if not exists (SELECT 1 from [res] where resid =(@nTab+14))
      BEGIN
            INSERT INTO [Res] (ResId,Lang_00,lang_01) VALUES ((@nTab + 14) ,'Motif','Reason')
      END         
      
      fetch next from curs into @nTab, @tabName
end

close curs
deallocate curs
