DECLARE @popupDescId INT

SELECT TOP 1 @popupDescId = CAST([Value] AS INT)
FROM [DESCADV]
WHERE [DescId] = 200
AND [Parameter] = 37
AND ISNUMERIC([Value]) = 1

UPDATE [DESC] SET 
[ReadOnly] = 0
,[Popup] = 3
,[PopupDescId] = ISNULL(@popupDescId, 117027)
WHERE [DescId] = 117027


UPDATE [DESC] SET 
[ReadOnly] = 0
WHERE [DescId] IN (117001, 117028, 117029, 117030, 117014)


UPDATE [DESC] SET 
[ReadOnly] = 0
,[Popup] = 6
WHERE [DescId] IN (117025, 117026)
