

-- IDX CampaignSelection sur CampaignId et lnkid
if not exists (select 1 from SYS.indexes where name='IDX_CampaignSelection_CampaignId_LnkId' and object_name(object_id) = 'CampaignSelection')
	and not exists( select 1 from [dbo].[efc_GetIndexLike]('CampaignSelection','CampaignId;lnkid',''))
begin 
	create NONCLUSTERED index [IDX_CampaignSelection_CampaignidLnkid] on [dbo].[CampaignSelection] ([CampaignId],[LnkId])
end
 
-- IDX CampaignSelection sur CampaignId avec colonne incluse lnkid
if not exists (select 1 from SYS.indexes where name='IDX_CampaignSelection_CampaignId_Include_LnkId' and object_name(object_id) = 'CampaignSelection')
	and not exists( select 1 from [dbo].[efc_GetIndexLike]('CampaignSelection','CampaignId','lnkid'))
begin 
	create NONCLUSTERED index [IDX_CampaignSelection_CampaignId_Include_LnkId] on [dbo].[CampaignSelection] ([CampaignId]) include ([LnkId])
end