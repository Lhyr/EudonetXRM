
//
// CONSTANTES
//
var INFOS_ITEM_ID_TAB = 0;
var INFOS_ITEM_ID_FILEID = 1;
var INFOS_ITEM_ID_INDEX = 2;
var INFOS_ITEM_ID_OWNER = 3;

var INFOS_ITEM_DATE_BEGIN = 0;
var INFOS_ITEM_DATE_END = 1;
var INFOS_ITEM_TYPE = 2;
var INFOS_ITEM_MULTIDAY = 3;
var INFOS_ITEM_VIEWABLE = 4;
var INFOS_ITEM_SCHEDULE = 5;
var INFOS_ITEM_READONLY = 6;
var INFOS_ITEM_MAP_BEGIN = 7;
var INFOS_ITEM_MAP_END = 8;


var g_oCellEnd = null;				// Cell max for mouse position

var g_aItemId = new Array();
var g_aItemInfos = new Array();
var g_aItemPos = new Array();			// Position de l'item - Valeurs de 1 à MAX_ITEM_OVERLAP
var g_aItemMaxPos = new Array();		// Nombre de positions total utilisé pour chaque rdv
var g_aFileIdItems = new Object;		// Englobe les items pour chaque fileid - Tableau associatif

var is_ie = ( (navigator.userAgent.toLowerCase().indexOf("msie")!=-1) && (navigator.userAgent.toLowerCase().indexOf("opera")==-1) );


function getCellId( strDate, strOwnerId )
{	
	var strRes, re;
	
	strRes = strDate;
	re = new RegExp( "/", "gmi" );
	strRes = strRes.replace( re, "" ); 
	re = new RegExp( ":", "gmi" );
	strRes = strRes.replace( re, "" ); 
	re = new RegExp( " ", "gmi" );
	strRes = strRes.replace( re, "" ); 
	
	if( strOwnerId != '' )
		strRes += "_" + strOwnerId;
	
	return "CELL_" + strRes;
}
/* ***************************** */
/* ***************************** */
/* ***************************** */
function getValueByID( strID )
{
	// HLA - Refonte Planning - Recupere la valeur à partir de l'id - 11/08/2008
	var strValue = "";
	strValue = strID.replace("CELL_","");
	
	if(strValue.indexOf('_') > 0)
		strValue = strValue.substring(0, strValue.indexOf("_"));
	
	if(strValue.length == 14)
		strValue = strValue.substring(0,2) + '/' + strValue.substring(2,4) + '/' + strValue.substring(4,8) + ' ' + strValue.substring(8,10) + ':' + strValue.substring(10,12) + ':' + strValue.substring(12,14);
	else
		strValue = "";
	
	return strValue;
}
/* ***************************** */
/* ***************************** */
/* ***************************** */
function putToArrayItems( sId )
{
	var sFileId;
	
	sFileId = getInfosByItemID( sId, INFOS_ITEM_ID_FILEID );
	
	if( g_aFileIdItems['ITEM_'+sFileId] )
		g_aFileIdItems['ITEM_'+sFileId] += ';';
	else
		g_aFileIdItems['ITEM_'+sFileId] = '';
	
	g_aFileIdItems['ITEM_'+sFileId] += sId;
}
function getInfosByItemID( strID, nInfos )
{
	// HLA - Refonte Planning - Recupere l'info de l'item à partir de l'id - 11/08/2008
	var aInfos, strValue;
	strValue = strID.replace("ITEM_","");
	aInfos = strValue.split( "_" );
	
	strValue = "";
	if(aInfos.length > nInfos)
		strValue = aInfos[nInfos];
	
	return strValue;
}
function getInfosByTabItem( nArrayIndex, nInfos, strDefaultInfos )
{
	// HLA - Refonte Planning - Recupere l'info de l'item à partir de l'id - 11/08/2008
	var aInfos, strValue;
	if( nArrayIndex != -1 )
		strValue = g_aItemInfos[nArrayIndex];
	else
		strValue = strDefaultInfos;
	
	if(!strValue)
		return '';
	
	aInfos = strValue.split( "_" );
	
	strValue = "";
	if(aInfos.length > nInfos)
		strValue = aInfos[nInfos];
	
	return strValue;
}
function setInfosByTabItem( nArrayIndex, nInfos, sValue, strDefaultInfos )
{
	// HLA - Refonte Planning - 11/08/2008
	var aInfos, strInfos, nPosFirst, nPosEnd;
	if( nArrayIndex != -1 )
		strInfos = '_' + g_aItemInfos[nArrayIndex] + '_';
	else
		strInfos = '_' + strDefaultInfos + '_';
	
	nPosFirst = 0;
	for( i=0 ; i <= nInfos - 1 ; i++ )
		nPosFirst = strInfos.indexOf('_', nPosFirst+1);
	
	nPosEnd = strInfos.indexOf('_', nPosFirst+1);
	
	strInfos = strInfos.substring(0, nPosFirst) + '_' + sValue + '_' + strInfos.substring(nPosEnd+1, strInfos.length);
	
	if( strInfos.length > 0 )
	{
		if( nArrayIndex != -1 )
			g_aItemInfos[nArrayIndex] = strInfos.substring(1, strInfos.length-1);
		else
			return strInfos.substring(1, strInfos.length-1);
	}
}
/* ***************************** */
/* ***************************** */
/* ***************************** */
function onBeforeLoadItems( nCount )
{
	var re, sDay, sTime, sCellId;
	
	re = new RegExp( "/", "gmi" );
	sDay = m_dFirstDateOfWeek.replace(re,'');
	
	re = new RegExp( ":", "gmi" );
	sTime = m_dViewHourEnd.replace(re,'');
	
	sCellId = 'CELL_'+sDay+sTime;
	if( m_nViewMode == VIEW_CAL_DAY_PER_USER )
		sCellId += '_'+m_sUserId;
	
	g_oCellEnd = document.getElementById( sCellId );
	
	if( nCount == 0 )
	{
		try	{
			//MAB V7 - 20080710 - empêche l'affichage d'une erreur sous FF3, même sous try/catch
			if (top.frames['FraHeader'].dlg)
				top.frames['FraHeader'].dlg.close();
		}catch(e){}
	}
}
function onAfterLoadItems()
{
	try	{
		//MAB V7 - 20080710 - empêche l'affichage d'une erreur sous FF3, même sous try/catch
		if (top.frames['FraHeader'] && top.frames['FraHeader'].dlg)	
			top.frames['FraHeader'].dlg.close();
	}catch(e){}
}
function PostItem( strCellBeginId, strCellEndId, strItemId, strItemInfos )
{
	var oSheet, oCellBegin, oCellEnd, oItem, nHeight, nWidth;
	var oGripTop, oGripBottom;
	
	oSheet = document.getElementById('SHEET');
	oCellBegin = document.getElementById( strCellBeginId );
	oCellEnd = document.getElementById( strCellEndId );
	
	if( oCellBegin && oCellEnd )
	{
		oItem = document.getElementById(strItemId);
		
		if( oItem )
		{
			oGripTop = document.getElementById(oItem.id +'_GRIP_TOP');
			oGripBottom = document.getElementById(oItem.id +'_GRIP_BOTTOM');
			
			oItem.style.top = oSheet.offsetTop + oCellBegin.offsetTop;
			oItem.style.left = oSheet.offsetLeft + oCellBegin.offsetLeft - 7;
			
			nWidth = oCellBegin.offsetWidth;
			nHeight = oCellEnd.offsetTop - oCellBegin.offsetTop + 1;
		
			if(nHeight < 0)
			{
				var strBeginDate = getValueByID( strCellBeginId );
				var strEndDate = getValueByID( strCellEndId );
				if (oItem.getAttribute('swappeddates') && (oItem.getAttribute('swappeddates') == '1')) {
					strBeginDate = getValueByID( strCellEndId );
					strEndDate = getValueByID( strCellBeginId );
				}
			
				var strTabText = strRes6155 + "<br><br>" + strRes804 + "<br>" + strRes1091 + " : " + strBeginDate + "<br>" + strRes1090 + " : " + strEndDate;
				MsgBox( strTabText, MSG_EXCLAM, strRes6155 );
				
				return;
			}
		
			// Compatibilité FireFox
			if( !is_ie ) nWidth = nWidth - 10;
			if( !is_ie ) nHeight = nHeight - 2;

			oItem.style.height = nHeight;
			oItem.style.width = nWidth;

			// rempli les tableux des informations de chaque item
			g_aItemId.push( strItemId );
			g_aItemInfos.push( strItemInfos );
			g_aItemPos.push( 0 );
			g_aItemMaxPos.push( 0 );
			putToArrayItems( strItemId );
			
			// HLA - Debug couper sur planning - 11/01/2010 - Bug #11172
			if( top.frames["FraParam"] && top.frames["FraParam"].document.getElementById("CuttedItem").value == strItemId)
			{
				// HLA - Rend invisible l'item déjà mis dans le buffer pour un couper
				var nArrayIndex = getArrayIndex( strItemId );
				setInfosByTabItem( nArrayIndex, INFOS_ITEM_VIEWABLE, '0' );
			}
		}
	}
}
/* ***************************** */
/* ***************************** */
/* ***************************** */
function setItemsPosition()
{
	var oSheet;
	var nCurIdx, nI, nIndex, isViewable;
	var nPos, nCntPosBusy, nCurMaxPos, aPosBusy, aItemMatch;
	var nOwnerIndex, sCellBegin, oCellBegin, nLeft, nWidth;
	
	aPosBusy = new Array();
	oSheet = document.getElementById("SHEET");
	
	// Vide les positions et les positions max de chaque item
	for( nCurIdx=0 ; nCurIdx < g_aItemId.length ; nCurIdx++ )
		{ g_aItemPos[nCurIdx] = 0; g_aItemMaxPos[nCurIdx] = 0; }
	
	// Recherche les positions pour chaque item
	for( nCurIdx=0 ; nCurIdx < g_aItemId.length ; nCurIdx++ )
	{	
		if( getInfosByTabItem( nCurIdx, INFOS_ITEM_VIEWABLE, '' ) == '0' )
			continue;
		
		nCntPosBusy = 0;
		nCurMaxPos = 0;
		
		// Tableau des item matcher par l'item en cours
		aItemMatch = new Array();
		
		for( nPos=1 ; nPos <= MAX_ITEM_OVERLAP ; nPos++ )
			aPosBusy[nPos] = 0;
		
		// Recherche les positions déjà utilisé pour l'item en cours
		for( nI=0 ; nI < g_aItemId.length ; nI++ )
		{
			// Sauf l'Item en cours et si l'item a déjà été positionné
			if( nCurIdx != nI && g_aItemPos[nI] > 0 )
			{
				if( isMatch( nCurIdx, nI ) )
				{
					// Tableau des item matcher
					aItemMatch.push(nI);
					// Compte le nb de position occupé
					nCntPosBusy++;
					// Enregistre la position occupé
					aPosBusy[g_aItemPos[nI]] = 1;
					// Recupère la position max
					if( nCurMaxPos < g_aItemPos[nI] )
						nCurMaxPos = g_aItemPos[nI];
				}
			}
			
			if( nCntPosBusy >= MAX_ITEM_OVERLAP )
				break;
		}
		
		// Indique pour l'item en cours la position libre pour lui même (-1 : aucunes disponibles de position)
		if( nCntPosBusy >= MAX_ITEM_OVERLAP )
		{
			g_aItemPos[nCurIdx] = -1;
		}
		else
		{
			for( nPos=1 ; nPos <= MAX_ITEM_OVERLAP ; nPos++ )
			{
				if( aPosBusy[nPos] == 0 )
				{
					g_aItemPos[nCurIdx] = nPos;
					
					// Reprendre le position en cours si c'est la position max
					if( nCurMaxPos < nPos )
						nCurMaxPos = nPos;
					// Ajout l'item en cours
					aItemMatch.push(nCurIdx);
					// Réinitialise les pos max des items matcher et de l'item en cours
					for( nI=0 ; nI < aItemMatch.length ; nI++ )
					{
						nIndex = aItemMatch[nI];
						if(g_aItemMaxPos[nIndex] < nCurMaxPos)
							g_aItemMaxPos[nIndex] = nCurMaxPos;
					}
					break;
				}
			}
		}
	}
	
	// Recherche les positions maximal pour chaque item
	for( nCurIdx=0 ; nCurIdx < g_aItemId.length ; nCurIdx++ )
	{
		nPosMax = 0;
		
		for( nI=0 ; nI < g_aItemId.length ; nI++ )
		{
			// Si l'item en cours ou Sauf l'Item en cours et match avec l'item de parcouru
			if( nCurIdx == nI || (nCurIdx != nI && isMatch( nCurIdx, nI )) )
			{
				if( nPosMax < g_aItemMaxPos[nI] )
					nPosMax = g_aItemMaxPos[nI];
				
				if( nPosMax >= MAX_ITEM_OVERLAP )
					break;
			}
		}
		
		g_aItemMaxPos[nCurIdx] = nPosMax;
	}
	
	// Retaille les items en fonction de leurs position
	for( nI=0 ; nI < g_aItemId.length ; nI++ )
	{
		nOwnerIndex = 0;
		if( m_nViewMode == VIEW_CAL_DAY_PER_USER )
			nOwnerIndex = getInfosByItemID( g_aItemId[nI], INFOS_ITEM_ID_OWNER );
		
		sCellBegin = getCellId( getInfosByTabItem( nI, INFOS_ITEM_MAP_BEGIN, '' ), nOwnerIndex );
		
		oCellBegin = document.getElementById( sCellBegin );
		if( !oCellBegin )
			continue;
		
		nLeft = oSheet.offsetLeft + oCellBegin.offsetLeft - 7;
		nWidth = oCellBegin.offsetWidth;
		
		setItemPosition( nWidth, nLeft, nI );
	}
}
// HLA - Debug couper sur planning - 11/01/2010 - Bug #11172
function setItemPosition( nTotalWidth, nOrigLeft, nCurIdx )
{
	var nLeft, nWidth;
	var oItem = document.getElementById( g_aItemId[nCurIdx] );
	
	if( g_aItemPos[nCurIdx] <= 0 )
		oItem.style.visibility = "hidden";		// HLA - les grip ne sont pas géré
	else
	{
		if( oItem.style.visibility != 'visible' || oItem.style.display != '' )
		{
			oItem.style.display = '';
			oItem.style.visibility = 'visible';
		}
		
		nLeft = nOrigLeft + ( ( nTotalWidth / g_aItemMaxPos[nCurIdx] ) * ( g_aItemPos[nCurIdx] - 1 ) );
		nWidth = ( nTotalWidth / g_aItemMaxPos[nCurIdx] );
		
		// Espacement entre les items
		nWidth = nWidth - 3;
		
		// Compatibilité FireFox
		if( !is_ie ) nWidth = nWidth - 10;
		
		if( oItem.offsefLeft != nLeft )
			oItem.style.left = nLeft;
		if( oItem.offsefWidth != nWidth )
			oItem.style.width = nWidth;
	}
}
function isMatch( nCurIdx, nIdx )
{
	var dBegin = new Date();
	var dEnd = new Date();
	var dCurBegin = new Date();
	var dCurEnd = new Date();
	var nCurOwnerIndex = 0, nOwnerIndex = 0;
	
	if( m_nViewMode == VIEW_CAL_DAY_PER_USER )
	{
		nOwnerIndex = getInfosByItemID( g_aItemId[nIdx], INFOS_ITEM_ID_OWNER );
		nCurOwnerIndex = getInfosByItemID( g_aItemId[nCurIdx], INFOS_ITEM_ID_OWNER );
	    if( nOwnerIndex != nCurOwnerIndex )
		    return false;
    }
	
	dBegin = getDateFromString( getInfosByTabItem( nIdx, INFOS_ITEM_MAP_BEGIN, '' ) );
	dEnd = getDateFromString( getInfosByTabItem( nIdx, INFOS_ITEM_MAP_END, '' ) );
	dCurBegin = getDateFromString( getInfosByTabItem( nCurIdx, INFOS_ITEM_MAP_BEGIN, '' ) );
	dCurEnd = getDateFromString( getInfosByTabItem( nCurIdx, INFOS_ITEM_MAP_END, '' ) );

	return( ( ( dCurBegin >= dBegin && dCurBegin < dEnd ) || ( dCurEnd > dBegin && dCurEnd <= dEnd ) ) || ( dCurBegin < dEnd && dCurEnd > dBegin ) );
}
