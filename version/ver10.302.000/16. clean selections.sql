UPDATE SELECTIONS SET TabOrder = dbo.efc_RemoveValueFromMultiple(TabOrder, 114000, ';') WHERE TabOrder IS NOT NULL
UPDATE SELECTIONS SET TabOrder = dbo.efc_RemoveValueFromMultiple(TabOrder, 114200, ';') WHERE TabOrder IS NOT NULL
UPDATE SELECTIONS SET TabOrder = dbo.efc_RemoveValueFromMultiple(TabOrder, 115000, ';') WHERE TabOrder IS NOT NULL
UPDATE SELECTIONS SET TabOrder = dbo.efc_RemoveValueFromMultiple(TabOrder, 115100, ';') WHERE TabOrder IS NOT NULL

UPDATE PREF SET BkmOrder = dbo.efc_RemoveValueFromMultiple(BkmOrder, 114000, ';') WHERE BkmOrder IS NOT NULL
UPDATE PREF SET BkmOrder = dbo.efc_RemoveValueFromMultiple(BkmOrder, 114200, ';') WHERE BkmOrder IS NOT NULL
UPDATE PREF SET BkmOrder = dbo.efc_RemoveValueFromMultiple(BkmOrder, 115000, ';') WHERE BkmOrder IS NOT NULL
UPDATE PREF SET BkmOrder = dbo.efc_RemoveValueFromMultiple(BkmOrder, 115100, ';') WHERE BkmOrder IS NOT NULL

