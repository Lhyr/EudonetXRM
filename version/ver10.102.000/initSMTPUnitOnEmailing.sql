
update CONFIG set SMTPEmailingServerName=SMTPServerName,SmtpUseDefaultParam=0 from CONFIG where UserId = 0 and isnull(SmtpUseDefaultParam,0) = 1
