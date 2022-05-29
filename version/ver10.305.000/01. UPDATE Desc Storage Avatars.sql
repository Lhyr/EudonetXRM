DECLARE @TypStoreInFile int = 0
DECLARE @FormatImage int = 13
DECLARE @fieldAvatar int = 75

UPDATE [DESC] SET
[Storage] = @TypStoreInFile
WHERE [DescId] % 100 = @fieldAvatar
AND [FORMAT] = @FormatImage
