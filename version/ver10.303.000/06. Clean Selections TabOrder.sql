delete from [SELECTIONS]
	where selectid not in (
		select max(selectid) from [SELECTIONS] where tab = 0 group by Userid, Label
		union
		select taborderid from [config] where TabOrderId is not null
	) and tab = 0