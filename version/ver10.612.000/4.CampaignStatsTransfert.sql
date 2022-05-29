IF (SELECT COUNT(1) FROM CAMPAIGNSTATSADV)= 0
BEGIN

	INSERT INTO CAMPAIGNSTATSADV (
			evtid, 

			CAMPAIGNSTATSADV01,
			CAMPAIGNSTATSADV02,
			CAMPAIGNSTATSADV03,
			CAMPAIGNSTATSADV04,
			CAMPAIGNSTATSADV05,
			CAMPAIGNSTATSADV06,
			CAMPAIGNSTATSADV07,
			CAMPAIGNSTATSADV08,
			CAMPAIGNSTATSADV09,
			CAMPAIGNSTATSADV10,
			CAMPAIGNSTATSADV11,
			CAMPAIGNSTATSADV12,
			CAMPAIGNSTATSADV13,
			CAMPAIGNSTATSADV14,

			CAMPAIGNSTATSADV95,

			CAMPAIGNSTATSADV97,
			CAMPAIGNSTATSADV99
			)


 	 SELECT
             	 EvtId,             
             
             	 IsNuLL([3],0) as 'NB_UNSUB' ,
             	 IsNuLL([4],0) as 'NB_VIEW' ,
             	 IsNuLL([5],0) as 'NB_TOTAL_DEST' ,
             	 IsNuLL([6],0) as 'NB_SINGLE_CLICK' ,
             	 IsNuLL([7],0) as 'NB_CLICK_VIEW' ,
             	 IsNuLL([8],0) as 'NB_CLICK' ,
             	 IsNuLL([9],0) as 'NB_SENT' ,
                         	  IsNull([2_BLOCKED],0) as [2_BLOCKED], 
             	 IsNull([2_INVALID_ADDRESS],0) as [2_INVALID_ADDRESS], 
             	 IsNull([2_REJECTED],0) as [2_REJECTED], 
             	 IsNull([2_TEMP_COMMUNICATION_FAILURE],0) as [2_TEMP_COMMUNICATION_FAILURE], 
             	 IsNull([2_TRANSIENT],0) as [2_TRANSIENT], 
             	 IsNull([2_UNKNOWN],0) as [2_UNKNOWN], 
             	 IsNull([2_COMPLAINT],0) as [2_COMPLAINT],
			
				 GETDATE(),
				(select top 1 CAMPAIGN97 from CAMPAIGN where CAMPAIGNid = EvtId),
				(select top 1 CAMPAIGN99 from CAMPAIGN where CAMPAIGNid = EvtId)
         
             	  FROM (  
     
             	  SELECT  cast(Category as varchar(2)) +   isnull('_'+ SubCategory,'')  cc, Number, cc.EvtId  FROM CAMPAIGNSTATS    cc
             	  where Category > 1          
               	   )  a
             	  PIVOT
             
             	  (max( number) for cc in (
             	  [2_BLOCKED],
                 	  [2_COMPLAINT],
               		[2_REJECTED],
               		[2_TEMP_COMMUNICATION_FAILURE],
               		[2_INVALID_ADDRESS],
               		[2_UNKNOWN],
               		[2_TRANSIENT],
               		[3],[4],[5],[6],[7],[8],[9])) as m
               		--where EvtId=5
             	  ORDER BY EvtId
END