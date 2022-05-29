 
/*
*  Met à jour la liste des déclencheur d'automatisme
*     SPH : 23/04/2018
	
	[xsp_MAJFORMULATRIGGER] 0 -- RECALCUL TOUS
	[xsp_MAJFORMULATRIGGER] 311 -- RECALCUL LES DECLENCHEURS DE LA FORMULE DU DESCID DEMANDE
	[xsp_MAJFORMULATRIGGER] 100000 -- RECALCUL LES DECLENCHEURS DE LA FORMULE DE LA TABLE DEMANDEE

	SELECT * FROM [FormulaTrigger] where formulaDescId like '10__'

*/
CREATE PROCEDURE [dbo].[XSP_MAJFORMULATRIGGER] 
	@Descid int
	 
AS

	SET NOCOUNT ON

	DELETE FROM [FormulaTrigger]   WHERE ( 
	
	case 
		when 	formulaDescId =  @DESCID then 1
		when  @Descid = 0  then 1
		when	@descid % 100 = 0 and formulaDescId - formulaDescId %100 = @descid then 1
		else 0
	end
	) = 1
 

	INSERT INTO [FormulaTrigger] (triggerDescId, formulaDescId, Formula)

	SELECT distinct eudotrigger.descid triggerDescId
		,formula.DescId formuleDescId
		,formula.Formula
 
	FROM [desc] eudotrigger
	INNER JOIN [desc] formula ON charindex('[' + eudotrigger.[file] collate french_ci_ai + '].[' + eudotrigger.[field] collate french_ci_ai + ']', formula.formula collate french_ci_ai) > 0
	WHERE eudotrigger.[file] <> '' and ( 
	
	case 
		when 	formula.DescId =  @DESCID  then 1
		when  @Descid = 0  then 1
		when	@descid % 100 = 0 and formula.DescId - formula.DescId %100 = @descid then 1
		else 0
	end
	) = 1


	SET NOCOUNT OFF