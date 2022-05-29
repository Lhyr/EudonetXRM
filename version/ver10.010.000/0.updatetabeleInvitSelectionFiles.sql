IF   EXISTS (select object_id from sys.objects where name = 'INVITSELECTIONFILES' AND TYPE = 'U')
  
 AND   EXISTS (
select COLUMN_ID from sys.columns where name collate french_ci_ai = 'ADRID' and object_id = (
select object_id from sys.objects where name = 'INVITSELECTIONFILES' AND TYPE = 'U')) 

BEGIN
 
	ALTER TABLE INVITSELECTIONFILES ALTER COLUMN ADRID numeric(18,0) null

END


IF   EXISTS (select object_id from sys.objects where name = 'INVITSELECTIONFILES' AND TYPE = 'U')
  
 AND  NOT EXISTS (
select COLUMN_ID from sys.columns where name collate french_ci_ai = 'TPLID' and object_id = (
select object_id from sys.objects where name = 'INVITSELECTIONFILES' AND TYPE = 'U')) 

BEGIN
 
	ALTER TABLE INVITSELECTIONFILES ADD  TPLID numeric(18,0) null

END
