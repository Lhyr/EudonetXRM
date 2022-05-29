

if( select c.ConditionalSendEnabled from config c where userid=0) = 0
BEGIN
 
	update [trait] set RulesId=null where  traitid IN (43, 44, 45, 46, 48)
	UPDATE config set ConditionalSendEnabled=1 where UserId=0

end
