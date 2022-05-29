if not exists (select sp.name,* from eudotrait.sys.objects  sm
inner join eudotrait.sys.objects  sp on sm.parent_object_id =sp.object_id
where sm.name collate french_ci_ai ='uc_Serial' collate french_ci_ai and sp.name collate french_ci_ai ='EXTERNALCLIENTS' collate french_ci_ai)
begin
ALTER TABLE eudotrait.dbo.EXTERNALCLIENTS
ADD CONSTRAINT uc_Serial UNIQUE (serial)
end

 