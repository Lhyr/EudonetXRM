USE EUDOTRAIT

DECLARE @dfConstraint as varchar(50)

select @dfConstraint = df.name from syscolumns col inner join sysobjects tab on col.id = tab.id	inner join sysobjects df on col.cdefault = df.id where tab.name = 'SERVERTREATMENTS' and col.name = 'PercentProgress'

IF isnull(@dfConstraint, '') <> ''
BEGIN
	exec ('ALTER TABLE [SERVERTREATMENTS] DROP [' + @dfConstraint + ']')
	
	ALTER TABLE [SERVERTREATMENTS] ADD DEFAULT('-1') FOR [PercentProgress]
END
