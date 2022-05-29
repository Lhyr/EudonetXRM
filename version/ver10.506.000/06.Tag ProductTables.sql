INSERT INTO DESCADV(DescId, Parameter, Value, Category)
SELECT DescId, 33, XrmProduct.XrmProductId, 2
FROM [DESC]
INNER join XrmProduct on [desc].descid between RangeStart and RangeEnd
WHERE [DESC].DescId % 100 = 0
AND NOT EXISTS (SELECT Id FROM DESCADV WHERE [Desc].DescId = DESCADV.DescId and Parameter = 33)