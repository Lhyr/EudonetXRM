
-- GRID 
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'ParentTab' and t.name ='XrmGrid' and t.type = 'U')
  alter table [XrmGrid] add [ParentTab] [numeric](18, 0) null;
  
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'ParentFileId' and t.name ='XrmGrid' and t.type = 'U')
  alter table [XrmGrid] add [ParentFileId] [numeric](18, 0) null;
 
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'Title' and t.name ='XrmGrid' and t.type = 'U')
  alter table [XrmGrid] add [Title] varchar(1000) null;
 
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'Tooltip' and t.name ='XrmGrid' and t.type = 'U')
  alter table [XrmGrid] add [Tooltip] varchar(1000) null; 
  
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'DisplayOrder' and t.name ='XrmGrid' and t.type = 'U')
  alter table [XrmGrid] add [DisplayOrder] int null; 
		
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'ShowWidgetTitle' and t.name ='XrmGrid' and t.type = 'U')
  alter table [XrmGrid] add [ShowWidgetTitle] bit null; 	
 
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'UpdatePermId' and t.name ='XrmGrid' and t.type = 'U')
   alter table [XrmGrid] add [UpdatePermId] [numeric](18, 0) null;
  
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'ViewPermId' and t.name ='XrmGrid' and t.type = 'U')
  alter table [XrmGrid] add [ViewPermId] [numeric](18, 0) null;
		  
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmGrid84' and t.name ='XrmGrid' and t.type = 'U')
   alter table [XrmGrid] add [XrmGrid84] bit null; 

if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmGrid88' and t.name ='XrmGrid' and t.type = 'U')
   alter table [XrmGrid] add [XrmGrid88] [varchar](1000) null;   

if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmGrid92' and t.name ='XrmGrid' and t.type = 'U')
   alter table [XrmGrid] add [XrmGrid92] [varchar](1000) null;
   
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmGrid95' and t.name ='XrmGrid' and t.type = 'U')
   alter table [XrmGrid] add [XrmGrid95] datetime null;
   
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmGrid96' and t.name ='XrmGrid' and t.type = 'U')
   alter table [XrmGrid] add [XrmGrid96] datetime null;
	
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmGrid97' and t.name ='XrmGrid' and t.type = 'U')
   alter table [XrmGrid] add [XrmGrid97]  [numeric](18, 0) null;
   
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmGrid98' and t.name ='XrmGrid' and t.type = 'U')
   alter table [XrmGrid] add [XrmGrid98]  [numeric](18, 0) null;   
      
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmGrid99' and t.name ='XrmGrid' and t.type = 'U')
   alter table [XrmGrid] add [XrmGrid99]  [numeric](18, 0) null;
   
   
-- WIDGET
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'Title' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [Title] varchar(1000) null;
   
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'SubTitle' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [SubTitle] varchar(1000) null;
      
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'Tooltip' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [Tooltip] varchar(1000) null;  
  
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'Type' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [Type] int null;
  
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'PictoIcon' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [PictoIcon] varchar(50) null;
  
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'PictoColor' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [PictoColor] varchar(50) null;
  
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'Move' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [Move] bit null;	
	
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'Resize' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [Resize] bit null;		
		
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'ManualRefresh' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [ManualRefresh] bit null;		
	   
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'DisplayOption' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [DisplayOption] int null;	
	
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'DefaultPosX' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [DefaultPosX] int null;			
	
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'DefaultPosY' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [DefaultPosY] int null;
		
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'DefaultWidth' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [DefaultWidth] int null;
  
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'DefaultHeight' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [DefaultHeight] int null;
		
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'ContentSource' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [ContentSource] varchar(max) null;	
	    
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'ContentType' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [ContentType] int null;	
	
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'ContentParam' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [ContentParam] varchar(1000) null;
  
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'ViewPermId' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [ViewPermId] [numeric](18, 0) null;
		
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'ShowHeader' and t.name ='XrmWidget' and t.type = 'U')
  alter table [XrmWidget] add [ShowHeader] int null;	
  
  
  if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmWidget84' and t.name ='XrmWidget' and t.type = 'U')
   alter table [XrmWidget] add [XrmWidget84] bit null;     


if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmWidget88' and t.name ='XrmWidget' and t.type = 'U')
   alter table [XrmWidget] add [XrmWidget88] [varchar](1000) null;     
 
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmWidget92' and t.name ='XrmWidget' and t.type = 'U')
   alter table [XrmWidget] add [XrmWidget92] [varchar](1000) null;     
   
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmWidget95' and t.name ='XrmWidget' and t.type = 'U')
   alter table [XrmWidget] add [XrmWidget95] datetime null;
   
   if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmWidget96' and t.name ='XrmWidget' and t.type = 'U')
   alter table [XrmWidget] add [XrmWidget96] datetime null;
	
   if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmWidget97' and t.name ='XrmWidget' and t.type = 'U')
   alter table [XrmWidget] add [XrmWidget97]  [numeric](18, 0) null;
   
  if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmWidget98' and t.name ='XrmWidget' and t.type = 'U')
   alter table [XrmWidget] add [XrmWidget98]  [numeric](18, 0) null;   
      
  if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmWidget99' and t.name ='XrmWidget' and t.type = 'U')
   alter table [XrmWidget] add [XrmWidget99]  [numeric](18, 0) null;   
  
  -- HOMEPAGE
  
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'Title' and t.name ='XrmHomePage' and t.type = 'U')
   alter table [XrmHomePage] add [Title] varchar(1000) null;
  
 if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'Tooltip' and t.name ='XrmHomePage' and t.type = 'U')
   alter table [XrmHomePage] add [Tooltip] varchar(1000) null;
  
 if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'UserAssign' and t.name ='XrmHomePage' and t.type = 'U')
   alter table [XrmHomePage] add [UserAssign] varchar(1000) null;
  
 if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'GroupAssign' and t.name ='XrmHomePage' and t.type = 'U')
   alter table [XrmHomePage] add [GroupAssign] varchar(1000) null;   
   
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmHomePage84' and t.name ='XrmHomePage' and t.type = 'U')
   alter table [XrmHomePage] add [XrmHomePage84] bit null; 
   
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmHomePage92' and t.name ='XrmHomePage' and t.type = 'U')
   alter table [XrmHomePage] add [XrmHomePage92] [varchar](1000) null;
   
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmHomePage95' and t.name ='XrmHomePage' and t.type = 'U')
   alter table [XrmHomePage] add [XrmHomePage95] datetime null;
   
   if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmHomePage96' and t.name ='XrmHomePage' and t.type = 'U')
   alter table [XrmHomePage] add [XrmHomePage96] datetime null;
	
   if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmHomePage97' and t.name ='XrmHomePage' and t.type = 'U')
   alter table [XrmHomePage] add [XrmHomePage97]  [numeric](18, 0) null;
   
  if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmHomePage98' and t.name ='XrmHomePage' and t.type = 'U')
   alter table [XrmHomePage] add [XrmHomePage98]  [numeric](18, 0) null;   
      
  if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id 
   where c.name = 'XrmHomePage99' and t.name ='XrmHomePage' and t.type = 'U')
   alter table [XrmHomePage] add [XrmHomePage99]  [numeric](18, 0) null;
   
   
  -- Widget PREF
  
  if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'PosX' and t.name ='XrmWidgetPref' and t.type = 'U')
  alter table [XrmWidgetPref] add [PosX] int null;			
	
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'PosY' and t.name ='XrmWidgetPref' and t.type = 'U')
  alter table [XrmWidgetPref] add [PosY] int null;
		
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'Width' and t.name ='XrmWidgetPref' and t.type = 'U')
  alter table [XrmWidgetPref] add [Width] int null;
  
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'Height' and t.name ='XrmWidgetPref' and t.type = 'U')
  alter table [XrmWidgetPref] add [Height] int null;
  
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'UserId' and t.name ='XrmWidgetPref' and t.type = 'U')
  alter table [XrmWidgetPref] add [UserId] [numeric](18, 0) null;
  
   

  
       	
  
 



   
   
   
   
   
   
   