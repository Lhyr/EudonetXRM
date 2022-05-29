UPDATE [USER]
SET [UserMailOther] = UserMail + isnull(';' + UserMailOther, '')
WHERE isnull(CHARINDEX(UserMail, UserMailOther), 0) = 0
	AND isnull(UserMail, '') <> ''
