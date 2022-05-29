// retourne une expression (1 ou 0 par defaut) suivant la valeur booleenne de l'expression bValue
function getBoolExpr( bValue, strTruePart, strFalsePart )
{
	if( strTruePart == null )
		strTruePart = 1;
	if( strFalsePart == null )
		strFalsePart = 0;
	if( bValue )
		return strTruePart;
	else
		return strFalsePart;	
}

function trim( strTrim )	
{
	return strTrim.replace( /(^\s*)|(\s*$)/g, "" );
}

// retire le code HTML et retourne le texte
function removeHtml( strHtml )
{
	var strText = strHtml;
	var re = new RegExp( "&nbsp;", "gmi" );
	strText = strText.replace( re , " " );
	var re = new RegExp( "<br>", "gmi" );
	strText = strText.replace( re , "\n" );
	var re = new RegExp( "<[^>]*>", "gmi" );
	strText = strText.replace( re , "" );
	return strText;
}

// Retourne un tableau contenant les valeurs de la chaine de parametres
function parseQueryString( strQueryString )
{
	var aValue = new Array();
	var aVal = strQueryString.split( "&" );
	for( var i = 0 ; i < aVal.length ; i++ )	
	{
		var strVal = aVal[i];
		var aFieldValue = strVal.split( "=" );

		aValue[i] = new Array(); 
		aValue[i][0] = decode( aFieldValue[0] );
		aValue[i][1] = decode( aFieldValue[1] );
	}
	return aValue;
}
function getArrayValue( aArray, strKey )
{
	for( var i = 0 ; i < aArray.length ; i++ )	
	{
		if( aArray[i][0].toLowerCase() == strKey.toLowerCase() )
			return aArray[i][1];
	}
}

// Desactive le champ et change la couleur de fond si c'est un type text
function setDisabled( strFieldId, bDisabled ) 
{
	var oField = document.getElementById(strFieldId);
			 
	if( !oField )
		return;
			
	oField.disabled = bDisabled;
	try
	{
		if( oField.getAttribute("type") == "text" )
		{
			if( bDisabled )
				oField.style.backgroundColor = "#E6E6E6";
			else
				oField.style.backgroundColor = "#FFFFFF";
		}
	}
	catch(e) {}
}
function setHidden( strFieldId, bHidden ) 
{
	var oField = document.getElementById(strFieldId);
			 
	if( !oField )
		return;
			
	try
	{
		if( !bHidden )
			oField.style.visibility = "visible";
		else
			oField.style.visibility = "hidden";
	}
	catch(e) {}
}
function getDateFromString( strDate )
{
	if( strDate == "" )
		return;


		
	var dDate = new Date(null);
	var aFullDate = strDate.split( " " );
	var aDate = (aFullDate[0]).split( "/" );

 // SPH
 // Inversion de l'odre de création de l'objet date :
 // Année d'abord pour que les années bisextilles soient 
 // correctement gérée
 //ELA -----------------SUPPRESSION-------------------------
 //dDate.setFullYear( parseInt( aDate[2], 10 ) ); 
 //dDate.setMonth( parseInt( aDate[1], 10 ) -1 );
 //dDate.setDate( parseInt( aDate[0], 10 ) ); 
 //ELA -----------------SUPPRESSION-------------------------
 
 var nDay = parseInt( aDate[0], 10 );
 var nMonth = parseInt( aDate[1], 10 ) - 1;
 var nYear = parseInt( aDate[2], 10 );
 
 var dNewDate = new Date(nYear, nMonth, nDay);
 dDate = dNewDate;

	
	if( aFullDate.length > 1 )
	{
		aDate = (aFullDate[1]).split( ":" );
	
		dDate.setHours( parseInt( aDate[0], 10 ) ); 
		dDate.setMinutes( parseInt( aDate[1], 10 ) ); 
		dDate.setSeconds( 0 ); 
	}
	else
	{
		dDate.setHours( 0 ); 
		dDate.setMinutes( 0 ); 
		dDate.setSeconds( 0 ); 
	}
	
	
	
	return( dDate );
}

function getStringFromDate( dDate, bOnlyDate, bOnlyTime )
{
	if( isNaN( dDate ) )
		return;
	
	var strDate = "";
	
	if( !bOnlyTime )
	{
		strDate = makeTwoDigit( dDate.getDate() )		+"/"+ 
				makeTwoDigit( dDate.getMonth() + 1 )	+"/"+ 
				makeTwoDigit( dDate.getFullYear() )
	}
	if( !bOnlyDate )
	{
		if( strDate != "" )
			strDate += " ";
		strDate += makeTwoDigit( dDate.getHours() )		+":"+ 
				makeTwoDigit( dDate.getMinutes() )		+":"+ 
				makeTwoDigit( dDate.getSeconds() ); 
	}
	return( strDate );
}
function getNbDayOfMonth( nMonth, nCurrentYear )
{
	var nDayOfMonth;
	if( nMonth == 2 )
	{
		//Gestion des annees bisextiles
		if( nCurrentYear % 4 == 0 )
		{
			nDayOfMonth = 29;
		}
		else
		{
			nDayOfMonth = 28;
		}
	}
	else
	{
		nDayOfMonth = dateAdd( 'd',-1, getDateFromString( '01/'+ makeTwoDigit( nMonth + 1 ) +'/'+ nCurrentYear ) ).getDate();
	} 
	return nDayOfMonth;
}

/* Ajoute un interval a une date et retourne la date
	d Day 
	h Hour 
	n Minute 
	s Second 
*/
function dateAdd( strInterval, lNumber, dDate ) 
{


	var dReturnDate = new Date(dDate);
	
	
	
	switch( strInterval.toLowerCase() )
	{
	case "h":
		dReturnDate.setHours( dDate.getHours() + lNumber );
		break;
	case "n":
		dReturnDate.setMinutes( dDate.getMinutes() + lNumber );
		break;
	case "s":
		dReturnDate.setSeconds( dDate.getSeconds() + lNumber );
		break;
	default:
		dReturnDate.setDate( dDate.getDate() + lNumber );
	}
	
	return dReturnDate;
}

//Retourne le numero de la semaine
function week(d) 
{
   w = d.getDay()
   return Math.floor((yearday(d)-1-w)/7)+2
}
//Retourne le jour de l'annee (1 à 366)
function yearday(d) {
   var d1 = new Date(d);
   d1.setMonth(0); d1.setDate(1)
   return Math.round((d.getTime()-d1.getTime())/(24*3600000))+1
}
// Compare deux dates et retourne le nombre de jours
function dateDiff( strInterval, dDateBegin, dDateEnd ) 
{
   var d = ( dDateEnd.getTime() - dDateBegin.getTime() ) / 1000
   switch( strInterval.toLowerCase() )  
   {
      case "yyyy":	d /= 12
      case "m":		d *= 12 * 7 / 365.25
      case "ww":	d /= 7
      case "d":		d /= 24
      case "h":		d /= 60
      case "n":		d /= 60
   }
   //return Math.round( d );
   return parseInt( d, 10 );
}
function makeTwoDigit( nValue ) 
{
    var nValue = parseInt( nValue, 10 );
    if( isNaN( nValue ) )
    {
		nValue = 0;
    }
    if( nValue < 10 ) 
        return( "0" + nValue );
    else
        return( nValue );
}

function parseDate( strDate )
{
	var strDate = trim( strDate );
	
	var aFullDate = strDate.split( " " );
	
	var strFullDate = aFullDate[0];
		
	var strFullDate = strFullDate.replace( /\./g, "/" );
	var strFullDate = strFullDate.replace( /\-/g, "/" );


	if( ( strFullDate.length == 4 || strFullDate.length == 8 ||  strFullDate.length == 6 ) && strFullDate.indexOf('/') == -1)
		strFullDate = strFullDate.substring(0,2)+'/'+strFullDate.substring(2,4)+'/'+strFullDate.substring(4,strFullDate.length);

	
	var aDate = strFullDate.split( "/" );
	
	var strDay = '';
	var strMonth = '';
	var strYear = '';
	
	if( aDate.length >= 1 )
		strDay = aDate[0];
	if( aDate.length >= 2 )
		strMonth = aDate[1];
	if( aDate.length >= 3 )
	{
		strYear = aDate[2];
		if( strYear.length > 4 )
			strYear = strYear.substring(0,4);
	}
	
	if( parseInt( strDay,10 ) < 10  && strDay.length == 1 )
		strDay = '0' + parseInt( strDay, 10 );
	
	
	if( parseInt( strMonth,10 ) < 10 && strMonth.length == 1 )
		strMonth = '0' + parseInt( strMonth, 10 );
	
	
	if( isNaN( strDay ) || isNaN( strMonth ) || (strDay == '' || strMonth == '' || parseInt( strDay,10 ) >31 || parseInt( strMonth,10 ) < 1 || parseInt( strMonth,10 ) >12 ) || strYear.length == 3 )
	{
		strDate ='';
	}
	
	
	else if( strYear == '' || !strYear )
	{
		var dCurrentDate = new Date();
 		strDate = strDay + '/' + strMonth + '/'+ dCurrentDate.getFullYear();
	}
	else if( parseInt(strYear,10) <100 )
	{
		if ( strYear.length == 1 )
			strYear = '0' +  strYear;
		
		if( parseInt( strYear, 10 ) > 50 )
			var strCurrentYear = '19';
		else
			var strCurrentYear = '20';
		strDate = strDay + '/' + strMonth + '/'+ strCurrentYear + strYear;
	}
	else if( ( parseInt( strYear, 10 ) < 1753 || parseInt( strYear, 10 ) > 9999 ) || isNaN( strYear ) )
	{
		strDate = '';
	}
	else
	{
		strDate = strDay + '/' + strMonth + '/' + strYear;	
	}
	
	if( aFullDate.length == 2 )
		strDate = strDate + ' ' + aFullDate[1];
	return ( strDate );
}

	
	function JSHTMLEncode(valuetoencode) { 
		var e = document.createElement("div"); 
		e.appendChild(document.createTextNode(valuetoencode)); 	
		return e.innerHTML ; 		
	}

	
// change la valeur d'une rubrique en mode liste (gestion de l'affichage)
function setListModeValue( strId, strValue, oPath )
{
   
 
	var regExpBeginning = /^\s+/;
	var regExpEnd = /\s+$/;  
	
	var is_ie=(((navigator.userAgent.toLowerCase()).indexOf("msie")!=-1)&&((navigator.userAgent.toLowerCase()).indexOf("opera")==-1));
	
	 
	
	if(is_ie) 
	{
		var aField;
		
		if ( oPath )
			aField = oPath.document.getElementsByName( strId );
		else
			aField = document.getElementsByName( strId );
		

		
		for(i=0 ; i < aField.length ; i++)
		{
			if ( aField[i].type != 'checkbox' )
			{
				if(aField[i].innerText)
				{
					aField[i].innerText = strValue;
				}
				else
				{
					aField[i].textContent = strValue;	
				}
			}
			else
			{
				aField[i].checked = strValue;
			}
		}
	}
	else
	{
		
	
		var aArray = strId.split("_")
				
		switch(aArray.length)
		{
			case 3:
				var nDescId = aArray[1];
			break;
			case 2:
				var nDescId = aArray[0];
			break;
			default:
				return;
			break;
		}
		
		if ( oPath )
		{
				
			oColl = oPath.document.getElementsByName("V_" + nDescId);
			oCollA = oPath.document.getElementsByName("V_" + nDescId + "A");
		}
		else
		{ 
		
			oColl = document.getElementsByName("V_" + nDescId);
			oCollA = document.getElementsByName("V_" + nDescId + "A");
			
			
			
			// HLA - V7 - FF - non actualisation du planning après mise à jour - 26/01/2010 - Bug #11272
			if( oColl.length == 0 && oCollA.length == 0 )
				oCollA = document.getElementsByName( strId );
		}
		
		if(oColl.length>0)
		{
		
		
			var sFieldsId;
			
			for(var i=0;i<oColl.length; i++)
			{
				if(oColl[i].id == strId)
				{
					if ( oColl[i].type != 'checkbox' )
					{
						if(oColl[i].nodeName == "input")
						{
							oColl[i].value = strValue.replace(regExpBeginning, "").replace(regExpEnd, "");	
						}
						else
						{
							oColl[i].textContent = strValue.replace(regExpBeginning, "").replace(regExpEnd, "");
						}
					}
					else
					{
						oColl[i].checked = strValue;
					}
				}
			}
		}
		
		if(oCollA.length>0)
		{
			var sFieldsId;
			
			for(var i=0;i<oCollA.length; i++){

				if(oCollA[i].id == strId){
					if ( oCollA[i].type != 'checkbox' )
					{
						if(oCollA[i].nodeName == "input")
						{
							oCollA[i].value = strValue.replace(regExpBeginning, "").replace(regExpEnd, "");
						}
						else
						{
							oCollA[i].textContent = strValue.replace(regExpBeginning, "").replace(regExpEnd, "");
						}
					}
					else
					{
						oCollA[i].checked = strValue;
					}
				}
			}
		}
	}
 
}
 

// extrait l'email d'une chaine de caractere
function getEmail( strMailAddress )
{
	var strMail = strMailAddress;
	var nPos = strMail.indexOf( "[", 1 );
	if( nPos > 0 )
	{
		strMail = trim( strMail.substring(nPos, strMail.length ) );
		strMail = trim ( strMail.substring (1, strMail.length - 1 ) );
	}
	else if( strMail.indexOf( "]", 1 ) > 0 )
	{
		nPos = strMail.indexOf( "]", 1 );
		strMail = trim( strMail.substring( 0, nPos - 1 ) );
	}
	
	return ( trim( strMail ) );
}
// Teste si une addresse email est valide
function isMailAddressValid( strMailAddress )
{
	var strEmail = strMailAddress.toLowerCase();
	strEmail = strEmail.replace("'","_");
	var bValid = true;
	//Test de la validité des adresses emails			
	// SPH & HLA - Modification de l'expression - Bug #8340
	bValid = !(strEmail.search(/^[a-z0-9_-]+(\.[_a-z0-9-]+)*@([a-z0-9-]+\.)+[a-z]{2,4}$/)==-1);
    
                //ELA - Bug#10658 - Correctif pour accepter le .museum / .travel
	if (!bValid)
		{
        bValid = !(strEmail.search(/^[a-z0-9_-]+(\.[_a-z0-9-]+)*@([a-z0-9-]+\.)+museum$/)==-1) || !(strEmail.search(/^[a-z0-9_-]+(\.[_a-z0-9-]+)*@([a-z0-9-]+\.)+travel$/)==-1);
		}
	//Un . aprés le @	
	var nPosPoint = strEmail.indexOf( "@.", 1 );
	
	bValid = (bValid && ( nPosPoint == -1 ));
	
	return bValid;
}
// Transforme le texte en texte capitalise
function CCase( strValue )
{
	var strCCaseValue = "";
	var aValue = strValue.split(" ");
	for( i in aValue )
	{
		var strWord = aValue[i];
		var strFirstChar = strWord.substr( 0, 1 );
		var strEndWord = strWord.substr( 1 );
		if( strCCaseValue != "" )
			strCCaseValue += " ";
		strCCaseValue += strFirstChar.toUpperCase() + strEndWord.toLowerCase();
	}
	return strCCaseValue;	
}
function MoveComboItem( cbo, bUp )
{
	if( cbo.selectedIndex == -1 )
	{
		if( cbo.length > 0 ) 
			cbo.selectedIndex = 0;
		return;
	}
	if( bUp && cbo.selectedIndex <= 0 )
		return;
	if( !bUp && cbo.selectedIndex < 0 && cbo.selectedIndex >= cbo.length - 1 )
		return;
	
	var strText, strValue, strColor, strBackColor;	// Tampons
	var nIndex;	// Nouvel index pour deplacement
	
	if( bUp )
		nIndex = cbo.selectedIndex-1;
	else		
		nIndex = cbo.selectedIndex+1;

	if( !cbo.options[nIndex] )
		return;
		
	// Sauve les tampons
	strText = cbo.options[cbo.selectedIndex].text;
	strValue = cbo.options[cbo.selectedIndex].value;
	strColor = cbo.options[cbo.selectedIndex].style.color;
	strBackColor = cbo.options[cbo.selectedIndex].style.backgroundColor;
	
	// Deplace l'index selectionne
	cbo.options[cbo.selectedIndex].text = cbo.options[nIndex].text;
	cbo.options[cbo.selectedIndex].value = cbo.options[nIndex].value;
	cbo.options[cbo.selectedIndex].style.color = cbo.options[nIndex].style.color;
	cbo.options[cbo.selectedIndex].style.backgroundColor = cbo.options[nIndex].style.backgroundColor;

	// Reaffecte l'index ecrase par le selectionne
	cbo.options[nIndex].text = strText;
	cbo.options[nIndex].value = strValue;
	cbo.options[nIndex].style.color = strColor;
	cbo.options[nIndex].style.backgroundColor = strBackColor;
	
	cbo.options[nIndex].selected = true;
}
function setPositionCursor(oInput, nPosition)
{
	var range = oInput.createTextRange();
	range.collapse(true);
	range.moveStart('character', nPosition);
	range.select();
}
function setPositionCursorAtStart(oInput)
{
	var range = oInput.createTextRange();
	range.collapse(true);
	range.select();
}
function setPositionCursorAtEnd(oInput)
{
	var range = oInput.createTextRange();
	range.collapse(false);
	range.select();
}
function loadPage( strPage )
{
	try
	{
		var oHead = document.getElementsByTagName('head').item(0);
		var oLastCmd  = document.getElementById('lastLoadedCmds');
		if (oLastCmd) oHead.removeChild(oLastCmd);

		var oScript = document.createElement('script');
		oScript.src = strPage;
		oScript.type = 'text/javascript';
		oScript.defer = false;
		oScript.id = 'lastLoadedCmds';
		void(oHead.appendChild(oScript));
	} 
	catch(e) {
	}
}


function loadPageAjax(strPage, bType) 
	{ 
		var strAjaxReturn = '';
        var httpRequestAjax;
		var url = strPage;
		if(window.XMLHttpRequest) // Firefox 
			httpRequestAjax = new XMLHttpRequest(); 
		else if(window.ActiveXObject) // Internet Explorer 
			httpRequestAjax = new ActiveXObject("Microsoft.XMLHTTP"); 
		else
			alert("Votre navigateur ne supporte pas les objets XMLHTTPRequest...");
			
		httpRequestAjax.open("GET", url, bType); 
		httpRequestAjax.onreadystatechange = function() {
		if (httpRequestAjax.readyState == 4) 
		{ 			
			if(httpRequestAjax.status == 200) 
			{ 			
				strAjaxReturn = httpRequestAjax.responseText;
			}
			delete httpRequestAjax;
			httpRequestAjax = null;
		}
		};
		httpRequestAjax.send(null);
		return strAjaxReturn;
	}

	


	
//RMA - 05/01/2011	
var defaultrows='';
//Résuit ou Agrandit la fiche
// nTab : Id de la table en cours
// value : True pour agrandir
// bSave : True si l'on souhaite que la valeurs soit sauvegardée en base

function ReduireAgrandir(nTab,value,bSave)
{
	//SPH 
	// #24861 : variable global pour mémoriser le status en cours
	sCurrentState = value;
	
	var frameset = parent.document.getElementById("FraMain");		
	if(value)
	{
		// KHA le 25 01 2011 bug 14541 si on clique deux fois sur réduire on ne peut plus agrandir à moins de recharger la page
		if (defaultrows == '') 
		{
			defaultrows=frameset.rows;
		}
		frameset.rows = "64,*"; 
	}
	else
	{
		ResizeFrame();
		defaultrows='';
	}	 
	
	if(bSave)
	{
		//Enregistre l'ouverture
		loadPage('Process.asp?Type=2&Tab='+nTab+'&Zoom='+value);
	}
}
//Affiche la fiche de façon maximisée
function ResizeFrame()	
{	
	if(typeof(nParentTab) != "undefined")
	{
		if(top.frames['FraTop'].oSelTab.id != nParentTab )
		{			 
			return;
		}
	}
	
	try	{ 
		if( top.frames['FraHeader'].document.frmData.offsetHeight != 0 )
		{
			var offset = top.frames['FraHeader'].document.frmData.offsetHeight;
			offset += (5*2); //GCH 5 étant la bordure haute et basse entre le body et le form data
			this.parent.document.getElementById("FraMain").rows = offset + ',*';
		}
	}
	catch(e)	{
	}
}
