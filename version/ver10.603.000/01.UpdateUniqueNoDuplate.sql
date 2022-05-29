UPDATE [DESC] 
SET [DESC].NoDefaultClone = 1
WHERE [DESC].NoDefaultClone = 0 and exists (SELECT * FROM [DESCADV] WHERE [DESC].DescId = [DESCADV].DescId AND [DESCADV].Parameter = 43 AND [DESCADV].Value = 1)
 