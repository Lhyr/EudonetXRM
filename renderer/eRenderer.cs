using Com.Eudonet.Engine.ORM;
using Com.Eudonet.Internal;
using Com.Eudonet.Merge;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Cryptography;
using System.Linq;
using Com.Eudonet.Internal.tools.filler;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Internal.eda;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Classe abstraite pour tous les renderer
    /// </summary>
    public abstract class eAbstractRenderer : IStepTimer
    {

        // Laisser la proprièté en private. Utiliser plutot l'accesseur
        /// <summary>Preference de l'user en cours</summary>
        protected ePref _ePref;

        /// <summary>
        /// Préférence
        /// </summary>
        protected virtual ePref Pref
        {
            get { return _ePref; }
            set { _ePref = value; }
        }


        /// <summary>Div global rendu par le renderer</summary>
        protected Panel _pgContainer = new Panel();


        /// <summary>
        /// Conteneur HTML principale du rendu 
        /// </summary>
        public virtual Panel PgContainer { get { return _pgContainer; } }


        /// <summary>
        /// Lance la génération du rendu
        /// </summary>
        /// <returns></returns>
        public virtual bool Generate() { return true; }

        /// <summary>
        /// Erreur éventuelle
        /// </summary>
        public virtual string Error { get; set; }

        #region Implémentation de IStepTimer

        private eStepTimer _eStep = eStepTimer.GetStepTime();


        /// <summary>
        /// liste des étapes chronométré
        /// </summary>
        /// <returns></returns>
        public List<eTimedStep> LstTimedSteps
        {
            get
            {
                return _eStep.LstTimedSteps;
            }
        }

        /// <summary>
        /// démare un timer d'étape
        /// </summary>
        /// <param name="sStepName">Nom de l'étape</param>
        /// <param name="sMethName">Nom de la méthode</param>     
        /// <param name="sClassPath">Chemin du fichier contenant l'appel</param>
        /// <param name="nLine">Ligne de l'appel</param>
        /// 
        public void StartTimerStep(string sStepName,
            [System.Runtime.CompilerServices.CallerMemberName] string sMethName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sClassPath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int nLine = 0

            )
        {
            _eStep.StartTimerStep(sStepName, sMethName, sClassPath, nLine);
        }

        /// <summary>
        /// stop le timer d'étape
        /// </summary>
        public void StopTimerStep()
        {
            _eStep.StopTimerStep();
        }


        /// <summary>
        /// Ajoute une liste d'étape interne "encadrée"
        /// </summary>
        /// <param name="bAddToLast">Arrête le timer en cours et ajoutes les sous-étapes à la dernière de la liste. Dans ce cas, le sStepName est ignoré </param>
        /// <param name="innerSteps">IStepTimer a "intégrer" dans celui courrant</param>
        /// <param name="sInnerStepName">Nom de l'étape "intern"</param>
        /// <param name="sMethName">Nom de la méthode</param>     
        /// <param name="sClassPath">Chemin du fichier contenant l'appel</param>  
        public void AddTimerInnerRange(IStepTimer innerSteps,
            string sInnerStepName = "",
            bool bAddToLast = false,
            [System.Runtime.CompilerServices.CallerMemberName] string sMethName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sClassPath = "")
        {
            try
            {
                _eStep.AddTimerInnerRange(innerSteps, sInnerStepName, bAddToLast, sMethName, sClassPath);
            }
            finally { }
        }
        #endregion
    }

    /// <summary>
    /// Classe gérant la génération de liste
    /// </summary>
    public class eRenderer : eAbstractRenderer
    {
        /// <summary>Booleen qui indique si l'affichage est un mode édition </summary>
        protected bool _bIsEditRenderer = false;
        /// <summary>Booleen qui indique si l'affichage est un mode fiche </summary>
        protected bool _bIsFileRenderer = false;
        /// <summary>Booleen qui indique si l'affichage est un mode Liste </summary>
        protected bool _bList = false;
        /// <summary>Booleen qui indique si l'affichage est un mode impression </summary>
        protected bool _bIsPrintRenderer = false;


      



        /// <summary>
        /// type du renderer
        /// </summary>
        protected RENDERERTYPE _rType = RENDERERTYPE.UNDEFINED;
    

        /// <summary>
        /// Différent contenu du renderer
        /// </summary>
        public Dictionary<string, Content> DicContent = new Dictionary<string, Content>();

        public enum ContentMode
        {
            /// <summary>Remplace tout le contenu du part</summary>
            REPLACE = 0,
            /// <summary>Enrichi l'objet du dom avec les enfants du part</summary>
            ADD_CHILDS = 1,
        }

        /// <summary>
        /// Représentation d'un rendu : Controle à rendre et éventuel script de callback JS
        /// Permet d'appeler une fonction JS après l'insertion du control dans le DOM
        /// </summary>
        public struct Content
        {
            /// <summary>
            /// Controle HTML 
            /// </summary>
            public Control Ctrl;

            /// <summary>
            /// Javascript de callback excuté après l'insertion dans le DOM du control
            /// </summary>
            public string CallBackScript;

            /// <summary>
            /// Permet d'indiqué si on souhaite remplacer le contenu total ou si on souhait l'enrichir. Par defaut, remplacement du contenu
            /// </summary>
            public int Mode;
        }

        private StringBuilder _sCallBackScript = new StringBuilder();
        /// <summary>
        /// Script de callback
        /// </summary>
        public string GetCallBackScript
        {
            get { return _sCallBackScript.ToString(); }
        }


        /// <summary>
        /// Ajoute un script JS de call back pour le rendu.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="newLine"></param>
        public void AddCallBackScript(string value, bool newLine = true)
        {
            if (string.IsNullOrEmpty(value))
                return;
            if (newLine)
                _sCallBackScript.AppendLine(value);
            else
                _sCallBackScript.Append(value);
        }


        /// <summary>
        /// Vide le script de callback JS
        /// </summary>
        public void ResetCallBackScript()
        {
            _sCallBackScript.Length = 0;

        }


        /// <summary>descid de la table principale sur laquelle porte la requête</summary>
        protected Int32 _tab;

        /// <summary>
        /// Largeur de la fenêtre/des éléments à afficher (variable mise à jour par le JS appelant selon la résolution):
        /// </summary>
        protected Int32 _width;
        /// <summary>
        /// Hauteur de la fenêtre/des éléments à afficher (variable mise à jour par le JS appelant selon la résolution):
        /// </summary>
        protected Int32 _height;

        /// <summary>Message d'erreur</summary>
        protected String _sErrorMsg = String.Empty;

        /// <summary>numéro de l'erreur</summary>
        protected QueryErrorType _nErrorNumber = QueryErrorType.ERROR_NONE;
        /// <summary>Objet exception</summary>
        protected Exception _eException;

        /// <summary>Indique si l'on est en mode impression</summary>
        public Boolean _bPrintmode = false;

        /// <summary>Booleen qui indique si l'affichage est un mode full list ou non</summary>
        protected Boolean _bFullList = true;

        private String _sActiveBkm = String.Empty;

        /// <summary>Liste des descid affiché</summary>
        protected ExtendedList<String> _liFieldsDescId = new ExtendedList<String>();


        /// <summary>Liste des descid autorisé en modif</summary>
        private ExtendedList<String> _liAllowedFieldsDescId = new ExtendedList<String>();


        /// <summary>Liste des descid autorisé en modif</summary>
        protected ExtendedList<String> AllowedFieldsDescId
        {
            get { return _liAllowedFieldsDescId; }
            set { _liAllowedFieldsDescId = value; }
        }




        /// <summary>liste des champs de type memo qui devront être mis en forme par CKEditor</summary>
        protected List<String> _sMemoIds = new List<String>();

        /// <summary>Liste des identifiant html des champs memo</summary>
        public List<String> MemoIds
        {
            get { return _sMemoIds; }
        }


        /// <summary>Conteneu d'informations cachés</summary>
        protected HtmlGenericControl _divHidden = null;

        /// <summary>Liste des plus longues valeurs pour chaque field (Key : Field.Alias)</summary>
        protected ExtendedDictionary<String, ListColMaxValues> _colMaxValues = new ExtendedDictionary<String, ListColMaxValues>();

        /// <summary>
        /// US #4141 - Informations étendues sur les champs pour l'administration (renseignées uniquement si utilisées)
        /// </summary>
        protected IDictionary<int, eAdminTableInfos> _adminTableInfos = new Dictionary<int, eAdminTableInfos>();

        #region constructeur

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        protected eRenderer()
        {
            _rType = RENDERERTYPE.Renderer;
        }

        /// <summary>
        /// Retourne un erender
        /// </summary>
        /// <returns></returns>
        public static eRenderer CreateRenderer()
        {
            eRenderer er = new eRenderer();
            return er;

        }


        /// <summary>
        /// Retourne un erender
        /// </summary>
        /// <returns></returns>
        public static eRenderer CreateRenderer(RENDERERTYPE rt)
        {
            eRenderer er = new eRenderer();
            er._rType = rt;
            er.setRendererType();
            return er;

        }

        #endregion

        #region Accesseurs

        /// <summary>
        /// Type du renderer
        /// </summary>
        public RENDERERTYPE RendererType
        {
            get { return _rType; }

        }

        /// <summary>Liste des descid affiché</summary>
        public ExtendedList<String> FieldsDescId
        {
            get { return _liFieldsDescId; }
            set { _liFieldsDescId = value; }
        }

        /// <summary>
        /// Message d'erreur
        /// Si aucun message d'erreur n'a été spécifié mais qu'un exception interne
        /// a été chargée, retourne le message d'exception interne
        /// </summary>
        public String ErrorMsg
        {
            get
            {
                //Si pas de message d'erreur set mais innerException
                if (_sErrorMsg.Length == 0 && _eException != null)
                    return _eException.ToString();
                return _sErrorMsg;
            }

        }


        /// <summary>
        /// Numéro d'erreur
        /// </summary>
        public QueryErrorType ErrorNumber
        {
            get { return _nErrorNumber; }
        }


        /// <summary>
        /// Permet d'affecter une erreur
        /// </summary>
        /// <param name="nErrorNumner">N° d'erreur</param>
        /// <param name="sErrorMsg">Message </param>
        /// <param name="ex">w</param>
        public void SetError(QueryErrorType nErrorNumner, String sErrorMsg, Exception ex = null)
        {
            _nErrorNumber = nErrorNumner;
            _sErrorMsg = sErrorMsg;
            _eException = ex;
        }


        /// <summary>
        /// Exception
        /// </summary>
        public Exception InnerException
        {
            get { return _eException; }
        }

        /// <summary>
        /// Conteneur principal du renderer
        /// </summary>
        public override Panel PgContainer
        {
            get { return _pgContainer; }
        }

        /// <summary>
        /// Recuperation du container de rendu
        /// </summary>
        /// <returns></returns>
        public virtual Panel GetContents()
        {
            return PgContainer;
        }

        /// <summary>
        /// retourne un panel contenant le message d'erreur
        /// </summary>
        public Panel PnlError
        {
            get
            {
                // TODO : GESTION ERREUR SUR SIGNET
                Panel pnlBookmark = new Panel();

                if (ErrorMsg.Length > 0 || InnerException != null)
                {
                    Literal error = new Literal();
                    //KHA le 19/09/2013 on affiche pas le contenu de l'erreur à l'utilisateur, on affiche un message plus userfriendly
                    //error.Text = String.Concat(ErrorMsg, Environment.NewLine);
                    //if (InnerException != null)
                    //{
                    //    error.Text = String.Concat(error.Text, Environment.NewLine, InnerException.Message, Environment.NewLine, InnerException.StackTrace);
                    //}

                    error.Text = eResApp.GetRes(_ePref, 72);
                    pnlBookmark.Controls.Add(error);

                }
                return pnlBookmark;
            }
        }


        #endregion

        private void setRendererType()
        {
            _bIsEditRenderer = (
                    _rType == RENDERERTYPE.EditFile
                || _rType == RENDERERTYPE.EditFileLite
                || _rType == RENDERERTYPE.AdminFile
                || _rType == RENDERERTYPE.PlanningFile
                || _rType == RENDERERTYPE.EditMail
                || _rType == RENDERERTYPE.EditMailing
                || _rType == RENDERERTYPE.EditSMS
                || _rType == RENDERERTYPE.EditSMSMailing
                || _rType == RENDERERTYPE.FileParentInFoot
                || _rType == RENDERERTYPE.FileParentInHead
                || _rType == RENDERERTYPE.FormularFile);

            _bIsFileRenderer = (
                    _bIsEditRenderer
                || _rType == RENDERERTYPE.MainFile
                || _rType == RENDERERTYPE.PlanningFile
                || _rType == RENDERERTYPE.AdminFile
                || _rType == RENDERERTYPE.MailFile
                || _rType == RENDERERTYPE.SMSFile);

            _bList = (!_bIsEditRenderer && !_bIsFileRenderer);
            _bIsPrintRenderer = (_rType == RENDERERTYPE.PrintFile);
        }

        /// <summary>
        /// appel la séquence de génération basique du renderer
        /// Dans la majorité des renderer, la sequence est : init->build->end 
        /// </summary>
        /// <returns></returns>
        public override Boolean Generate()
        {
            //Flag de type de renderer
            setRendererType();

            try
            {

                if (!Init())
                    return false;


                if (!Build())
                    return false;


                if (!End())
                    return false;


                return true;
            }
            catch (eFileLayout.eFileLayoutException e)
            {
                throw;
            }
            catch (EudoException e)
            {
                throw;
            }
            catch (Exception e)
            {
                //Interception des erreur
                _eException = e;
                _sErrorMsg = e.ToString();
                _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;

                return false;
            }
            finally
            {

            }
        }

        /// <summary>
        /// alimente la cellule de libellé avec les informations nécessaires à eFieldEditor.js
        /// </summary>
        /// <param name="myLabel">Cellule de tableau à peupler</param>
        /// <param name="myRecord">Enregistrement en cours</param>
        /// <param name="myField">Champ à afficher</param>
        public void GetFieldLabelCell(WebControl myLabel, eRecord myRecord, eFieldRecord myField)
        {
            GetFieldLabelCell(myLabel, myRecord, new List<eFieldRecord>() { myField });
        }
        /// <summary>
        /// alimente la cellule de libellé avec les informations nécessaires à eFieldEditor.js
        /// </summary>
        /// <param name="myLabel">Cellule de tableau à peupler</param>
        /// <param name="myRecord">Enregistrement en cours</param>
        /// <param name="listField">Champs à afficher</param>
        /// <param name="forceDrawLabel">Si true, le libellé sera toujours affiché</param>
        public void GetFieldLabelCell(WebControl myLabel, eRecord myRecord, List<eFieldRecord> listField, bool forceDrawLabel = false)
        {
            eFieldRecord myField = listField[0];
            String sLabel = myField.FldInfo.Libelle;
            FieldFormat format = myField.FldInfo.AliasSourceField?.Format ?? myField.FldInfo.Format;
            if (myField.FldInfo.ProspectMatch == 1 && String.IsNullOrEmpty(sLabel))
                sLabel = eResApp.GetRes(_ePref, 1658);

            //dans le cas Du champ de liaison, on affiche le nom de la table
            eFieldRecord fieldRowAliasRelation = null;
            if (myField.FldInfo.Format == FieldFormat.TYP_ALIASRELATION)
            {
                fieldRowAliasRelation = myField;
                myField = myRecord.GetFieldByAlias(fieldRowAliasRelation.FldInfo.RelationSourceField.Alias);
                myField.FldInfo.ToolTipText = fieldRowAliasRelation.FldInfo.ToolTipText;
                myField.FldInfo.Watermark = fieldRowAliasRelation.FldInfo.Watermark;
                myField.FldInfo.PosTabIndex = fieldRowAliasRelation.FldInfo.PosTabIndex;
                myField.FldInfo.PosColSpan = fieldRowAliasRelation.FldInfo.PosColSpan;
                myField.FldInfo.FieldStyle = fieldRowAliasRelation.FldInfo.FieldStyle;
                myField.FldInfo.StyleBold = fieldRowAliasRelation.FldInfo.StyleBold;
                myField.FldInfo.StyleFlat = fieldRowAliasRelation.FldInfo.StyleFlat;
                myField.FldInfo.StyleForeColor = fieldRowAliasRelation.FldInfo.StyleForeColor;
                myField.FldInfo.StyleItalic = fieldRowAliasRelation.FldInfo.StyleItalic;
                myField.FldInfo.StyleUnderline = fieldRowAliasRelation.FldInfo.StyleUnderline;
                myField.FldInfo.ValueColor = fieldRowAliasRelation.FldInfo.ValueColor;
                myField.FldInfo.LabelHidden = fieldRowAliasRelation.FldInfo.LabelHidden;
                myField.FldInfo.Libelle = fieldRowAliasRelation.FldInfo.Libelle;
                sLabel = myField.FldInfo.Libelle;

                myField.RightIsVisible = fieldRowAliasRelation.RightIsVisible && myField.RightIsVisible;

                switch (fieldRowAliasRelation.FldInfo.RelationSource)
                {
                    case 200:
                        myField.IsMandatory = fieldRowAliasRelation.IsMandatory || fieldRowAliasRelation.FldInfo.Table.InterPPNeeded;
                        break;
                    case 300:
                        myField.IsMandatory = fieldRowAliasRelation.IsMandatory || fieldRowAliasRelation.FldInfo.Table.InterPMNeeded;
                        break;
                    case 400:
                        //myField.IsMandatory = fieldRowAliasRelation.IsMandatory && fieldRowAliasRelation.FldInfo.Table.InterPMNeeded;
                        break;
                    default:
                        myField.IsMandatory = fieldRowAliasRelation.IsMandatory || fieldRowAliasRelation.FldInfo.Table.InterEVTNeeded;
                        break;
                }

                myLabel.Attributes.Add("prt", "1");

            }
            else if (_bIsFileRenderer && myField.FldInfo.Table.DescId != myRecord.ViewTab && myField.FldInfo.Descid == myField.FldInfo.Table.MainFieldDescId)
            {
                if (listField.Count <= 1)
                    sLabel = myField.FldInfo.Table.Libelle;
            }
            else if (listField.Count > 1)   //GCH - si plusieurs field à afficher dans la même casegvf
            {
                String sSeparator = GetMultiFieldsSeparator(myField.FldInfo);
                sLabel = "";
                foreach (eFieldRecord currentField in listField)
                {
                    if (sLabel.Length > 0)
                        sLabel = String.Concat(sLabel, sSeparator);
                    sLabel = String.Concat(sLabel, currentField.FldInfo.Libelle);
                }
            }

            Boolean drawSpecLabel = (_bIsFileRenderer && format == FieldFormat.TYP_BIT && (myField.FldInfo.Table.EdnType == EdnType.FILE_MAIL || myField.FldInfo.Table.EdnType == EdnType.FILE_SMS) && myField.FldInfo.Descid == myField.FldInfo.Table.DescId + MailField.DESCID_MAIL_ISHTML.GetHashCode())
                                        || forceDrawLabel;

            // Visibilité du libellé
            Boolean labelHidden = myField.FldInfo.LabelHidden;

            // HLA - Le label est ajouté dans ce cas directement avec la valeur (la case à cocher)
            if (
                (!_bIsFileRenderer
                || (format != FieldFormat.TYP_BIT && format != FieldFormat.TYP_BITBUTTON)
                || drawSpecLabel)

                && (!labelHidden ||
                    (
                          ((myField.FldInfo.Descid - myField.FldInfo.Descid % 100) != _tab)
                       && (fieldRowAliasRelation == null || (fieldRowAliasRelation != null && (fieldRowAliasRelation.FldInfo.Descid - fieldRowAliasRelation.FldInfo.Descid % 100) != _tab))
                    )
                )
                )
            {
                //LiteralControl label = new LiteralControl(HttpUtility.HtmlEncode(sLabel));
                Label myLabelSpan = new Label();
                myLabelSpan.Text = sLabel;
                myLabelSpan.Style.Add(HtmlTextWriterStyle.Height, "auto");
                myLabel.Controls.Add(myLabelSpan);
                // myLabel.Controls.Add(label);

                if (myField.IsMandatory && _bIsFileRenderer)
                {
                    LiteralControl lMandIndic = new LiteralControl("<span class='MndAst'>*</span>");
                    myLabel.Controls.Add(lMandIndic);
                }
            }

            // Couleurs et mise en forme
            if (myLabel != null
                && (
                (myField.FldInfo.Descid - myField.FldInfo.Descid % 100) == _tab)
                || (fieldRowAliasRelation != null && (fieldRowAliasRelation.FldInfo.Descid - fieldRowAliasRelation.FldInfo.Descid % 100) == _tab)
                )
            {
                if (myField.FldInfo.StyleForeColor.Length > 0) { myLabel.Style.Add(HtmlTextWriterStyle.Color, myField.FldInfo.StyleForeColor); }
                if (myField.FldInfo.StyleBold) { myLabel.Style.Add(HtmlTextWriterStyle.FontWeight, "bold"); }
                if (myField.FldInfo.StyleItalic) { myLabel.Style.Add(HtmlTextWriterStyle.FontStyle, "italic"); }
                if (myField.FldInfo.StyleUnderline) { myLabel.Style.Add(HtmlTextWriterStyle.TextDecoration, "underline"); }
                if (myField.FldInfo.StyleFlat) { myLabel.Style.Add(HtmlTextWriterStyle.BorderStyle, "thin"); }
            }


            AddFieldProperties(myLabel, myRecord, myField);

            if (fieldRowAliasRelation?.FldInfo?.Format == FieldFormat.TYP_ALIASRELATION)
            {
                if (_rType == RENDERERTYPE.AdminFile)
                {
                    myLabel.Attributes["did"] = fieldRowAliasRelation.FldInfo.Descid.ToString();
                }
                else
                {
                    myLabel.Attributes["did"] = eLibTools.GetTabFromDescId(myField.FldInfo.Descid).ToString();
                    myLabel.Attributes["popid"] = myLabel.Attributes["did"];
                    myLabel.Attributes.Add("prt", "1");
                    myLabel.Attributes.Add("pop", "2");
                }
            }
        }

        /// <summary>
        /// Retourne le rendu du séparateur séparant plusieurs valeurs dans un même champ
        /// </summary>
        /// <param name="fldInfo">informations sur le champ</param>
        /// <returns></returns>
        private String GetMultiFieldsSeparator(Field fldInfo)
        {
            if (fldInfo.Format == FieldFormat.TYP_PHONE || fldInfo.Descid == PMField.TEL.GetHashCode())
                return " / ";
            else
                return " ";
        }

        /// <summary>
        /// ajoute sur le webControl les propriétés du champ
        /// </summary>
        /// <param name="webCtrl"></param>
        /// <param name="myRecord"></param>
        /// <param name="myField"></param>
        public void AddFieldProperties(WebControl webCtrl, eRecord myRecord, eFieldRecord myField)
        {
            // IMPORTANT : LES INFORMATIONS DANS LES ATTRIBUTS DOIVENT ETRE REPORTE SUR ELISTRENDERER

            String cssClass = String.Concat(" table_labels", myField.IsMandatory ? " mandatory_Label" : "");

            webCtrl.Attributes.Add("class", cssClass);

            webCtrl.ID = eTools.GetFieldValueCellName(myRecord, myField);


            //POur les rubriques de type alias, certaines propriétés du champ source doivent être prises en compte
            FieldFormat format = myField.FldInfo.AliasSourceField?.Format ?? myField.FldInfo.Format;
            PopupDataRender popupDataRend = myField.FldInfo.AliasSourceField?.PopupDataRend ?? myField.FldInfo.PopupDataRend;
            PopupType popup = myField.FldInfo.AliasSourceField?.Popup ?? myField.FldInfo.Popup;
            bool bMultiple = myField.FldInfo.AliasSourceField?.Multiple ?? myField.FldInfo.Multiple;
            int iPopupDescId = myField.FldInfo.AliasSourceField?.PopupDescId ?? myField.FldInfo.PopupDescId;
            int iBoundDescId = myField.FldInfo.AliasSourceField?.BoundDescid ?? myField.FldInfo.BoundDescid;
            Field fBoundField = myField.FldInfo.AliasSourceField?.BoundField ?? myField.FldInfo.BoundField;


            //Ajoute le tooltip
            // #32652
            Boolean bDoTT = _ePref.GetConfig(eLibConst.PREF_CONFIG.TOOLTIPTEXTENABLED) == "1";

            //SPH : désactiver.
            // cela casse certain controle, en effet, le tooltip reprend le innerhtml du controle
            // et l'ajoute dans le dom dans un tt. Le dom contient alors plusieurs conrole avec le même id dans certains cas
            // webCtrl.Attributes.Add("onmouseover", String.Concat("ste(event, '", webCtrl.ID, "');"));
            //webCtrl.Attributes.Add("onmouseout", "ht();");


            //Pas de tooltip pour les champs Bouton
            //#60548 on affiche le tooltip sur le libellé des champs mémo
            if (bDoTT && format != FieldFormat.TYP_BITBUTTON)
            {
                // Demande #37 662 - Les améliorations apportées à StripHtml > SanitizeHtml, via l'appel d'une nouvelle fonction
                // PreProcessHtmlBeforeSanitize, encodent les chevrons orphelins avant traitement, pour que SanitizeHtml fonctionne correctement.
                // Or, l'attribut <title> n'étant pas affiché en HTML à l'affichage, il faut reconvertir les chevrons encodés par cette nouvelle
                // fonction, en chevrons texte

                String tooltip = String.Empty;
                if (myField.FldInfo.Descid == AdrField.PERSO.GetHashCode()) // #55164
                    tooltip = eResApp.GetRes(Pref, 6740);
                else
                    tooltip = myField.FldInfo.Libelle;

                String sTitle = HtmlTools.StripHtml(tooltip.Replace(Environment.NewLine, "")).Replace("&lt;", "<").Replace("&gt;", ">");

                if (!String.IsNullOrEmpty(myField.FldInfo.ToolTipText))
                {
                    String sTT = HtmlTools.StripHtml(myField.FldInfo.ToolTipText.Replace(Environment.NewLine, "")).Replace("&lt;", "<").Replace("&gt;", ">");
                    if (sTT.Length > 0)
                        sTitle = String.Concat(sTitle, Environment.NewLine, sTT);
                }

                // EudoTag
                sTitle = sTitle.Replace("[[BR]]", "\n");

                webCtrl.Attributes.Add("title", sTitle);
            }

            webCtrl.Attributes.Add("did", myField.FldInfo.Descid.ToString());

            // #41247 : eltvalid est nécessaire pour récupérer le fileid d'une fiche pour la suppression des PJ
            //if ((myField.FldInfo.Descid - myField.FldInfo.Table.DescId) == 1)  //Pour le message de suppression on à besoin des info sur le champ 01, mais si besoin pour d'autre champs : test à retirer
            webCtrl.Attributes.Add("eltvalid", eTools.GetFieldValueCellId(myRecord, myField));
            webCtrl.Attributes.Add("lib", myField.FldInfo.Libelle);
            webCtrl.Attributes.Add("mult", bMultiple ? "1" : "0");
            webCtrl.Attributes.Add("tree", popupDataRend == PopupDataRender.TREE ? "1" : "0");

            webCtrl.Attributes.Add("rul", myField.FldInfo.IsInRules ? "1" : "0");

            if (myRecord.ViewTab == myField.FldInfo.Table.DescId)
            {
                // Type de l'autocompletion, 0 pour désactivé
                // #67 882 et #69 580 - Ce bloc a donc été déplacé sous la condition ci-dessus : "if (myRecord.ViewTab == myField.FldInfo.Table.DescId)"                
                // Il n'est censé y avoir qu'un seul champ déclencheur d'adresses prédictives (Bing Maps / DataGouv) par onglet.
                // Il faut donc bien vérifier, ici, qu'on soit dans le cas où on affiche le champ en mode Fiche non signet (et pas en champ de liaison) avant de lui
                // affecter l'attribut autocpl.
                // Car dans le cas d'une base où on aurait Contacts.Nom (par ex.) comme déclencheur, et que ce champ déclencheur apparaîtrait en liaison sur un signet
                // (ex : fiche Adresse affichée en signet de Contacts), ce champ de liaison serait considéré à tort comme déclencheur, et perturberait donc le JavaScript
                // (eAutoCompletion) qui câblerait Bing Maps sur celui-ci plutôt que sur le vrai champ déclencheur, si ce champ déclencheur se situe en-dessous (donc
                // initialisé après). Et, de ce fait, la recherche Bing Maps ne fonctionnerait pas tant que le JavaScript ne détecte pas l'anomalie
                if (myField.FldInfo.AutoCompletionEnabled)
                {
                    webCtrl.Attributes.Add("autocpl", myField.FldInfo.AutoCompletion.GetHashCode().ToString());
                    IDictionary<eLibConst.CONFIGADV, string> dic = eLibTools.GetConfigAdvValues(_ePref, new List<eLibConst.CONFIGADV>() { eLibConst.CONFIGADV.PREDICTIVEADDRESSESREF });
                    webCtrl.Attributes.Add("data-provider", (!String.IsNullOrEmpty(dic[eLibConst.CONFIGADV.PREDICTIVEADDRESSESREF])) ? dic[eLibConst.CONFIGADV.PREDICTIVEADDRESSESREF] : "0");
                }


                //On récupère l'info en session uniquement
                OrmMappingInfo ormInfo = eLibTools.OrmLoadAndGetMapWeb(_ePref);


                bool mf = myField.FldInfo.HasMidFormula || ormInfo.GetAllValidatorDescId.Contains(myField.FldInfo.Descid);
                webCtrl.Attributes.Add("mf", mf ? "1" : "0");

                try
                {
                    webCtrl.Attributes.Add("bf", (!String.IsNullOrEmpty(myField.FldInfo.Formula)) ? "1" : "0");
                }
                catch (Exception)
                {
                }
                try
                {
                    if (_ePref.User.UserLevel >= UserLevel.LEV_USR_ADMIN.GetHashCode() && (_ePref.IsFromEudo || _ePref.IsLocalIp)
                        &&
                        (myField.FldInfo.Table.TabType == TableType.EVENT
                        || myField.FldInfo.Table.TabType == TableType.PM
                        || myField.FldInfo.Table.TabType == TableType.PP
                        || myField.FldInfo.Table.TabType == TableType.TEMPLATE
                        || myField.FldInfo.Table.TabType == TableType.CAMPAIGN
                        || myField.FldInfo.Table.TabType == TableType.HISTO
                        ))
                    {
                        string testHash = HashSHA.GetHashSHA1(String.Concat("EUD0N3T", "tab=", myField.FldInfo.Table.DescId, "&fid=", myField.FileId, "XrM"));
                        webCtrl.Attributes.Add("hsh", testHash);

                    }
                }
                catch
                {
                    // cet attribut est uniquement destiné a du debug interne, il ne doit pas faire planter l'appli
                }
            }

            webCtrl.Attributes.Add("Tab", "tab");
            webCtrl.Attributes.Add("fld", "fld");
            webCtrl.Attributes.Add("pop", ((int)popup).ToString());
            webCtrl.Attributes.Add("popid", iPopupDescId.ToString());
            webCtrl.Attributes.Add("bndId", iBoundDescId.ToString());
            if (myField.FldInfo.BoundField != null)
                webCtrl.Attributes.Add("bndPop", fBoundField.Popup.GetHashCode().ToString());

            // HLA - Informations necessaires pour la gestion du editorCatalog
            if (popup != PopupType.NONE || format == FieldFormat.TYP_USER)
            {
                webCtrl.Attributes.Add("special", popup == PopupType.SPECIAL ? "1" : "0");
                webCtrl.Attributes.Add("treeolc", (myField.FldInfo.AliasSourceField?.IsTreeViewOnlyLastChildren ?? myField.FldInfo.IsTreeViewOnlyLastChildren) ? "1" : "0");
                webCtrl.Attributes.Add("treeusr", (myField.FldInfo.AliasSourceField?.IsTreeViewUserList ?? myField.FldInfo.IsTreeViewUserList) ? "1" : "0");
            }

            // Ajout de l'info sur l'annulation de la saisie
            if (myField.FldInfo.CancelLastValueAllowed)
                webCtrl.Attributes.Add("cclval", "1");

            // Ajout du format du champ si necessaire
            AddFormat(webCtrl, myField);
        }

        /// <summary>
        /// Ajout du format du champ si necessaire
        /// </summary>
        /// <param name="webCtrl">Champ auquel on souhaite ajouter le format</param>
        /// <param name="myField">information sur le champ</param>
        protected virtual void AddFormat(WebControl webCtrl, eFieldRecord myField)
        {
            FieldFormat format = myField.FldInfo.AliasSourceField?.Format ?? myField.FldInfo.Format;
            //GCH - #35859 - Internationnalisation - on permet l'identification des champs au format date pour les convertir au format de la Base de données
            //GCH - #36869 - Internationalisation - Type numérique
            if (format == FieldFormat.TYP_DATE
                || format == FieldFormat.TYP_AUTOINC
                || format == FieldFormat.TYP_COUNT
                || format == FieldFormat.TYP_ID
                || format == FieldFormat.TYP_NUMERIC
                || format == FieldFormat.TYP_MONEY
                || format == FieldFormat.TYP_CHART)
                webCtrl.Attributes.Add("frm", ((int)format).ToString());
        }

        /// <summary>
        /// Permet de passer tout le rendu en mode readonly (impacte chaque fieldrecord)
        /// </summary>
        protected virtual Boolean ReadonlyRenderer
        {
            get
            {
                return false;
            }
        }




        /// <summary>
        /// Création d'une cellule de tableau les valeurs
        /// </summary>
        /// <param name="row">Ligne d'enregistrement</param>
        /// <param name="fieldRow">Rubrique de l'enregistrement</param>
        /// <param name="idx">Index de la colonne</param>
        /// <param name="Pref">Préférence de l'utilisateur</param>
        /// <param name="colMaxValues">Collection des valeurs max de la colonne</param>
        /// <param name="nbCol">nombre de colonnes qu'occupera la cellule</param>
        /// <param name="nbRow">nombre de lignes qu'occupera la cellule</param>
        /// <returns></returns>
        public WebControl GetFieldValueCell(eRecord row, eFieldRecord fieldRow, Int32 idx, ePref Pref, ExtendedDictionary<String, ListColMaxValues> colMaxValues = null, int nbCol = 1, int nbRow = 1)
        {
            return GetFieldValueCell(row, new List<eFieldRecord>() { fieldRow }, idx, Pref, colMaxValues, nbCol, nbRow);
        }

        /// <summary>
        /// Création d'une cellule de tableau les valeurs
        /// </summary>
        /// <param name="row">Ligne d'enregistrement</param>
        /// <param name="lstFieldRecord">Liste de rubrique, rubrique 1 : Rubrique de l'enregistrement et les suivantes juste pour compléter</param>
        /// <param name="idx">Index de la colonne</param>
        /// <param name="Pref">Préférence de l'utilisateur</param>
        /// <param name="colMaxValues">Collection des valeurs max de la colonne</param>
        /// <param name="nbCol">nombre de colonnes qu'occupera la cellule</param>
        /// <param name="nbRow">nombre de lignes qu'occupera la cellule</param>
        /// <returns></returns>
        protected WebControl GetFieldValueCell(eRecord row, List<eFieldRecord> lstFieldRecord, Int32 idx, ePref Pref,
            ExtendedDictionary<String, ListColMaxValues> colMaxValues = null, int nbCol = 1, int nbRow = 1)
        {
            return GetFieldValueCell(row, lstFieldRecord, idx, Pref, Pref.ThemePaths, colMaxValues, nbCol, nbRow);
        }




        /// <summary>
        /// Création d'une cellule de tableau pour la valeur d'un champ
        /// </summary>
        /// <param name="row">Ligne d'enregistrement</param>
        /// <param name="lstFieldRecord">Liste de rubrique, rubrique 1 : Rubrique de l'enregistrement et les suivantes juste pour compléter</param>
        /// <param name="idx">Index de la colonne</param>
        /// <param name="Pref">Préférence de l'utilisateur</param>
        /// <param name="themePaths">Gestion de dossier du theme</param>
        /// <param name="colMaxValues">Collection des valeurs max de la colonne</param>
        /// <param name="nbCol">nombre de colonnes qu'occupera la cellule</param>
        /// <param name="nbRow">nombre de lignes qu'occupera la cellule</param>        
        /// <returns></returns>
        protected virtual WebControl GetFieldValueCell(eRecord row, List<eFieldRecord> lstFieldRecord, Int32 idx, ePrefLite Pref, ePref.CalculateDynamicTheme themePaths,
            ExtendedDictionary<String, ListColMaxValues> colMaxValues = null, int nbCol = 1, int nbRow = 1)
        {
            eFieldRecord fieldRow = lstFieldRecord[0];
            eFieldRecord fieldRowAliasRelation = fieldRow;

            EdnWebControl ednWebCtrl;
            #region relation d'en-tête
            if (fieldRow.FldInfo.Format == FieldFormat.TYP_ALIASRELATION && fieldRow.FldInfo.RelationSourceField != null)
            {
                bool bGlobalInvit = false, bGlobalAffect = false;
                int nParentTab = 0;
                string sParentTabLibelle = "";
                TableCell tcReadonlyGhost = new TableCell();
                tcReadonlyGhost.CssClass = "table_values";
                tcReadonlyGhost.Style.Add(HtmlTextWriterStyle.BackgroundColor, "#F0F0F0");

                try
                {
                    if (_bIsFileRenderer)
                    {
                        eMainFileRenderer rdr = (eMainFileRenderer)this;
                        bGlobalInvit = rdr.GlobalInvit;
                        bGlobalAffect = rdr.GlobalAffect;
                        if (rdr.DicParams != null)
                        {
                            rdr.DicParams.TryGetValueConvert("parenttab", out nParentTab);
                            rdr.DicParams.TryGetValueConvert("parenttablabel", out sParentTabLibelle);
                        }
                        if (bGlobalInvit)
                        {
                            return tcReadonlyGhost;
                        }

                    }
                }
                catch (InvalidCastException)
                {
                    // Dans le cas où on est dans un adminFileRenderer le cast en eMainFileRenderer n'est pas possible
                    // Mais ce cas n'est pas concerné par GlobalInvit donc on ignore l'erreur
                }

                fieldRow = row.GetFieldByAlias(fieldRow.FldInfo.RelationSourceField.Alias);
                if (fieldRow != null)
                {
                    fieldRow.FldInfo.ToolTipText = fieldRowAliasRelation.FldInfo.ToolTipText;
                    fieldRow.FldInfo.Watermark = fieldRowAliasRelation.FldInfo.Watermark;
                    fieldRow.FldInfo.PosTabIndex = fieldRowAliasRelation.FldInfo.PosTabIndex;
                    fieldRow.FldInfo.PosColSpan = fieldRowAliasRelation.FldInfo.PosColSpan;
                    fieldRow.FldInfo.FieldStyle = fieldRowAliasRelation.FldInfo.FieldStyle;
                    fieldRow.FldInfo.StyleBold = fieldRowAliasRelation.FldInfo.StyleBold;
                    fieldRow.FldInfo.StyleFlat = fieldRowAliasRelation.FldInfo.StyleFlat;
                    fieldRow.FldInfo.StyleForeColor = fieldRowAliasRelation.FldInfo.StyleForeColor;
                    fieldRow.FldInfo.StyleItalic = fieldRowAliasRelation.FldInfo.StyleItalic;
                    fieldRow.FldInfo.StyleUnderline = fieldRowAliasRelation.FldInfo.StyleUnderline;
                    fieldRow.FldInfo.ValueColor = fieldRowAliasRelation.FldInfo.ValueColor;
                    fieldRow.FldInfo.LabelHidden = fieldRowAliasRelation.FldInfo.LabelHidden;
                    fieldRow.FldInfo.Libelle = fieldRowAliasRelation.FldInfo.Libelle;

                    fieldRow.IsLink = true;


                    // rétrocompatibilité : dans les liaisons hautes, les règles n'étaient pas appliquées
                    fieldRow.RightIsUpdatable = fieldRowAliasRelation.RightIsUpdatable;
                    fieldRow.RightIsVisible = fieldRowAliasRelation.RightIsVisible;

                    fieldRow.Value = fieldRow.FileId.ToString();

                    switch (fieldRowAliasRelation.FldInfo.RelationSource)
                    {
                        case 200:
                            fieldRow.IsMandatory = fieldRowAliasRelation.IsMandatory || fieldRowAliasRelation.FldInfo.Table.InterPPNeeded;
                            break;
                        case 300:
                            fieldRow.IsMandatory = fieldRowAliasRelation.IsMandatory || fieldRowAliasRelation.FldInfo.Table.InterPMNeeded;
                            break;
                        case 400:
                            //myField.IsMandatory = fieldRowAliasRelation.IsMandatory && fieldRowAliasRelation.FldInfo.Table.InterPMNeeded;
                            break;
                        default:
                            fieldRow.IsMandatory = fieldRowAliasRelation.IsMandatory || fieldRowAliasRelation.FldInfo.Table.InterEVTNeeded;
                            break;
                    }

                    if (bGlobalAffect && nParentTab > 0)
                    {
                        if (fieldRowAliasRelation.FldInfo.RelationSource == (int)TableType.ADR)
                        {
                        }
                        else if (nParentTab == fieldRowAliasRelation.FldInfo.RelationSource)
                        {
                            return tcReadonlyGhost;
                        }
                        else if ((nParentTab == (int)TableType.PP || nParentTab == (int)TableType.PM)
                            && (fieldRowAliasRelation.FldInfo.RelationSource != (int)TableType.PP && fieldRowAliasRelation.FldInfo.RelationSource != (int)TableType.PM))
                        {

                        }
                        else
                        {
                            fieldRow.DisplayValue = String.Concat("<", eResApp.GetRes(Pref, 819), " ", sParentTabLibelle, ">");
                            fieldRow.Value = "-1";
                            fieldRow.FileId = -1;
                            fieldRow.FldInfo.Case = CaseField.CASE_NONE;
                        }
                    }
                }
                else
                {
                    fieldRow = fieldRowAliasRelation;
                }
            }
            #endregion

            try
            {
                if (this.RendererType == RENDERERTYPE.Bookmark
                     && fieldRow.FldInfo.Descid == row.MainField?.Descid
                     && fieldRow.IsLink
                     && Pref.GetPrefType() == ePrefSQL.Type.ePref)
                {
                    List<int> liSelectedTabs = new List<int>();
                    liSelectedTabs.AddRange(((ePref)Pref).GetTabs(ePrefConst.PREF_SELECTION_TAB.TABORDER).SplitToInt(";"));
                    if (!liSelectedTabs.Exists(t => t == fieldRow.FldInfo.Table.DescId))
                        fieldRow.IsLink = false;
                }
            }
            catch (Exception e)
            {
                throw e;
            }


            // Cas particuliers pour l'affichage de certains champs : Email.De, Email.HTML
            bool bIsMailConsultField = _rType == RENDERERTYPE.MailFile || _rType == RENDERERTYPE.SMSFile || (_rType == RENDERERTYPE.PrintFile && fieldRow.FldInfo.Table.EdnType == EdnType.FILE_MAIL || _rType == RENDERERTYPE.PrintFile && fieldRow.FldInfo.Table.EdnType == EdnType.FILE_SMS);
            bool bIsMailEditField = _rType == RENDERERTYPE.EditMail || _rType == RENDERERTYPE.EditMailing || _rType == RENDERERTYPE.EditSMS || _rType == RENDERERTYPE.EditSMSMailing;

            bool bIsMailField = bIsMailConsultField || bIsMailEditField;
            bool bIsMailFromField = bIsMailField
                && fieldRow.FldInfo.Descid == ((_rType == RENDERERTYPE.EditMailing || _rType == RENDERERTYPE.EditSMSMailing) ? CampaignField.SENDER.GetHashCode() : MailField.DESCID_MAIL_FROM.GetHashCode() + _tab);

            bool bIsMailHistoField = (fieldRow.FldInfo.Table.EdnType == EdnType.FILE_MAIL || fieldRow.FldInfo.Table.EdnType == EdnType.FILE_SMS)
                && fieldRow.FldInfo.Descid == _tab + MailField.DESCID_MAIL_HISTO.GetHashCode();
            bIsMailHistoField = bIsMailHistoField || fieldRow.FldInfo.Descid == CampaignField.HISTO.GetHashCode();

            bool bIsHeaderLink = _rType == RENDERERTYPE.FileParentInHead && fieldRow.FldInfo.Descid % 100 == 1;
            bool bisHeaderField = _rType == RENDERERTYPE.FileParentInHead && fieldRow.FldInfo.Descid % 100 != 1;

            // Modes Liste : Cas où la valeur n'est pas affichée directement dans la cellule mais dans un panel intermediaire
            bool bFieldValueInAnInnerPanel = false;

            //champ alias : dans certaines propriétés considérées doivent être celles des champs sources
            FieldFormat format = fieldRow.FldInfo.AliasSourceField?.Format ?? fieldRow.FldInfo.Format;
            int iPopupDescId = fieldRow.FldInfo.AliasSourceField?.PopupDescId ?? fieldRow.FldInfo.PopupDescId;
            bool bMultiple = fieldRow.FldInfo.AliasSourceField?.Multiple ?? fieldRow.FldInfo.Multiple;
            PopupType popup = fieldRow.FldInfo.AliasSourceField?.Popup ?? fieldRow.FldInfo.Popup;
            CaseField caseField = fieldRow.FldInfo.AliasSourceField?.Case ?? fieldRow.FldInfo.Case;

            // champ de type email : un bouton permet d'envoyer un mail directement via l'appli
            if ((_rType == RENDERERTYPE.ListRendererMain || _rType == RENDERERTYPE.Bookmark)
                && format == FieldFormat.TYP_EMAIL)
            {
                bFieldValueInAnInnerPanel = true;
            }

            // #59 789 : idem pour les champs de type Téléphone -> envoi de SMS
            if ((_rType == RENDERERTYPE.ListRendererMain || _rType == RENDERERTYPE.Bookmark)
                && (format == FieldFormat.TYP_PHONE && Pref.SmsEnabled))
            {
                bFieldValueInAnInnerPanel = true;
            }

            if (_rType == RENDERERTYPE.AdminFile && format == FieldFormat.TYP_MEMO)
            {
                bFieldValueInAnInnerPanel = true;
            }

            // liste de filtre et de rapport: la colonne Libellé contient également une div contenant les boutonts d'actions
            if ((_rType == RENDERERTYPE.FilterReportList
                || _rType == RENDERERTYPE.UserMailTemplateList
                || _rType == RENDERERTYPE.PjList
                || _rType == RENDERERTYPE.ListPjFromTpl
                || this._rType == RENDERERTYPE.AutomationList)
                && fieldRow.FldInfo.Descid == fieldRow.FldInfo.Table.MainFieldDescId)
            {
                bFieldValueInAnInnerPanel = true;
            }




            bool bFormular = (_rType == RENDERERTYPE.FormularFile);
            //Mode formulaire : il faut un label car il est inline à l'inverse du Panel.
            bool bFieldValueInAnInnerLabel = false;
            if (bFormular && !fieldRow.RightIsUpdatable)
                bFieldValueInAnInnerLabel = true;


            /* US #4141 - "En nouvel Eudonet X, la rubrique Catalogue ne doit pas apparaitre sous forme d'étape"
            Règles :
            - Sur l'admin d'un onglet uniquement
            - Uniquement sur les catalogues simples non arborescents
            - Cas 1 : si la fiche est en état : activé, alors le catalogue apparait dans sa forme par défaut, quelque soit les options d'affichage définies par l'admin
            - Cas 2 : si la fiche est en état : désactivé, prévisualisation, alors le catalogue apparait sous forme graphique (cas actuel)
            */
            bool bShowStepCatalog = fieldRow.FldInfo.PopupDataRend == PopupDataRender.STEP;
            if (bShowStepCatalog && fieldRow.FldInfo.AliasSourceField != null)
                bShowStepCatalog = fieldRow.FldInfo.AliasSourceField.PopupDataRend == PopupDataRender.STEP;
            if (bShowStepCatalog && RendererType == RENDERERTYPE.AdminFile)
                bShowStepCatalog = GetAdminTableInfos(_ePref, fieldRow.FldInfo.Table.DescId)?.EudonetXIrisBlackStatus != EUDONETX_IRIS_BLACK_STATUS.ENABLED;

            // TODO - à modifier ? dans le cas d'une image on donne le même rendu qu'en mode liste
            if (
                (_bIsEditRenderer || (!bFieldValueInAnInnerLabel && bFormular) || _rType == RENDERERTYPE.AdminFile)
                && !bisHeaderField
                && format != FieldFormat.TYP_BIT
                && format != FieldFormat.TYP_MEMO
                && format != FieldFormat.TYP_IMAGE
                && format != FieldFormat.TYP_IFRAME
                && format != FieldFormat.TYP_CHART
                && format != FieldFormat.TYP_BITBUTTON
                && !bIsMailFromField
                && !bShowStepCatalog
            )
            {
                ednWebCtrl = CreateEditEdnControl(fieldRow);
            }
            else if (_rType == RENDERERTYPE.FormularFile && fieldRow.RightIsUpdatable)
            {
                switch (format)
                {
                    // Une case à cocher "Eudo" (eCheckBoxCtrl) doit être encapsulée dans un contrôle de type Label (span) afin que sa
                    // valeur puisse être lue par getFldEngFromElt (lors de la validation du formulaire par l'utilisateur),
                    // et que ses attributs (ID et autres) soient conservés lorsqu'ils sont ajoutés au conteneur parent (Panel/div)
                    case FieldFormat.TYP_BIT:
                        ednWebCtrl = new EdnWebControl() { WebCtrl = new Label(), TypCtrl = EdnWebControl.WebControlType.LABEL };
                        break;
                    case FieldFormat.TYP_IMAGE:
                        ednWebCtrl = new EdnWebControl() { WebCtrl = new Label(), TypCtrl = EdnWebControl.WebControlType.LABEL };
                        break;
                    case FieldFormat.TYP_MEMO:
                        TextBox tb = new TextBox();
                        tb.TextMode = TextBoxMode.MultiLine;
                        ednWebCtrl = new EdnWebControl() { WebCtrl = tb, TypCtrl = EdnWebControl.WebControlType.TEXTBOX };
                        break;
                    default:

                        ednWebCtrl = new EdnWebControl() { WebCtrl = new TextBox(), TypCtrl = EdnWebControl.WebControlType.TEXTBOX };

                        break;
                }
            }
            #region else if (fieldRow.FldInfo.Format == FieldFormat.TYP_BUTTON) - Commenté à suppr ? KHA 20/07/17
            //else if (fieldRow.FldInfo.Format == FieldFormat.TYP_BUTTON)
            //{
            //    Panel panel = new Panel();
            //    panel.CssClass = "buttonField";
            //    HtmlGenericControl a = new HtmlGenericControl("a");
            //    a.InnerText = fieldRow.FldInfo.Libelle;
            //    a.Attributes.Add("href", "#");
            //    panel.Controls.Add(a);

            //    ednWebCtrl = new EdnWebControl() { WebCtrl = panel, TypCtrl = EdnWebControl.WebControlType.PANEL };
            //}
            #endregion
            else if (bFieldValueInAnInnerPanel)
                ednWebCtrl = new EdnWebControl() { WebCtrl = new Panel(), TypCtrl = EdnWebControl.WebControlType.PANEL };
            else if (bFieldValueInAnInnerLabel)
            {
                if (format == FieldFormat.TYP_MEMO)
                {
                    TextBox tb = new TextBox();
                    tb.TextMode = TextBoxMode.MultiLine;
                    ednWebCtrl = new EdnWebControl() { WebCtrl = tb, TypCtrl = EdnWebControl.WebControlType.TEXTBOX };
                }
                else
                {
                    ednWebCtrl = new EdnWebControl() { WebCtrl = new Label(), TypCtrl = EdnWebControl.WebControlType.LABEL };
                }
            }
            //BSE US:765 Rubrique type mot de passe
            else if (fieldRow.FldInfo.Format == FieldFormat.TYP_PASSWORD)
                ednWebCtrl = new EdnWebControl() { WebCtrl = new TableCell(), TypCtrl = EdnWebControl.WebControlType.LABEL };
            else
                ednWebCtrl = new EdnWebControl() { WebCtrl = new TableCell(), TypCtrl = EdnWebControl.WebControlType.TABLE_CELL };
            WebControl webControl = ednWebCtrl.WebCtrl;

            // && ((fieldRow.IsLink && _rType == RENDERERTYPE.MainFile) || !fieldRow.IsLink)
            // Couleur valeur
            if (!String.IsNullOrEmpty(fieldRow.FldInfo.ValueColor) && (fieldRow.FldInfo.Descid - fieldRow.FldInfo.Descid % 100) == _tab)
            {
                webControl.CssClass = "useValueColor";
                webControl.Style.Add(HtmlTextWriterStyle.Color, fieldRow.FldInfo.ValueColor);
            }

            //utilisé pour afficher une enveloppe dans les modes listes à coté de l'adresse e-mail
            //pour permettre à l'utilisateur d'envoyer un mail directement depuis l'application.
            // #59 789 : ou un bouton SMS pour les numéros de téléphone
            Panel webComplementControl = null;

            //Construit la liste des champs affichés en mode fiche
            if (_bIsEditRenderer)
            {
                if (fieldRow.RightIsUpdatable || (row.MainFileid == 0 && fieldRow.Value.Length > 0))
                {
                    FieldsDescId.AddContains(fieldRow.FldInfo.Descid.ToString());
                }

                if (fieldRow.RightIsUpdatable)
                {
                    AllowedFieldsDescId.AddContains(fieldRow.FldInfo.Descid.ToString());
                }
            }
            else if
                (
                    (_rType == RENDERERTYPE.MailFile && fieldRow.FldInfo.Descid % 100 == MailField.DESCID_MAIL_HISTO.GetHashCode()) ||
                    (_rType == RENDERERTYPE.SMSFile && fieldRow.FldInfo.Descid % 100 == MailField.DESCID_MAIL_HISTO.GetHashCode())
                )
            {
                FieldsDescId.AddContains(fieldRow.FldInfo.Descid.ToString());

                if (fieldRow.RightIsUpdatable)
                {
                    AllowedFieldsDescId.AddContains(fieldRow.FldInfo.Descid.ToString());
                }
            }

            // Id de la fiche du field
            Int32 fieldFileId = fieldRow.FileId;

            // Id de la table principale
            Int32 nMasterFileId = row.MainFileid;

            // Nom de la cellule - commun à toutes la colonne, entête inclus
            String colName = eTools.GetFieldValueCellName(row, fieldRow);
            String shortColName = colName.Replace("COL_", "");

            // ID de la cellule
            webControl.ID = eTools.GetFieldValueCellId(row, fieldRow, idx);
            webControl.Attributes.Add("ename", colName);

            // #49 045 - TabIndex (aka. TabOrder)
            // Son indexation doit commencer à 1
            // http://www.w3schools.com/tags/att_global_tabindex.asp
            // Pour éviter de mettre tous les TabIndex à 0, on utilise le DispOrder si TabIndex n'est pas renseigné en base
            webControl.Attributes.Add("tabindex", ((fieldRow.FldInfo.PosTabIndex == 0 ? fieldRow.FldInfo.PosDisporder : fieldRow.FldInfo.PosTabIndex) + 1).ToString());

            #region rubrique associée

            if (fieldRow.FldInfo.AssociateField?.Length > 0)
                webControl.Attributes.Add("assfld", fieldRow.FldInfo.AssociateField);

            #endregion

            #region infobulle sur la valeur
            // info bulle
            //ALISTER => Demande #82 217 (this._rType != RENDERERTYPE.PrintFile)
            if (this._rType != RENDERERTYPE.FormularFile && this._rType != RENDERERTYPE.PrintFile)
            {
                if (fieldRow.FldInfo.Descid == (int)CampaignField.RATING)
                {
                    // sur le score de campagne, l'infobulle est basé sur le détail du score, pas sur la valeur infobulle paramétrée en admin
                    if (fieldRow.ExtendedProperties != null && fieldRow.ExtendedProperties is ExtendedScoreDetail)
                    {
                        String sTT = ((ExtendedScoreDetail)fieldRow.ExtendedProperties).ToolTip.Replace("'", "\\'").Replace("[[BR]]", "<br />"); ;
                        webControl.Attributes.Add("onmouseover", String.Concat("st(event, '", sTT, "');"));
                        webControl.Attributes.Add("onmouseout", "ht();");
                    }
                }
                else if (!String.IsNullOrEmpty(fieldRow.FldInfo.ToolTipText))
                {
                    // #38353 - Les infos bulles ne doivent plus apparaitre sur le mouseover des valeurs, on doit afficher la valeur
                    if (!_bIsEditRenderer)
                    {
                        String sTT = fieldRow.FldInfo.ToolTipText.Replace("'", "\\'").Replace("[[BR]]", "<br />");
                        webControl.Attributes.Add("onmouseover", String.Concat("st(event, '", sTT, "');"));
                        webControl.Attributes.Add("onmouseout", "ht();");
                    }

                }
            }
            #endregion

            if (fieldRow.FldInfo.Watermark.Length > 0)
                webControl.Attributes.Add("placeholder", fieldRow.FldInfo.Watermark);


            //Si on est en type fiche, l'alignement n'est pas paramétrable 
            if (!_bIsFileRenderer)
                if (fieldRow.FldInfo.XrmAlignRight)
                    webControl.Style.Add(HtmlTextWriterStyle.TextAlign, "right");

            //si le champ a afficher est le status d'une campagne, on force l'alignement à droite
            //SHA : Backlog #1 516
            //if (
            //    _bIsFileRenderer
            //        && fieldRow.FldInfo.Table.DescId == TableType.CAMPAIGN.GetHashCode()
            //    && fieldRow.FldInfo.Descid == CampaignField.STATUS.GetHashCode()
            //  )
            //{
            //    webControl.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
            //}


            // Si on est sur la visualisation d'un type E-mail, les champs sont en lecture seule sauf Historisé
            if ((fieldRow.FldInfo.Table.EdnType == EdnType.FILE_MAIL || fieldRow.FldInfo.Table.EdnType == EdnType.FILE_SMS) && !_bIsEditRenderer && !bIsMailHistoField)
                fieldRow.RightIsUpdatable = false;

            //Si campagne non assistante et pas le champ description
            Boolean campaignNotUpdatable = fieldRow.FldInfo.Descid < ((int)TableType.CAMPAIGN + FREE_FIELDS_LIMIT.CAMPAIGN_MIN)
                || fieldRow.FldInfo.Descid > ((int)TableType.CAMPAIGN + FREE_FIELDS_LIMIT.CAMPAIGN_MAX);


            campaignNotUpdatable = campaignNotUpdatable
                && fieldRow.FldInfo.Descid != CampaignField.DESCRIPTION.GetHashCode()
                && fieldRow.FldInfo.Descid != CampaignField.HISTO.GetHashCode();

            if (fieldRow.FldInfo.Table.DescId == TableType.CAMPAIGN.GetHashCode()
                && (_rType != RENDERERTYPE.EditMailing && _rType != RENDERERTYPE.EditSMSMailing)
                && campaignNotUpdatable)
                fieldRow.RightIsUpdatable = false;

            // En admin, rubrique "Couleurs" pour Planning est en lecture seule
            if (fieldRow.FldInfo.Table.EdnType == EdnType.FILE_PLANNING
                && _rType == RENDERERTYPE.AdminFile
                && fieldRow.FldInfo.Descid % 100 == PlanningField.DESCID_CALENDAR_COLOR.GetHashCode())
                fieldRow.RightIsUpdatable = false;

            // Rend le rendu en lecture seule
            if (ReadonlyRenderer || fieldRow.FldInfo.Format == FieldFormat.TYP_PASSWORD)
                fieldRow.RightIsUpdatable = false;

            // Classe de la colonne - Classe de base
            StringBuilder sbClass = new StringBuilder();

            if (!fieldRow.RightIsVisible)
            {
                GetValueContentControl(ednWebCtrl, fieldRow.DisplayValue);
            }
            else
            {
                // Visualisation autorisée

                // Récupération de :
                //  -> Valeur SQL
                //  -> Valeur Affichée
                //  -> Class CSS
                //  -> Attributs Spécifiques
                //   --> Les js d'action sont gérés post traitement via class css


                //Initialisation des variables	de construction des input                


                // Classe définissant l'action JS a pluger sur le field
                /*  LNKGOFILE   : Lien vers une fiche
                 *  LNKCATFILE  : Lien vers un catalogue de type fiche 
                 *  LNKADVCAT   : Ouvre un catalogue avancé  
                 *  LNKDATE   : champ de type date
                 *  LNKFREETEXT : Lien vers un editeur texte libre
                 *  LNKCHECK    : Case à cocher
                 *  LNKWEBSIT   : champ de type site web
                 *  LNKSOCNET   : champ de type Reseau Social
                 *  LNKMAIL  : champ de type email
                 *  LNKOPENMAIL : Ouverture client mail
                 *  LNKCATUSER : catalogue utilisateur
                 *  LNKOPENFILE : Ouvre un fichier
                 *  LNKMNGFILE  : Ouvre une fenêtre de choix de fichier
                 *  LNKOPENMEMO  : ouvre un champ memo
                 *  LNKNUM      : Lien vers un editeur numérique
                 *  LNKBUTTON   : Type bouton
                 */
                String sClassAction = String.Empty;
                Boolean bNotesFromParent = false;


                // [#38132 KHA/MOU] le rendu de champ de liaison est toujours de type char 
                if (fieldRow.IsLink)
                {
                    RenderCharFieldFormat(row, fieldRow, ednWebCtrl, sbClass, ref sClassAction);
                }
                else
                {
                    // Suivant le format du champ,
                    // récupération de l'action associée au champ et des valeurs réelles/affichables du champ
                    switch (format)
                    {
                        #region TYPE CHAR

                        case FieldFormat.TYP_CHAR:
                            //Fait un rendu de type char
                            //cela permet de redéfinir la methode si on veut faire un affichage différent
                            //ref sClassAction : les modifs sur  cette variable sont prise en compte
                            RenderCharFieldFormat(row, fieldRow, ednWebCtrl, sbClass, ref sClassAction);
                            break;

                        #endregion

                        #region TYPE PHONE - OK

                        case FieldFormat.TYP_PHONE:
                            // Pas d'id fiche
                            if (fieldFileId <= 0 && fieldRow.FldInfo.Table.Alias != row.ViewTab.ToString())
                                break;

                            if (fieldRow.RightIsUpdatable)
                                sClassAction = "LNKPHONE";
                            else
                                sClassAction = "LNKPHONERO";

                            GetValueContentControl(ednWebCtrl, fieldRow.DisplayValue);

                            if (_ePref.SmsEnabled && fieldRow.Value.Length > 0 && webControl.GetType() == typeof(Panel) && (_rType == RENDERERTYPE.Bookmark || _rType == RENDERERTYPE.ListRendererMain))
                            {
                                webComplementControl = new Panel();
                                webComplementControl.CssClass = "icon-sms btn inTDbtn";
                                webComplementControl.Attributes.Add("eaction", "LNKSENDPHONE");
                                webComplementControl.Attributes.Add("efld", "1");
                                webComplementControl.Attributes.Add("eActTg", webControl.ID);
                            }

                            break;

                        #endregion

                        #region TYP DATE - OK

                        case FieldFormat.TYP_DATE:
                            // Pas d'id fiche
                            if (fieldFileId <= 0 && fieldRow.FldInfo.Table.Alias != row.ViewTab.ToString())
                                break;

                            if (fieldRow.RightIsUpdatable)
                                sClassAction = "LNKDATE";

                            GetValueContentControl(ednWebCtrl, fieldRow.DisplayValue);

                            if (RendererType == RENDERERTYPE.AdminFile)
                            {
                                webControl.Attributes.Add("dbv", fieldRow.Value);
                                webControl.Attributes.Add("readonly", "");
                                sbClass.ConcatValue("readonly");
                            }
                            else if (RendererType == RENDERERTYPE.FormularFile)
                            {
                                webControl.Attributes.Add("dbv", fieldRow.Value);
                            }
                            break;

                        #endregion

                        #region TYP BIT - OK

                        case FieldFormat.TYP_BIT:


                            // TODO pourquoi ne pas rédéfinir  RenderBitFieldFormat dans ePrintRenderer ?????
                            if (_rType == RENDERERTYPE.PrintFile)
                            {
                                Control cellContent = new Control();
                                cellContent.Controls.Add(new LiteralControl(fieldRow.Value == "1" ? eResApp.GetRes(Pref.LangId, 58) : eResApp.GetRes(Pref.LangId, 59)));
                                webControl.Controls.Add(cellContent);

                            }
                            else
                            {
                                //Fait un rendu du champ de type Binaire en checkbox par defaut, ou boutons radio (e-mail par exemple )
                                WebControl checkBox = RenderBitFieldFormat(row, fieldRow, ref sClassAction);

                                if (checkBox != null)
                                {
                                    Control cellContent = new Control();

                                    cellContent.Controls.Add(checkBox);
                                    webControl.Controls.Add(cellContent);

                                    webControl.Attributes.Add("ednt", "LNKCHECK");
                                }
                            }

                            break;
                        #endregion

                        #region TYP EMAIL - OK

                        case FieldFormat.TYP_EMAIL:
                            //webControl.ToolTip = eResApp.GetRes(Pref.Lang, 1243); //TODO - à retirer si ctrl+click n'est plus d'actualité


                            if (fieldRow.RightIsUpdatable)
                                sClassAction = "LNKMAIL";
                            else
                            {
                                if (fieldRow.Value.Length > 0 && (_rType == RENDERERTYPE.EditFileLite || _rType == RENDERERTYPE.EditFile))
                                    sClassAction = "LNKMAILRO";
                            }

                            GetValueContentControl(ednWebCtrl, fieldRow.DisplayValue);

                            webControl.Attributes.Add("data-suggEnabled", fieldRow.FldInfo.EmailsSuggestionsEnabled ? "1" : "0");

                            if (fieldRow.Value.Length > 0 && webControl.GetType() == typeof(Panel) && (_rType == RENDERERTYPE.Bookmark || _rType == RENDERERTYPE.ListRendererMain))
                            {
                                webComplementControl = new Panel();
                                webComplementControl.CssClass = "icon-email btn inTDbtn";
                                webComplementControl.Attributes.Add("eaction", "LNKSENDMAIL");
                                webComplementControl.Attributes.Add("efld", "1");
                                webComplementControl.Attributes.Add("eActTg", webControl.ID);
                            }

                            break;

                        #endregion

                        #region TYP WEB - OK

                        case FieldFormat.TYP_WEB:
                            sClassAction = GetFieldValueCell_TYP_WEB(ednWebCtrl, fieldRow);
                            break;

                        #endregion

                        #region TYP SOCIAL NETWORK - TODO

                        case FieldFormat.TYP_SOCIALNETWORK:
                            webControl.Attributes.Add("eRootUrl", fieldRow.FldInfo.AliasSourceField?.RootURL ?? fieldRow.FldInfo.RootURL);
                            sClassAction = GetFieldValueCell_TYP_SOCIALNETWORK(ednWebCtrl, fieldRow);
                            break;

                        #endregion

                        #region TYP IFRAME - OK

                        case FieldFormat.TYP_IFRAME:


                            HtmlGenericControl iFrame = eTools.GetFieldIFrame(_ePref, eModelTools.ReplaceEudoParamInURL(_ePref, fieldRow.Value, row.GetFields), fieldRow.FldInfo.Table.DescId, fieldRow.FldInfo.Descid, fieldRow.FileId);

                            iFrame.Style.Add("height", "100%");
                            iFrame.Style.Add("width", "95%");
                            //CNA - Demande #52769 - On retire le cadre autour du rendu des champs de type pageweb
                            iFrame.Style.Add("border", "none");
                            webControl.Controls.Add(iFrame);

                            break;

                        #endregion

                        #region TYP CHART - OK

                        case FieldFormat.TYP_CHART:
                            // rendu en mode administration
                            if (_rType == RENDERERTYPE.AdminFile)
                            {
                                // TODO - C'est moche - Ouverture et fermeture d'une connexion SQL pour chaque rubrique de type CHARTS
                                // récupération des paramètres du chart
                                eFieldChart ef = eFieldChart.GetFieldChart(fieldRow.FldInfo.Descid, _ePref);

                                string reportName = String.Empty;
                                if (ef.ChartId != 0)
                                {
                                    reportName = ef.ChartLabel;
                                }

                                Panel reportNamePanel = new Panel();


                                Literal reportNameLiteral = new Literal();
                                if (!String.IsNullOrEmpty(reportName))
                                    reportNameLiteral.Text = String.Concat("[", eResApp.GetRes(Pref, 1005), " : ", reportName, "]");
                                else
                                    reportNameLiteral.Text = String.Concat("[", eResApp.GetRes(Pref, 1005), "]");

                                if (ef.ChartId != 0)
                                {
                                    HtmlGenericControl openReportListlink = new HtmlGenericControl("a");
                                    openReportListlink.Attributes.Add("onclick", String.Concat("nsAdminField.editFieldProperties(", fieldRow.FldInfo.Descid, ");nsAdminField.showChartsList(", ef.ChartId, ");"));
                                    openReportListlink.Attributes.Add("title", eResApp.GetRes(Pref, 7633));

                                    openReportListlink.Controls.Add(reportNameLiteral);
                                    reportNamePanel.Controls.Add(openReportListlink);
                                }
                                else
                                {
                                    reportNamePanel.Controls.Add(reportNameLiteral);
                                }

                                webControl.Controls.Add(reportNamePanel);
                            }
                            else if (fieldRow.FileId > 0)
                            {
                                // TODO - C'est moche - Ouverture et fermeture d'une connexion SQL pour chaque rubrique de type CHARTS
                                // récupération des paramètres du chart
                                eFieldChart ef = eFieldChart.GetFieldChart(fieldRow.FldInfo.Descid, _ePref);

                                if (_rType == RENDERERTYPE.ListRendererMain
                                    || _rType == RENDERERTYPE.FullMainList
                                    || _rType == RENDERERTYPE.Bookmark)
                                {
                                    sClassAction = "LNKOPENCHART";
                                    webControl.Attributes.Add("data-reportid", ef.ChartId.ToString());

                                    // En mode liste, on affiche seulement une icône
                                    HtmlGenericControl cellContent = new HtmlGenericControl();
                                    cellContent.Attributes.Add("class", "icon-bar-chart");

                                    webControl.Controls.Add(cellContent);

                                }
                                else
                                {
                                    eRenderer chart = eRendererFactory.CreateChartRenderer(_ePref, ef.ChartId, true);
                                    chart.PgContainer.Style.Add("width", String.Concat(ef.Width.ToString(), "px"));
                                    chart.PgContainer.Style.Add("height", String.Concat(ef.Height.ToString(), "px"));
                                    chart.PgContainer.ID = String.Concat("mainChart_", ef.ChartId);
                                    chart.PgContainer.CssClass = "fileChartMain";

                                    // Création du div container du div pour le zoom

                                    HtmlGenericControl lineZoom = new HtmlGenericControl("div");
                                    lineZoom.Attributes.Add("class", "lineZoom");
                                    lineZoom.ID = "lineZoom";

                                    // Création du div pour le zoom
                                    Panel pnlChartHeader = new Panel();
                                    pnlChartHeader.CssClass = "divZoom";
                                    pnlChartHeader.ID = "divZoom";

                                    // Action du zoom du graphique
                                    String sJsZoom = String.Concat("ZoomBookmark('mainChart_", ef.ChartId, "');");
                                    pnlChartHeader.Attributes.Add("onclick", sJsZoom);

                                    chart.PgContainer.Controls.AddAt(0, lineZoom);


                                    lineZoom.Controls.Add(pnlChartHeader);

                                    chart.PgContainer.Controls.Add(renderer.eSyncFusionChartRenderer.GetHtmlChart(_ePref, ef.ChartId.ToString(), fromHomePage: false));


                                    webControl.Controls.Add(chart.PgContainer);
                                }

                            }

                            break;

                        #endregion

                        #region TYP USER/GROUP - OK

                        case FieldFormat.TYP_USER:
                        case FieldFormat.TYP_GROUP:
                            //N'affiche la case que si elle est liée à une fiche
                            if (fieldFileId <= 0 && fieldRow.FldInfo.Table.Alias != row.ViewTab.ToString())
                                break;

                            GetValueContentControl(ednWebCtrl, fieldRow.DisplayValue);

                            //if (RendererType == RENDERERTYPE.AdminFile
                            //    && fieldRow.FldInfo.Descid % 100 == AllField.OWNER_USER.GetHashCode()
                            //    && !String.IsNullOrEmpty(fieldRow.FldInfo.DefaultValue))
                            //{
                            //    webControl.Attributes.Add("dbv", fieldRow.FldInfo.DefaultValue);
                            //}
                            if (String.IsNullOrEmpty(fieldRow.Value)
                                && (fieldRow.FldInfo.Descid == (int)FilterField.USERID
                                || fieldRow.FldInfo.Descid == (int)ImportTemplateField.USERID
                                || fieldRow.FldInfo.Descid == (int)TableType.MAIL_TEMPLATE + (int)AllField.OWNER_USER))
                                webControl.Attributes.Add("dbv", "0");
                            else
                                webControl.Attributes.Add("dbv", fieldRow.Value);

                            // Flag le champ comme devant gérer les conflit de planning
                            if (fieldRow.FldInfo.Table.EdnType == EdnType.FILE_PLANNING
                                && _ePref.GetPref(fieldRow.FldInfo.Table.DescId, ePrefConst.PREF_PREF.CALENDARENABLED).Equals("1")
                                &&
                                (fieldRow.FldInfo.Descid == fieldRow.FldInfo.Table.GetOwnerDescId() || fieldRow.FldInfo.Descid == fieldRow.FldInfo.Table.GetMultiOwnerDescId()))
                            {
                                webControl.Attributes.Add("calconflict", "1");
                            }

                            if (fieldRow.FldInfo.AliasSourceField?.IsTreeViewUserList ?? fieldRow.FldInfo.IsTreeViewUserList)
                                webControl.Attributes.Add("treeviewuserlist", "1");

                            if (fieldRow.FldInfo.AliasSourceField?.IsFullUserList ?? fieldRow.FldInfo.IsFullUserList)
                                webControl.Attributes.Add("fulluserlist", "1");

                            if (fieldRow.FldInfo.Format == FieldFormat.TYP_GROUP)
                                webControl.Attributes.Add("showcurrentuser", "0");
                            else if (fieldRow.FldInfo.Format == FieldFormat.TYP_USER || fieldRow.FldInfo.Descid.ToString().EndsWith("99"))
                                webControl.Attributes.Add("showcurrentuser", "1");

                            if (bMultiple)
                                webControl.Attributes.Add("mult", "1");

                            // champ system créer par / modifié par
                            if (fieldRow.RightIsUpdatable)
                                sClassAction = "LNKCATUSER";

                            sbClass.ConcatValue("readonly");
                            break;

                        #endregion

                        #region TYP IMAGE - TODO : IMAGE PAR DEFAUT / IMAGE DANS LA BASE

                        case FieldFormat.TYP_IMAGE:

                            //Fait un rendu de l'image :  IMAGE PAR DEFAUT / IMAGE DANS LA BASE / IMAGE URL
                            RenderImageFieldFormat(row, fieldRow, webControl, themePaths, ref sClassAction, nbCol, nbRow);
                            break;

                        #endregion

                        #region TYP MEMO - OK

                        case FieldFormat.TYP_MEMO:

                            bNotesFromParent = fieldRow.FldInfo.Table.Alias != row.ViewTab.ToString();

                            //Fait un rendu du champ mémo html/text
                            RenderMemoFieldFormat(row, fieldRow, ednWebCtrl, webControl, sbClass, ref sClassAction);
                            break;

                        #endregion

                        #region TYP NUMERIC - OK

                        case FieldFormat.TYP_AUTOINC:
                        case FieldFormat.TYP_MONEY:
                        case FieldFormat.TYP_NUMERIC:

                            RenderNumericFieldFormat(row, fieldRow, ednWebCtrl, format, sbClass, ref sClassAction);
                            break;

                        #endregion

                        #region TYP FILE - OK

                        case FieldFormat.TYP_FILE:
                            if (!String.IsNullOrEmpty(fieldRow.Value) && !bMultiple)
                                sClassAction = "LNKOPENFILE";
                            else if (fieldRow.RightIsUpdatable)
                                sClassAction = "LNKMNGFILE";

                            GetValueContentControl(ednWebCtrl, fieldRow.DisplayValue);
                            sbClass.ConcatValue("readonly");
                            webControl.Attributes.Add("pdbv", fieldRow.BoundFieldValue);
                            //#50617 champ type File: permettre la mise à jour à NULL si ausune valeur n'est séléctionnée 
                            webControl.Attributes.Add("dbv", fieldRow.Value);
                            if (bMultiple)
                                webControl.Attributes.Add("mult", "1");

                            break;

                        #endregion

                        #region type ID
                        case FieldFormat.TYP_ID:
                            RenderIDFieldFormat(row, fieldRow, ednWebCtrl, sbClass, ref sClassAction);

                            if (RendererType == RENDERERTYPE.AdminFile)
                            {
                                webControl.Attributes.Add("readonly", "");
                                sbClass.ConcatValue("readonly");
                            }


                            break;
                        #endregion

                        #region TYP Geographique

                        case FieldFormat.TYP_GEOGRAPHY_V2:
                            if (fieldRow.RightIsUpdatable)
                                sClassAction = "LNKGEO";
                            webControl.Attributes.Add("dbv", fieldRow.Value); // Ajout du dbv pour pouvoir l'enregistrer en mode popup dans les properties
                            GetValueContentControl(ednWebCtrl, fieldRow.DisplayValue);

                            sbClass.ConcatValue("readonly");
                            break;

                        #endregion

                        #region TYP BUTTON
                        case FieldFormat.TYP_BITBUTTON:

                            //ALISTER => Demande 77 923
                            sClassAction = (fieldRow.RightIsUpdatable) ? "LNKBITBUTTON" : "";

                            WebControl btn = RenderBitButtonFieldFormat(row, fieldRow);

                            if (btn != null)
                            {
                                Control cellContent = new Control();
                                cellContent.Controls.Add(btn);
                                webControl.Controls.Add(cellContent);
                            }

                            break;
                        #endregion

                        #region TYP AUTRE  - OK

                        default:
                            GetValueContentControl(ednWebCtrl, fieldRow.DisplayValue);
                            break;

                            #endregion
                    }
                }

                //#38353 - Sur les fiches, on affiche le contenu du champ sur les mouseover
                //#60548 - On n'affiche pas de tooltip sur les champs mémo
                if (_bIsEditRenderer && format != FieldFormat.TYP_MEMO)
                {
                    String sTT = String.Empty;

                    if (format == FieldFormat.TYP_BITBUTTON)
                        sTT = HtmlTools.StripHtml(fieldRow.FldInfo.ToolTipText);
                    else
                        sTT = HtmlTools.StripHtml(fieldRow.DisplayValue);

                    webControl.Attributes.Add("title", sTT);
                }

                //SHA : tâche #2 047
                if (fieldRow.FldInfo.Descid == (int)CampaignField.PREHEADER && ednWebCtrl.TypCtrl == EdnWebControl.WebControlType.TEXTBOX && format == FieldFormat.TYP_CHAR)
                    webControl.Attributes.Add("maxlength", eLibConst.MAX_PREHEADER_LENGTH.ToString());

                //GCH : rendu des champs additionnels
                if (lstFieldRecord.Count > 1)
                {
                    String sSeparator = GetMultiFieldsSeparator(fieldRow.FldInfo);
                    String sCurrentValue = GetCurrentValueContentControl(ednWebCtrl);
                    // #32 049 - Affichage de &nbsp; incongru lors de la concaténation de plusieurs champs
                    if (sCurrentValue == "&nbsp;")
                        sCurrentValue = "";
                    foreach (eFieldRecord currentField in lstFieldRecord.GetRange(1, lstFieldRecord.Count - 1))
                    {
                        sCurrentValue = String.Concat(sCurrentValue, sSeparator);   //Le separateur est présent même si pas de droit d'affichage
                        if (currentField.RightIsVisible)
                            sCurrentValue = String.Concat(sCurrentValue, currentField.DisplayValue);
                    }
                    GetValueContentControl(ednWebCtrl, sCurrentValue);
                }

                if (!fieldRow.RightIsUpdatable || bNotesFromParent/* || RendererType == RENDERERTYPE.AdminFile*/)
                {
                    sbClass.ConcatValue("readonly");
                    webControl.Attributes.Add("readonly", "readonly");
                    webControl.Attributes.Add("ero", "1");

                    if (bNotesFromParent && fieldRow.RightIsUpdatable)
                    {
                        //uaoz = Update Allowed On Zoom
                        webControl.Attributes.Add("uaoz", "1");

                        //Indique que la note vient du parent
                        webControl.Attributes.Add("fromparent", "1");
                    }

                    // #57 465 - Si on traite un fichier de type Cible Etendue (ProspectEnabled) en création (FileId == 0) et que le champ actuellement traité est obligatoire alors qu'il a été placé en lecture seule (!RightIsUpdatable),
                    // on supprime la condition d'obligation afin d'autoriser la création de la fiche à vide (les valeurs des champs mappés seront renseignées à partir de celles du fichier lié, cf. demande 39 838)
                    if (fieldRow.IsMandatory && fieldRow.FldInfo.Table.ProspectEnabled && fieldRow.FileId == 0)
                        fieldRow.IsMandatory = false;
                }

                if (RendererType == RENDERERTYPE.AdminFile)
                {
                    webControl.Attributes.Add("did", fieldRow.FldInfo.Descid.ToString());
                }

                // Ajoute la class d'action
                webControl.Attributes.Add("eaction", sClassAction);
                sbClass.ConcatValue(sClassAction);

                // Si une action est définie pour le champ, flag la cellule
                if (!String.IsNullOrEmpty(sClassAction))
                    webControl.Attributes.Add("efld", "1");

                switch (sClassAction)
                {
                    case "LNKGOUSERFILE":
                        sbClass.ConcatValue("gofile");
                        break;
                    case "LNKOPENRGPDLOGFILE":
                        sbClass.ConcatValue("gofile");
                        break;
                    case "LNKGOHOMEFILE":
                        sbClass.ConcatValue("gofile");
                        break;
                    case "LNKGOFILE":
                        sbClass.ConcatValue("gofile");
                        break;
                    case "LNKCHECK":
                        break;
                    case "LNKOPENPJ":
                        sbClass.ConcatValue("gofile");      // pour les pj ajoute la classe indiquant qu'il s'agit d'un lien cliquable
                        break;
                    default:
                        // Ajoute la class de cellule éditable
                        if (fieldRow.RightIsUpdatable)
                        {
                            if (fieldRow.FldInfo.XrmAlignRight && (RendererType == RENDERERTYPE.MainFile))
                                sbClass.ConcatValue("editl");
                            else
                                sbClass.ConcatValue("edit");
                        }
                        break;
                }

                if (fieldRow.IsMandatory)
                {
                    webControl.Attributes.Add("obg", "1");
                    webControl.Attributes.Add("required", "");
                    webControl.Attributes.Add("obg", "1");
                    if (webControl.GetType() == typeof(TextBox))
                        sbClass.ConcatValue("obg");
                }

                // Case - Cas particulier surla representation du PP01 nom + prenom ...
                if (fieldRow.IsCaseOnThisFormat && (fieldRow.FldInfo.Descid != TableType.PP.GetHashCode() + 1 || fieldRow.NameOnly))
                {
                    switch (caseField)
                    {
                        case CaseField.CASE_UPPER:
                            sbClass.ConcatValue("Up");
                            break;
                        case CaseField.CASE_CAPITALIZE:
                            sbClass.ConcatValue("Cap");
                            break;
                        case CaseField.CASE_LOWER:
                            sbClass.ConcatValue("Lw");
                            break;
                    }
                }
            }

            webControl.CssClass = String.Concat(webControl.CssClass, " ", sbClass.ToString());

            if (colMaxValues != null && colMaxValues.ContainsKey(colName))
            {
                ListColMaxValues curColMax = colMaxValues[colName];
                String bigValue = curColMax.ColMaxValue;
                String curValue = HttpUtility.HtmlEncode(fieldRow.DisplayValue);

                if (eTools.MesureString(HttpUtility.HtmlDecode(curValue)) > eTools.MesureString(HttpUtility.HtmlDecode(bigValue)))
                    curColMax.ColMaxValue = curValue;
            }
            if (bFormular)
                return webControl;
            else
                if (webControl.GetType() != typeof(TableCell))
            {
                // Si webControl n'est pas un tablecell,
                // il faut l'encapsuler dans un tablecell

                TableCell tc = new TableCell();
                tc.Controls.Add(webControl);
                if (webComplementControl != null)
                {
                    tc.Controls.Add(webComplementControl);
                }

                if (ednWebCtrl?.AdditionalWebCtrl != null)
                    tc.Controls.Add(ednWebCtrl.AdditionalWebCtrl);

                if (bFieldValueInAnInnerPanel)
                {
                    tc.CssClass = webControl.CssClass;
                    webControl.CssClass = String.Concat("divct divct_", shortColName);
                }

                return tc;
            }
            else
                return (TableCell)webControl;
        }

        protected virtual EdnWebControl CreateEditEdnControl(eFieldRecord fieldRow)
        {
            if (fieldRow.FldInfo.Format == FieldFormat.TYP_PASSWORD)
                return new EdnWebControl() { WebCtrl = new TextBox() { TextMode = TextBoxMode.Password }, TypCtrl = EdnWebControl.WebControlType.PASSWORD_CELL };
            else
                return new EdnWebControl() { WebCtrl = new TextBox(), TypCtrl = EdnWebControl.WebControlType.TEXTBOX };
        }

        /// <summary>
        /// Rendu du champ numérique
        /// </summary>
        /// <param name="row">eRecord</param>
        /// <param name="fieldRow">eFieldRecord</param>
        /// <param name="ednWebControl">ednWebControl</param>
        /// <param name="format">FieldFormat</param>
        /// <param name="sbClass">StringBuilder de classe</param>
        /// <param name="sClassAction">Classe action</param>
        /// <returns></returns>
        protected virtual bool RenderNumericFieldFormat(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, FieldFormat format, StringBuilder sbClass, ref String sClassAction)
        {
            if (fieldRow.RightIsUpdatable)
                sClassAction = "LNKNUM";

            if (fieldRow.FldInfo.Descid == (int)FilterField.TAB
             || fieldRow.FldInfo.Descid == (int)HistoField.DESCID
              || fieldRow.FldInfo.Descid == (int)PaymentTransactionField.PAYTRANTARGETDESCID
              || fieldRow.FldInfo.Descid == (int)PaymentTransactionField.PAYTRANSTATUS
              || fieldRow.FldInfo.Descid == (int)PaymentTransactionField.PAYTRANCODE
              || fieldRow.FldInfo.Descid == (int)PaymentTransactionField.PAYTRANCATEGORY
              || fieldRow.FldInfo.Descid == (int)PaymentTransactionField.PAYTRANEUDOSTATUS
             || fieldRow.FldInfo.Descid == (int)HistoField.TYPE
             || fieldRow.FldInfo.Descid == (int)CampaignField.STATUS
             || fieldRow.FldInfo.Descid == (int)CampaignField.SENDTYPE
             || fieldRow.FldInfo.Descid == (int)CampaignField.MAILADDRESSDESCID
             || fieldRow.FldInfo.Descid == (int)CampaignStatsField.CATEGORY
             || fieldRow.FldInfo.Descid % 100 == (int)MailField.DESCID_MAIL_STATUS
             || fieldRow.FldInfo.Descid % 100 == (int)MailField.DESCID_MAIL_SENDTYPE
             || fieldRow.FldInfo.Descid % 100 == (int)PlanningField.DESCID_CALENDAR_ITEM
             || fieldRow.FldInfo.Descid == (int)RGPDTreatmentsLogsField.Type
             || fieldRow.FldInfo.Descid == (int)RGPDTreatmentsLogsField.Status
             )
                ednWebControl.WebCtrl.Attributes.Add("dbv", fieldRow.Value);


            GetValueContentControl(ednWebControl, fieldRow.DisplayValue);

            if (format == FieldFormat.TYP_AUTOINC && RendererType == RENDERERTYPE.AdminFile)
            {
                ednWebControl.WebCtrl.Attributes.Add("readonly", "");
                sbClass.ConcatValue("readonly");
            }

            return true;
        }

        /// <summary>
        /// Indique si le champ memo a générer est en html
        /// </summary>
        /// <param name="fieldRow"></param>
        /// <returns></returns>
        protected virtual bool RenderMemoFieldIsHtml(eFieldRecord fieldRow)
        {
            return fieldRow.FldInfo.AliasSourceField?.IsHtml ?? fieldRow.FldInfo.IsHtml;
        }

        /// <summary>
        /// Fait le rendu de champ de type memo
        /// </summary>
        /// <param name="Pref">preference utilisateur</param>
        /// <param name="themePaths">path de them</param>
        /// <param name="fieldRow"></param>
        /// <param name="webControl">control sur lequel on fait un rendu </param>
        /// <param name="sClassAction">class css a metter a jour</param>
        protected virtual void RenderMemoFieldFormat(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebCtrl, WebControl webControl, StringBuilder sbClass, ref String sClassAction)
        {
            Boolean bNotesFromParent = fieldRow.FldInfo.Table.Alias != row.ViewTab.ToString();

            //N'affiche la case que si elle est liée à une fiche
            if (fieldRow.FileId <= 0 && bNotesFromParent)
                return;

            //#49 151 Dans le cas ou nous sommes en mode liste on ajoute pas le suffixe POPUP car il n'y a pas de bouton pour l'ouverture
            // et le champs mémo devient non cliquable avec ce suffixe
            //BSE #50 866, si on est en mode liste et que le champ Notes c'est celui d'un parent, on autorise l'édition du champ notes
            if (fieldRow.RightIsUpdatable)
            {
                webControl.Attributes.Add("efld", "1");

                if (!_bIsEditRenderer || bNotesFromParent)
                    sClassAction = String.Concat("LNKOPENMEMO", bNotesFromParent && !_bList ? "POPUP" : "");
            }
            // Champ Mémo Texte brut ou HTML
            webControl.Attributes.Add("html", RenderMemoFieldIsHtml(fieldRow) ? "1" : "0");

            // On précise le DescID et le FileID en attributs du champ Mémo associé pour la mise à jour de la valeur en base
            // Exemple #47 223 : dans le cas où l'utilisateur change de signet, le curseur sort du champ et donc, la valeur du champ Mémo doit être sauvegardée.
            // Seulement, le changement de signet entraîne un rechargement de la page ! Ce qui empêche le moteur de mise à jour (eMemoEditor.update)
            // de retrouver ces infos sur le contexte de la page et fait échouer la mise à jour
            // Même problème que pour la demande #27 563
            // Ces attributs seront ensuite utilisés par eMain.updateFileMemo()
            webControl.Attributes.Add("descid", fieldRow.FldInfo.Descid.ToString());
            webControl.Attributes.Add("fileid", row.MainFileid.ToString());

            // Ajout d'attributs spécifiques sur la cellule du champ Mémo si celui-ci est sur un template de type E-mail,
            // qui seront interprétés par la fonction updateFileMemo() pour activer certaines fonctionnalités du champ Mémo
            // (barre d'outils spécifique, champs de fusion...)
            // #59 789 - ajouté pour les SMS pour la rétrocompatibilité des JS, même si le champ Mémo n'est pas censé être en format Texte
            if (
                RendererType == RENDERERTYPE.MailFile ||
                RendererType == RENDERERTYPE.SMSFile ||
                RendererType == RENDERERTYPE.EditMail ||
                RendererType == RENDERERTYPE.EditMailing ||
                RendererType == RENDERERTYPE.EditSMS ||
                RendererType == RENDERERTYPE.EditSMSMailing
            )
            {
                // Sélection de la barre d'outils à afficher et du mode d'affichage selon les besoins
                // Permet de renseigner la propriété toolbarType de l'objet eMemoEditor afin d'afficher la barre d'outils souhaitée
                // Si cet attribut n'est pas renseigné ou s'il spécifie une barre d'outils inexistante, la barre d'outils par défaut sera automatiquement utilisée
                // TOCHECK SMS
                if (_rType == RENDERERTYPE.EditMailing || _rType == RENDERERTYPE.EditSMSMailing)
                    webControl.Attributes.Add("toolbartype", "mailing");
                else
                    webControl.Attributes.Add("toolbartype", "mail");

                // Type de champ Mémo
                webControl.Attributes.Add("editortype", webControl.Attributes["toolbartype"]);

                // #68 13x - Type de champ Mémo - Editeur de templates HTML avancé (grapesjs) ou CKEditor
                // Indique si on doit instancier un éditeur de templates HTML avancé (ex : pour l'e-mailing)
                webControl.Attributes.Add("enabletemplateeditor",
                    (
                        // grapesjs désactivé sur les mails unitaires pour le moment (cf. backlog #43) - Pour réactiver, décommenter/activer la condition ci-dessous
                        /* (_rType == RENDERERTYPE.EditMail && webControl.ID.EndsWith("_0")) ||*/ // Affichage du premier éditeur dans le cas d'un mail unitaire - TEST A REMPLACER PAR .UseCkEditor
                        (_rType == RENDERERTYPE.EditMailing && !((eEditMailingRenderer)this).UseCkeditor) // Affichage de grapesjs (= pas CKEditor) dans le cas d'un e-mailing
                    )
                    && (!ReadonlyRenderer) // 72869 - Pour la consultation de campagne/mail envoyé, on passe par ck pour l'instant qui supporte le mode lecture seule
                                           // Editeur de templates HTML avancé (grapesjs) disponible selon licence (E2017 uniquement)
                    && (
                            eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.HTMLTemplateEditor)
                          && _ePref.ClientInfos.ClientOffer > 0
                          && !eTools.IsMSBrowser
                    ) ? "1" : "0");

                //webControl.Attributes.Add("inlinemode", "1");

                // On indique le nom de la variable JavaScript (générée par le renderer, cf. eMailFileRenderer qui contiendra la liste des champs de fusion affichables
                webControl.Attributes.Add("mergefieldsjsvarname", "mailMergeFields");

                // On indique le nom de la variable JavaScript (générée par le renderer, cf. eEditMailFileRenderer) qui contiendra la liste des rubrique l’information de track 
                webControl.Attributes.Add("trackfieldsjsvarname", "oTrackFields");

                // On indique le nom de la variable JavaScript (générée par le renderer, cf. eEditMailFileRenderer) qui contiendra la liste des rubriques des Champs de fusions Emailing - Hyperlien 
                webControl.Attributes.Add("mergehyperlinkfieldsjsvarname", "oMergeHyperLinkFields");

                try
                {
                    //TODO : a optimiser : config adv est loader au chargement
                    //  a faire lorsque le reste du dev sera validé. Les options a activer/désactiver sont à définir (au 27/05/2015). 
                    IDictionary<eLibConst.CONFIGADV, String> dicEmailAdvConfig = eLibTools.GetConfigAdvValues(this._ePref,
                        new HashSet<eLibConst.CONFIGADV> {
                            eLibConst.CONFIGADV.EXTERNAL_TRACKING_ENABLED,
                            eLibConst.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD
                        });

                    if (dicEmailAdvConfig[eLibConst.CONFIGADV.EXTERNAL_TRACKING_ENABLED] == "1")
                        webControl.Attributes.Add("externaltracking", "1");

                    if (dicEmailAdvConfig[eLibConst.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD] == "1")
                        webControl.Attributes.Add("newunsubmethod", "1");
                }
                catch (Exception) { }


            }
            // Sinon, si on affiche un champ Mémo en mode Fiche, il faut ajouter une bordure sur la cellule de tableau car son affichage se fait en mode "inline"
            // sans bordures. On rajoute pour cela une classe qui pourra ensuite être contrôlée par une CSS
            else if (RendererType == RENDERERTYPE.EditFile || RendererType == RENDERERTYPE.EditFileLite || RendererType == RENDERERTYPE.MainFile || RendererType == RENDERERTYPE.AdminFile)
            {
                sbClass.ConcatValue("inlineMemoEditor");
                webControl.Attributes.Add("inlinemode", "1");
                webControl.Attributes.Add("onclick", "openChildDialogOnTablet(event, this)");
            }

            if (RenderMemoFieldIsHtml(fieldRow) && (!_bList))
            {
                if (bNotesFromParent)
                    fieldRow.DisplayValue = HtmlTools.SanitizeHtml(fieldRow.DisplayValue);

                GetHTMLMemoControl(ednWebCtrl, fieldRow.DisplayValue);
            }
            else
                GetRawMemoControl(ednWebCtrl, fieldRow.DisplayValue);

            try
            {
                if (_bIsFileRenderer && !bNotesFromParent)
                    MemoIds.Add(webControl.ID);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Fait un rendu du champ Image
        /// </summary>
        /// <param name="Pref">preference utilisateur</param>
        /// <param name="themePaths">path de them</param>
        /// <param name="fieldRow"></param>
        /// <param name="webControl">control sur lequel on fait un rendu </param>
        /// <param name="sClassAction">class css a metter a jour</param>
        /// <param name="nbCol">nombre de colonnes qu'occupera la cellule</param>
        /// <param name="nbRow">nombre de lignes qu'occupera la cellule</param>
        protected virtual void RenderImageFieldFormat(eRecord row, eFieldRecord fieldRow, WebControl webControl, ePref.CalculateDynamicTheme themePaths, ref String sClassAction,
            int nbCol = 1, int nbRow = 1)
        {
            ImageStorage imageStorage = fieldRow.FldInfo.AliasSourceField?.ImgStorage ?? fieldRow.FldInfo.ImgStorage;
            //N'affiche la case que si elle est liée à une fiche
            if (fieldRow.FileId <= 0 && fieldRow.FldInfo.Table.Alias != row.ViewTab.ToString())
                return;

            bool bRenderAsImg = true;
            Image oImg = new Image();
            String sImageUrl = String.Empty;
            String sImageClass = String.Empty; // #93 306 - Classe CSS supplémentaire(s) appliquée(s) sur la balise <img> selon les cas

            if (imageStorage == ImageStorage.STORE_IN_DATABASE) // Pas de test sur la valeur car Value ou DisplayValue est toujours vide pour ce type de stockage
            {
                sClassAction = "LNKOPENIMG";

                #region Image en BDD

                if (!_bList)
                {


                    int h = eConst.FILE_LINE_HEIGHT * fieldRow.FldInfo.PosRowSpan;
                    bool bDoTrait = true;
                    if (this is eEditFileRenderer && fieldRow.FileId == 0)
                    {
                        if (((eEditFileRenderer)this).File.DicValues?.ContainsKey(fieldRow.FldInfo.Descid) ?? false)
                        {
                            string sVal = ((eEditFileRenderer)this).File.DicValues[fieldRow.FldInfo.Descid];
                            if (!string.IsNullOrEmpty(sVal))
                            {

                                sImageUrl = "data:image/png;base64," + sVal;
                                oImg.Attributes.Add("width", h.ToString() + " px");
                                oImg.Attributes.Add("height", h.ToString() + " px");
                                oImg.Attributes.Add("isb64", "1");

                                bDoTrait = false;
                            }
                        }
                    }


                    if (bDoTrait)
                    {


                        sImageUrl = String.Concat("eImage.aspx?did=", fieldRow.FldInfo.Descid, "&fid=", fieldRow.FileId, "&it=IMAGE_FIELD&h=", h.ToString(), "&w=", h);
                    }
                }
                else
                {
                    // En mode liste, on appelle directement la classe eImageTools afin qu'elle nous renvoie simplement un booléen indiquant si une image est présente ou non,
                    // afin que l'on charge directement les images "génériques" sans passer par eImage.aspx (qui empêcherait le stockage de l'image en cache)
                    // Si on souhaitait afficher une miniature de l'image réellement stockée en base, il suffirait d'appeler directement la page eImage.aspx avec h=16 et
                    // w=16 pour que la page calcule automatiquement la miniature en question - ce qui demande un temps de traitement sensiblement plus long
                    bool bComputeRealThumbnail = false; // En "dur" pour le moment, à rendre paramétrable si besoin est
                    if (bComputeRealThumbnail)
                        sImageUrl = String.Concat("eImage.aspx?did=", fieldRow.FldInfo.Descid, "&fid=", fieldRow.FileId, "&w=16&h=16&it=IMAGE_FIELD");
                    else
                    {
                        sImageClass = String.Concat(sImageClass, " defaultThumbnail");
                        bool bCanAccessImage = false;
                        string strError = String.Empty;
                        Field fieldInfo = new Field();

                        eFieldRecord fieldRecord = new eFieldRecord();


                        if (eImageTools.DBImageExists(eLibTools.GetDBImageData(Pref, fieldRow.FldInfo.Descid, fieldRow.FileId, out fieldInfo, out fieldRecord, out bCanAccessImage, out strError)))
                            sImageUrl = String.Concat("themes/", themePaths.GetImageWebPath("/images/ui/picture.png"));
                        else
                            sImageUrl = String.Concat("themes/", themePaths.GetDefaultImageWebPath());

                    }

                    webControl.Attributes.Add("fid", fieldRow.FileId.ToString());
                }

                #endregion
            }
            else if (fieldRow.Value.Length != 0)
            {
                sClassAction = "LNKOPENIMG";

                #region Image en Fichier ou en Lien

                if (_bIsFileRenderer || _bIsPrintRenderer)
                {
                    // Mode fiche : image normale
                    if (imageStorage == ImageStorage.STORE_IN_URL)
                        sImageUrl = fieldRow.Value;
                    else
                        sImageUrl = String.Concat(eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.FILES, Pref.GetBaseName), "/", fieldRow.Value);

                    /* ELAIZ - Condition mise en comm pour le moment car le nouveau format d'image n'est pas fini (demande 75933)*/
                    //if (fieldRow.FldInfo.Descid % 100 != (int)AllField.AVATAR)
                    //{
                    oImg.Style.Add("max-height", "100%");
                    oImg.Style.Add("max-width", "100%");
                    //}

                    webControl.Style.Add("text-align", "center");
                }
                else
                {
                    // Autre mode : image miniature
                    if (imageStorage == ImageStorage.STORE_IN_URL)
                    {
                        bool bComputeRealThumbnail = false; // En "dur" pour le moment, à rendre paramétrable si besoin est
                        // Si on souhaitait afficher une miniature réelle de l'image, il suffirait d'appeler directement la page eImage.aspx avec h=16 et
                        // w=16 pour que la page calcule automatiquement la miniature en question - ce qui demande un temps de traitement sensiblement plus long
                        if (bComputeRealThumbnail)
                            sImageUrl = String.Concat("eImage.aspx?did=", fieldRow.FldInfo.Descid, "&fid=", fieldRow.FileId, "&w=16&h=16&it=IMAGE_FIELD");
                        else
                        {
                            sImageClass = String.Concat(sImageClass, " defaultThumbnail");
                            sImageUrl = String.Concat("themes/", themePaths.GetImageWebPath("/images/ui/picture.png"));
                        }
                    }
                    else
                    {
                        // TODO BENCHMARK - Pour le moment, la miniature est générée à la volée, et non sur disque.
                        // En attendant de réaliser un benchmark pour voir quelle méthode est la plus performante
                        //sImageUrl = eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.FILES, Pref) + "/" + eImageTools.CreateThumbnail(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.FILES, Pref) + @"\" + fieldRow.Value, 16, 16);
                        bool bComputeRealThumbnail = false; // En "dur" pour le moment, à rendre paramétrable si besoin est
                        if (bComputeRealThumbnail)
                        {
                            sImageUrl = String.Concat("eImage.aspx?did=", fieldRow.FldInfo.Descid, "&fid=", fieldRow.FileId, "&w=16&h=16&it=IMAGE_FIELD");
                        }
                        else if (imageStorage == ImageStorage.STORE_IN_FILE && !String.IsNullOrEmpty(fieldRow.Value) && Pref.ThumbNailEnabled)
                        {
                            String sOrigFileName = String.Concat(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.FILES, Pref.GetBaseName), @"\", fieldRow.Value);
                            String sThumb = eImageTools.GetThumbNailName(fieldRow.Value);
                            String sCompleteThumb = String.Concat(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.FILES, Pref.GetBaseName), @"\", sThumb);

                            if (!File.Exists(sCompleteThumb))
                            {
                                String sError = "";
                                sThumb = eImageTools.CreateThumbnail(sOrigFileName, eLibConst.THUMBNAILWIDTH, eLibConst.THUMBNAILHEIGHT, false, out sError);
                            }
                            sImageUrl = String.Concat(eLibTools.GetAppUrl(HttpContext.Current.Request), "/", eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.FILES, Pref.GetBaseName), "/", sThumb);
                        }
                        else
                        {
                            sImageClass = String.Concat(sImageClass, " defaultThumbnail");
                            sImageUrl = String.Concat("themes/", themePaths.GetImageWebPath("/images/ui/picture.png"));
                        }
                    }
                }

                webControl.Attributes.Add("dbv", fieldRow.Value);

                #endregion
            }
            // Pas d'image actuellement en base
            else if (fieldRow.RightIsUpdatable)
            {
                if (fieldRow.FileId <= 0 && RendererType != RENDERERTYPE.AdminFile)
                {
                    bool objSessionExists = false;
                    try
                    {
                        string tempImageSessionObjName = String.Concat("TempImageFile_", Pref.UserId, "_", fieldRow.FldInfo.Descid);

                        System.Web.HttpContext context = System.Web.HttpContext.Current;

                        objSessionExists = context.Session[tempImageSessionObjName] != null;
                    }
                    catch (Exception e) { }

                    if (objSessionExists)
                    {
                        // Dans le cas de la création d'une nouvelle fiche, on passe par eImage.aspx pour récupérer une éventuelle image stockée en session, si le renderer est
                        // sollicité à la suite d'un refresh type applyRuleOnBlank() (exemple : clic sur un autre type d'adresse lors d'une création de contact), ce qui permet
                        // de reprendre les images précédemment uploadées, si l'utilisateur est toujours en train de créer sa fiche.
                        // Il faut ajouter ici un timestamp sur l'URL pour court-circuiter le cache et éviter qu'une image précédemment uploadée lors de la création d'une fiche
                        // "réapparaîsse" sur le même champ lorsqu'on recrée une fiche sur le même onglet
                        // (en passant par eImage.aspx, le FileID étant à -1, l'URL "Pas d'image" reste la même pour chaque champ Image)
                        bool bComputeRealThumbnail = false; // En "dur" pour le moment, à rendre paramétrable si besoin est
                                                            // Si on souhaitait afficher une miniature réelle de l'image, il suffirait d'appeler directement la page eImage.aspx avec h=16 et
                                                            // w=16 pour que la page calcule automatiquement la miniature en question - ce qui demande un temps de traitement sensiblement plus long
                        if (bComputeRealThumbnail)
                            sImageUrl = String.Concat("eImage.aspx?did=", fieldRow.FldInfo.Descid, "&fid=-1&w=16&h=16&it=IMAGE_FIELD&ts=", DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                        else
                            sImageUrl = String.Concat("eImage.aspx?did=", fieldRow.FldInfo.Descid, "&fid=-1&it=IMAGE_FIELD&ts=", DateTime.Now.ToString("yyyyMMddHHmmssfff"));

                        oImg.Style.Add("max-height", "100%");
                        oImg.Style.Add("max-width", "100%");

                        // On ajoute l'attribut "session=1" sur l'image pour indiquer qu'il faudra déclencher son upload en JavaScript à la validation de la fiche
                        // C'est eImageManager qui se chargera de détecter s'il existe réellement une image stockée en session, ou non, et de réaliser l'upload en fonction
                        oImg.Attributes.Add("session", "1");

                        webControl.Style.Add("text-align", "center");
                    }
                    else
                    {
                        webControl.Controls.Add(GetEmptyImagePanel(nbRow));
                        bRenderAsImg = false;
                    }
                }
                else
                {
                    //sImageUrl = String.Concat("themes/", themePaths.GetDefaultImageWebPath());

                    webControl.Controls.Add(GetEmptyImagePanel(nbRow));
                    bRenderAsImg = false;
                }

                sClassAction = "LNKCATIMG";
                // Permet de transmettre à la valeur vide au applyruleonblanck ou a la duplication
                webControl.Attributes.Add("dbv", String.Empty);
            }
            else
                return;

            if (fieldRow.FldInfo.Descid == fieldRow.FldInfo.Table.DescId + ((int)AllField.AVATAR))
            {
                if (bRenderAsImg)
                {
                    oImg.Attributes.Add("fid", fieldRow.FileId.ToString());
                    oImg.Attributes.Add("tab", fieldRow.FldInfo.Table.DescId.ToString());
                }
                else
                {
                    webControl.Attributes.Add("fid", fieldRow.FileId.ToString());
                    webControl.Attributes.Add("tab", fieldRow.FldInfo.Table.DescId.ToString());
                }
                webControl.Attributes.Add("dbv", fieldRow.Value);
                webControl.Attributes.Add("eavatar", "1");


                if (fieldRow.FldInfo.Table.DescId == TableType.USER.GetHashCode())
                {
                    sClassAction = "LNKOPENUSERAVATAR";
                }
                else
                {
                    sClassAction = "LNKOPENAVATAR";
                }


            }


            // Drag&Drop
            if (imageStorage != ImageStorage.STORE_IN_URL && this.RendererType != RENDERERTYPE.AdminFile)
            {
                //webControl.Attributes.Add("ondragenter", "imageDragEnter(event);return false;");
                webControl.Attributes.Add("ondragover", "UpFilDragOver(this, event);return false;");
                webControl.Attributes.Add("ondragleave", "UpFilDragLeave(this); return false;");
                webControl.Attributes.Add("ondrop", "UpFilDrop(this,event,null,null,1);return false;");
            }

            if (bRenderAsImg)
            {
                oImg.Attributes.Add("onerror", "onErrorImg(this);");
                oImg.CssClass = sImageClass.Trim();
                oImg.ImageUrl = sImageUrl;
                webControl.Controls.Add(oImg);
            }
        }

        /// <summary>
        /// Retourne le rendu de la div pour les champs images vides
        /// Rendu identique fait dans eMain.js->onImageSubmit, à modifier en cas de modification ici
        /// </summary>
        /// <param name="nbRow">nombre de rowspan du champs</param>
        /// <returns></returns>
        protected Panel GetEmptyImagePanel(int nbRow)
        {
            Panel divImg = new Panel();
            divImg.CssClass = "icon-picture-o emptyPictureArea";
            divImg.Attributes.Add("data-eEmptyPictureArea", "1");
            if (nbRow > 0)
            {
                int nImgHeight = (eConst.FILE_LINE_HEIGHT * nbRow) - 12; //12 = (border 2px + padding 2px + margin 2px) x2
                divImg.Style.Add("font-size", String.Concat(nImgHeight.ToString(), "px"));
            }


            return divImg;
        }

        /// <summary>
        /// Fait un rendu du champ de type ID
        /// </summary>
        /// <param name="row">Ligne de la liste a afficher</param>
        /// <param name="fieldRow">Le record</param>
        /// <param name="ednWebControl">elment html dans lequel on fait le rendu</param>
        /// <param name="sbClass">classe CSS</param>
        /// <param name="sClassAction">la type d action</param>
        protected virtual Boolean RenderIDFieldFormat(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref String sClassAction)
        {
            try
            {
                GetValueContentControl(ednWebControl, fieldRow.DisplayValue);
                return true;
            }
            catch
            {
                throw;
            }

        }

        /// <summary>
        /// Fait un rendu du champ de type char
        /// </summary>
        /// <param name="row">Ligne de la liste a afficher</param>
        /// <param name="fieldRow">Le record</param>
        /// <param name="ednWebControl">elment html dans lequel on fait le rendu</param>
        /// <param name="sbClass">classe CSS</param>
        /// <param name="sClassAction">le type d action</param>
        protected virtual Boolean RenderCharFieldFormat(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref String sClassAction)
        {
            WebControl webControl = ednWebControl.WebCtrl;
            int iPopupDescId = fieldRow.FldInfo.AliasSourceField?.PopupDescId ?? fieldRow.FldInfo.PopupDescId;
            PopupType popupTyp = fieldRow.FldInfo.AliasSourceField?.Popup ?? fieldRow.FldInfo.Popup;

            // ParentDataId
            if (fieldRow.BoundFieldValue != null)
                webControl.Attributes.Add("pdbv", fieldRow.BoundFieldValue);

            if (fieldRow.FldInfo.Descid == (TableType.PP.GetHashCode() + 1) && fieldRow.NameOnly)
                webControl.Attributes.Add("nameonly", "");

            if (fieldRow.FldInfo.Descid == ((int)TableType.ADR + 1) && !fieldRow.IsLink)
                webControl.Attributes.Add("adrperso", "1");

            // Champ Principal de Event (EVT01) - PP01  - PM01 - ADR01 (pro) - specialcat
            if ((_rType == RENDERERTYPE.MainFile
                   || _rType == RENDERERTYPE.EditFile
                   || _rType == RENDERERTYPE.EditFileLite
                   || _rType == RENDERERTYPE.AdminFile
                    ) && fieldRow.FldInfo.Descid == fieldRow.FldInfo.Table.MainFieldDescId
                      && row.ViewTab == fieldRow.FldInfo.Table.DescId
                      && fieldRow.FldInfo.Table.EdnType == EdnType.FILE_MAIN
               //&& fieldRow.FldInfo.Descid != TableType.ADR.GetHashCode() + 1 // #39616
               //&& !fieldRow.IsLink //#38114 
               )
            {
                //cas du mode fiche
                if (_rType != RENDERERTYPE.AdminFile)
                    webControl.Attributes.Add("dbv", fieldRow.Value);

                if (fieldRow.RightIsUpdatable)
                    sClassAction = "LNKFREETEXT";
            }
            else if (Pref.AdminMode && fieldRow.FldInfo.Descid == (int)XrmHomePageField.Title)
            {
                sClassAction = "LNKGOHOMEFILE";
                webControl.Attributes.Add("lnkid", fieldRow.FileId.ToString());
            }
            else if (Pref.AdminMode && fieldRow.FldInfo.Descid == (int)UserField.LOGIN && _bList)
            {

                sClassAction = "LNKGOUSERFILE";
                webControl.Attributes.Add("lnkid", fieldRow.FileId.ToString());
            }
            else if (Pref.AdminMode && fieldRow.FldInfo.Descid == (int)RGPDTreatmentsLogsField.Label && _bList)
            {

                sClassAction = "LNKOPENRGPDLOGFILE";
                webControl.Attributes.Add("lnkid", fieldRow.FileId.ToString());
            }
            else if (fieldRow.FldInfo.Table.EdnType == EdnType.FILE_HISTO && fieldRow.FldInfo.Descid == HistoField.FILEID.GetHashCode())
            {
                eRecordHisto rowHisto = (eRecordHisto)row;

                // Gestion de la valeur d'historique avec lien vers une autre fiche
                if (!rowHisto.HistoEmptySpFiche)
                {
                    sClassAction = "LNKGOFILE";
                    webControl.Attributes.Add("lnkdid", rowHisto.HistoTabFiche.ToString());
                    webControl.Attributes.Add("lnkid", fieldRow.Value);
                }
            }
            else if (fieldRow.FldInfo.Table.EdnType == EdnType.FILE_HISTO && fieldRow.FldInfo.Descid == (int)HistoField.EXPORT_TAB)
            {
                webControl.Attributes.Add("dbv", fieldRow.Value);
            }
            else if (fieldRow.FldInfo.Descid == (int)CampaignField.RECIPIENTTABID)
            {
                webControl.Attributes.Add("dbv", fieldRow.Value);
            }
            else if (fieldRow.FldInfo.Descid == (int)PJField.TYPE
                || fieldRow.FldInfo.Descid == (int)PJField.FILE)
            {
                webControl.Attributes.Add("dbv", fieldRow.Value);
            }
            // MAB - US #1586 - Tâche #3265 - Demande #75 895 - Minifiche sur les champs Alias            
            else if (fieldRow.IsLink || fieldRow.FldInfo.AliasSourceField?.Popup == PopupType.SPECIAL || popupTyp == PopupType.SPECIAL)
            {
                eFieldRecord referenceFieldRow = fieldRow.AliasSourceFieldRecord ?? fieldRow;

                //TODO : si pas de liaison direct,
                if (referenceFieldRow.RightIsUpdatable && referenceFieldRow.FldInfo.Alias.Split('_').Length <= 2)
                    //Modif seulement en Fiche sur les liaisons du haut
                    sClassAction = "LNKCATFILE";

                //#47296
                //les champs de liaison PP ou PM peuvent ou pas cascader sur PM ou PP en fonction des champ de desc NoCascadePPPM/NoCascadePMPP                   
                if ((referenceFieldRow.IsLink && fieldRow.FldInfo.AliasSourceField != null)
                        && row.MainField != null
                        && (
                            (referenceFieldRow.FldInfo.Descid == 201 && row.MainField.Table.NoCascadePPPM)
                            || (referenceFieldRow.FldInfo.Descid == 301 && row.MainField.Table.NoCascadePMPP)
                            )
                    )
                {
                    webControl.Attributes.Add("nocc", "1");
                }
                // Correctif #50029 commenté 
                //int nLnkId = eLibTools.GetNum(eTools.GetLnkId(fieldRow, this));
                int nLnkId = eLibTools.GetNum(eTools.GetLnkId(referenceFieldRow));
                if (nLnkId != 0)
                {
                    if (referenceFieldRow.FldInfo.Descid % 100 == 01)
                    {
                        webControl.Attributes.Add("lnkid", nLnkId.ToString());
                        if (referenceFieldRow.FldInfo.Descid == TableType.PP.GetHashCode() + 1)
                            webControl.Attributes.Add("dbv", referenceFieldRow.Value);
                    }
                    else if (referenceFieldRow.FldInfo.Descid == PJField.FILEID.GetHashCode())
                    {
                        eRecordPJ rowPj = (eRecordPJ)row;
                        webControl.Attributes.Add("lnkdid", rowPj.PJTabDescID.ToString());
                        webControl.Attributes.Add("dbv", referenceFieldRow.Value);
                    }
                    else
                    {
                        webControl.Attributes.Add("dbv", nLnkId.ToString());

                    }

                    if (nLnkId > 0)
                        sClassAction = "LNKGOFILE";

                    //CNA - Minifiches/VCard
                    List<RENDERERTYPE> listRenderTypeMiniFiche = new List<RENDERERTYPE>() {
                            RENDERERTYPE.MainFile,
                            RENDERERTYPE.FileParentInHead,
                            RENDERERTYPE.FileParentInFoot,
                            RENDERERTYPE.EditFileLite
                        };
                    if ((iPopupDescId % 100 == 1 || referenceFieldRow.FldInfo.Descid % 100 == 1) && listRenderTypeMiniFiche.Contains(RendererType))
                    {
                        int lnkTab = (iPopupDescId % 100 == 1 ? iPopupDescId : referenceFieldRow.FldInfo.Descid) - 1;
                        if (lnkTab == TableType.PP.GetHashCode())
                        {
                            //??Activation de la VCard
                            //VCMouseActionAttributes(webControl);
                        }
                        else
                        {
                            if (VCMiniFileCheckMappingEnabled(lnkTab))
                                VCMiniFileMouseActionAttributes(webControl, lnkTab);
                        }
                    }
                }

                //Champ de liaison : précise le descid du champ de liaison
                if (referenceFieldRow.IsLink)
                {
                    webControl.Attributes.Add("fldlnk", referenceFieldRow.FldInfo.Table.FieldLink.ToString());
                }

                //TODO : Vérifier pourquoi on ajoute la classe readonly...
                // J'ai regardé d'ou ca viens et j'ai copié le commentaire de Kha
                //khau  20/11/2012 11:32:39
                //Mode fiche : Griser les champs catalogues et ne pas mettre le curseur dans l’input lorsque l’utilisateur ne peut pas rentrer de valeur personnalisée
                sbClass.ConcatValue("readonly");

                /*Si lien vers PP01 on affiche la VCARD*/
                if (iPopupDescId == 201
                    && referenceFieldRow.FldInfo.Descid != 201
                    && _rType == RENDERERTYPE.PrintFile
                    && (!String.IsNullOrEmpty(
                    _ePref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[] { eLibConst.CONFIG_DEFAULT.VCARDMAPPING })[eLibConst.CONFIG_DEFAULT.VCARDMAPPING]
                    )))
                {
                    VCMouseActionAttributes(webControl);
                }
            }
            else if (popupTyp == PopupType.DESC)
            {
                webControl.Attributes.Add("dbv", fieldRow.Value);

                if (fieldRow.RightIsUpdatable)
                    sClassAction = "LNKCATDESC";

                sbClass.ConcatValue("readonly");
            }
            else if (popupTyp == PopupType.ENUM)
            {
                webControl.Attributes.Add("dbv", fieldRow.Value);

                if (fieldRow.RightIsUpdatable)
                    sClassAction = "LNKCATENUM";

                sbClass.ConcatValue("readonly");
            }
            else if (popupTyp == PopupType.DATA) //Catalogue avancé
            {
                webControl.Attributes.Add("dbv", fieldRow.Value);

                if (fieldRow.FldInfo.AliasSourceField?.IsTreeViewOnlyLastChildren ?? fieldRow.FldInfo.IsTreeViewOnlyLastChildren)
                    webControl.Attributes.Add("lastdataid", fieldRow.Value); // seulement les derniers ID de l'arbo

                Field fld = fieldRow.FldInfo.AliasSourceField ?? fieldRow.FldInfo;

                /* US #4141 - "En nouvel Eudonet X, la rubrique Catalogue ne doit pas apparaitre sous forme d'étape"
                Règles :
                - Sur l'admin d'un onglet uniquement
                - Uniquement sur les catalogues simples non arborescents
                - Cas 1 : si la fiche est en état : activé, alors le catalogue apparait dans sa forme par défaut, quelque soit les options d'affichage définies par l'admin
                - Cas 2 : si la fiche est en état : désactivé, prévisualisation, alors le catalogue apparait sous forme graphique (cas actuel)
                */
                bool bShowStepCatalog = fld.PopupDataRend == PopupDataRender.STEP && _bIsFileRenderer;
                if (bShowStepCatalog && RendererType == RENDERERTYPE.AdminFile)
                    bShowStepCatalog = GetAdminTableInfos(Pref, fieldRow.FldInfo.Table.DescId)?.EudonetXIrisBlackStatus != EUDONETX_IRIS_BLACK_STATUS.ENABLED;
                if (bShowStepCatalog)
                    sClassAction = "LNKSTEPCAT";
                else if (fieldRow.RightIsUpdatable)
                    sClassAction = "LNKADVCAT";


                sbClass.ConcatValue("readonly");
            }
            else if (iPopupDescId != 0
                && !((fieldRow.FldInfo.Table.EdnType == EdnType.FILE_MAIL || fieldRow.FldInfo.Table.EdnType == EdnType.FILE_SMS) && (fieldRow.FldInfo.Descid - fieldRow.FldInfo.Table.DescId) == (int)MailField.DESCID_MAIL_OBJECT))
            {
                // Autre type de catalogue sauf objet de mail : pour ne pas afficher la popup (pas iso - v7)
                if (popupTyp == PopupType.FREE)
                {
                    sClassAction = "LNKFREECAT";
                }
                else
                {
                    sClassAction = "LNKCAT";
                    sbClass.ConcatValue("readonly");
                }

                if (!fieldRow.RightIsUpdatable)
                    sClassAction = String.Empty;
            }
            // GCH - #36685 : [isoV7] mode liste sur PP :
            //Si champ 01 d'adresse affiché depuis une autre fiche que adresse et qu'aucunes fiches n'est affichée
            //  ne pas permettre la modification du champ
            //else if (
            //    fieldRow.FldInfo.Descid == (TableType.ADR.GetHashCode() + 1) && //Si champ 01 d'adresse
            //    fieldRow.FileId <= 0 && //pas de fiche adresse sélectionnée
            //    row.ViewTab != fieldRow.FldInfo.Table.DescId //affiché depuis une autre ttable qu'adresse HLA - #37878
            //    )
            //{
            //    sClassAction = String.Empty;
            //}
            else if (RendererType == RENDERERTYPE.AdminFile
                && fieldRow.FldInfo.Table.EdnType == EdnType.FILE_PLANNING
                && fieldRow.FldInfo.Descid % 100 == PlanningField.DESCID_CALENDAR_COLOR.GetHashCode())
            {
                #region Rubrique "Couleurs" de Planning en mode Admin
                sClassAction = "LNKPICKCOLOR";
                webControl.Style.Add("background-color", fieldRow.Value);
                #endregion
            }
            else if (fieldRow.FldInfo.Descid == PJField.LIBELLE.GetHashCode() && RendererType != RENDERERTYPE.EditFile)
            {
                eRecordPJ rowPj = (eRecordPJ)row;


                Boolean fromMailling = false;
                if (this.RendererType == RENDERERTYPE.PjList || this.RendererType == RENDERERTYPE.ListPjFromTpl)
                {
                    ePJToAdd a = ((ePjListRenderer)this).Attachment;
                    if (a != null)
                    {
                        fromMailling = a.ParentTab.TabType == TableType.CAMPAIGN || a.ParentTab.TabType == TableType.MAIL_TEMPLATE;
                    }
                }



                if (!fromMailling)
                {
                    SetSecuredPJLinkAttribute(rowPj, webControl);
                    sClassAction = "LNKOPENPJ";
                }


                else
                    sClassAction = "LNKSELPJ";

                webControl.Attributes.Add("eTyp", rowPj.PjType.GetHashCode().ToString());
                webControl.Attributes.Add("lnkdid", rowPj.PJTabDescID.ToString());
                webControl.Attributes.Add("lnkid", rowPj.PJFileID.ToString()); //ID de la fiche
                webControl.Attributes.Add("title", rowPj.ToolTip); //ID de la fiche
            }
            else if (fieldRow.RightIsUpdatable)
            {
                sClassAction = "LNKFREETEXT";
            }


            // Sécurité 
            //XSS Encode la display value                        
            //fieldRow.DisplayValue = HttpUtility.HtmlAttributeEncode(fieldRow.DisplayValue);

            #region Catalogue Etape
            if (sClassAction == "LNKSTEPCAT")
            {
                // Mode étape d'un catalogue
                webControl.Controls.Add(RenderStepCatalogFormat(row, fieldRow));
            }
            else
            {
                GetValueContentControl(ednWebControl, fieldRow.DisplayValue);
            }
            #endregion

            //  if (fieldRow.FldInfo.AutoCompletionEnabled)
            //      ednWebControl.WebCtrl.Attributes.Add("onkeyup", "oAutoCompletion.Search(event);");

            return true;
        }

        /// <summary>
        /// Fait un rendu du champ de type Binaire en Label si l email en consultation
        /// en bouton radios si l email/emailing en modification pour le champ IsHtml
        /// </summary>
        /// <param name="rowRecord">Ligne de la liste a afficher</param>
        /// <param name="fieldRecord">Le champ binaire</param>
        /// <param name="sClassAction">classe CSS choisi pour l'element</param>
        /// <returns>Retourne le control généré pour la rubrique de type BIT (retourne un eCheckBoxCtrl)</returns>
        protected virtual WebControl RenderBitFieldFormat(eRecord rowRecord, eFieldRecord fieldRecord, ref String sClassAction)
        {
            //N'affiche la case que si elle est liée à une fiche
            if (fieldRecord.FileId <= 0 && fieldRecord.FldInfo.Table.Alias != rowRecord.ViewTab.ToString())
                return null;

            /* ***************** TODOQUERY ******************
            'Si modification de la rubrique recharge la feuille pour mettre à jour les autorisations d'historisation ( FOLLOWUP)
            ' If nDescId = nHistoDescId And bFollowUpActive Then 
                ' strReload = "self.location.reload();"
            ' Else	
                ' strReload = ""
            ' End If
                ' +  DESACTIVATION DE LA CASE A COCHER SUIVANT LA REGLE DE FOLLOWUP
            ' ***************** TODOQUERY ******************/



            eCheckBoxCtrl _chkValue = new eCheckBoxCtrl(fieldRecord.DisplayValue == "1", !fieldRecord.RightIsUpdatable);
            // ajout de la checkbox
            if (fieldRecord.RightIsUpdatable)
                sClassAction = "LNKCHECK";

            _chkValue.Attributes.Add("ednt", "LNKCHECK");


            _chkValue.AddClick(String.Empty);
            if (_bIsFileRenderer)
            {
                Boolean bDoTT = _ePref.GetConfig(eLibConst.PREF_CONFIG.TOOLTIPTEXTENABLED) == "1";

                _chkValue.AddText(fieldRecord.FldInfo.Libelle, String.Concat(fieldRecord.FldInfo.Libelle, (bDoTT) ? String.Concat(Environment.NewLine, fieldRecord.FldInfo.ToolTipText) : ""));
                if (fieldRecord.FldInfo.StyleForeColor.Length > 0) { _chkValue.Style.Add(HtmlTextWriterStyle.Color, fieldRecord.FldInfo.StyleForeColor); }
                if (fieldRecord.FldInfo.StyleBold) { _chkValue.Style.Add(HtmlTextWriterStyle.FontWeight, "bold"); }
                if (fieldRecord.FldInfo.StyleItalic) { _chkValue.Style.Add(HtmlTextWriterStyle.FontStyle, "italic"); }
                if (fieldRecord.FldInfo.StyleUnderline) { _chkValue.Style.Add(HtmlTextWriterStyle.TextDecoration, "underline"); }
                if (fieldRecord.FldInfo.StyleFlat) { _chkValue.Style.Add(HtmlTextWriterStyle.BorderStyle, "thin"); }
            }

            return _chkValue;
        }

        /// <summary>
        /// Rendu d'un catalogue en mode Etape
        /// </summary>
        /// <param name="rowRecord"></param>
        /// <param name="fieldRecord"></param>
        /// <returns></returns>
        protected virtual WebControl RenderStepCatalogFormat(eRecord rowRecord, eFieldRecord fieldRecord)
        {
            HtmlGenericControl li, p;
            HyperLink a;
            String tooltip = String.Empty;
            Boolean bSelectedValue = false;
            Boolean bBeforeValue = (!String.IsNullOrEmpty(fieldRecord.Value)) ? true : false; // Si la valeur est avant la valeur sélectionnée
            Boolean bSequenceMode = fieldRecord.FldInfo.AliasSourceField?.SequenceMode ?? fieldRecord.FldInfo.SequenceMode;
            string sSelectedValueColor = fieldRecord.FldInfo.AliasSourceField?.SelectedValueColor ?? fieldRecord.FldInfo.SelectedValueColor;
            int descid = fieldRecord.FldInfo.Descid;

            Panel panel = new Panel();
            panel.Attributes.Add("eaction", "LNKSTEPCAT");
            panel.Attributes.Add("ename", eTools.GetFieldValueCellName(rowRecord, fieldRecord));
            panel.ID = eTools.GetFieldValueCellId(rowRecord, fieldRecord, 0);

            HtmlGenericControl ul = new HtmlGenericControl("ul");
            ul.Attributes.Add("class", "chevronCatalog");
            ul.Attributes.Add("seq", bSequenceMode ? "1" : "0");


            foreach (eCatalog.CatalogValue value in fieldRecord.CatalogValues)
            {
                bSelectedValue = value.DbValue == fieldRecord.Value;
                if (!value.IsDisabled || (value.IsDisabled && bSelectedValue))
                {
                    li = new HtmlGenericControl("li");
                    li.Attributes.Add("title", value.ToolTipText);
                    if (bBeforeValue && bSequenceMode)
                        li.Attributes.Add("class", "beforeSelectedValue");

                    HtmlGenericControl dvA = new HtmlGenericControl("div");
                    dvA.InnerText = value.DisplayValue;

                    a = new HyperLink();
                    a.Controls.Add(dvA);
                    a.NavigateUrl = "#";
                    a.Attributes.Add("class", "stepValue");
                    a.Attributes.Add("dbv", value.DbValue);
                    a.Attributes.Add("did", descid.ToString());
                    a.Attributes.Add("onclick", (!value.IsDisabled) ? "selectStep(this);" : "return false;");

                    Label lblStep = new Label();

                    if (!value.IsDisabled && bSelectedValue)
                    {
                        a.Style.Add(HtmlTextWriterStyle.BackgroundColor, sSelectedValueColor);
                        lblStep.Style.Add("border-left-color", sSelectedValueColor);
                    }

                    // Valeur sélectionnée
                    if (bSelectedValue)
                    {
                        li.Attributes.Add("class", "selectedValue " + ((value.IsDisabled) ? "disabledValue" : ""));
                        tooltip = value.ToolTipText;
                        bBeforeValue = false;
                    }

                    a.Controls.Add(lblStep);
                    li.Controls.Add(a);

                    ul.Controls.Add(li);
                }

            }
            panel.Controls.Add(ul);

            // Ajout du "Tooltip"
            // Pour les catalogues de ce type (étape) il a été spécifié que l'info-bulle de l'"étape" clickée soit affichée de façon 
            // permanente cf document de spécif (Spec Ergonomie 20170124 2017 R2 #8.docx), datant de fin 2017
            p = new HtmlGenericControl("p");
            p.Attributes.Add("class", "catalogTooltip");
            p.InnerText = tooltip;
            panel.Controls.Add(p);


            if (!String.IsNullOrEmpty(sSelectedValueColor))
            {
                string[] arInputName = { "stepFieldAfter", "stepField", "stepFieldAfter", "stepField" };
                int inc = 0;

                foreach (var sInputName in arInputName)
                {
                    _divHidden.Controls.Add(CreateHiddenInput(inc, rowRecord, fieldRecord, sSelectedValueColor, sInputName));
                    inc++;
                }
            }


            return panel;
        }

        /// <summary>
        /// Permet de créer un input hidden avec les iformations en base
        /// </summary>
        /// <param name="increment"></param>
        /// <param name="rowRecord"></param>
        /// <param name="fieldRecord"></param>
        /// <param name="sSelectedValueColor"></param>
        /// <param name="sInputName"></param>
        /// <returns></returns>
        HtmlInputHidden CreateHiddenInput(int increment, eRecord rowRecord, eFieldRecord fieldRecord, string sSelectedValueColor, string sInputName)
        {
            string sClassParentName = (increment > 1) ? "li.selectedValue" : "li.beforeSelectedValue";
            string sClassChildName = sInputName == "stepFieldAfter" ? "a:after" : "a";
            string eCSS = sInputName == "stepFieldAfter" ? "border-left-color:" : "background-color:";

            HtmlInputHidden inptRulesCss = new HtmlInputHidden();
            inptRulesCss = new HtmlInputHidden();
            inptRulesCss.ID = sInputName + fieldRecord.FldInfo.Descid;
            inptRulesCss.Attributes.Add("etype", "css");
            inptRulesCss.Attributes.Add("ecssname", $"LNKSTEPCAT[ename='{eTools.GetFieldValueCellName(rowRecord, fieldRecord)}'] {sClassParentName} {sClassChildName}");
            inptRulesCss.Attributes.Add("ecssclass", $"{eCSS} {sSelectedValueColor}");
            return inptRulesCss;
        }

        /// <summary>
        /// Rendu du type BOUTON
        /// </summary>
        /// <param name="rowRecord"></param>
        /// <param name="fieldRecord"></param>
        /// <returns></returns>
        protected virtual WebControl RenderBitButtonFieldFormat(eRecord rowRecord, eFieldRecord fieldRecord)
        {
            String btnCssClass = String.Empty;

            Boolean bDoTT = _ePref.GetConfig(eLibConst.PREF_CONFIG.TOOLTIPTEXTENABLED) == "1";

            //Panel wrapper = new Panel();
            //wrapper.CssClass = "btnBitField";

            HyperLink button = new HyperLink();
            button.Text = fieldRecord.FldInfo.Libelle;
            button.NavigateUrl = "#";
            button.CssClass = "btnBitField";

            #region Attributs 
            button.Attributes.Add("dis", !fieldRecord.RightIsUpdatable ? "1" : "0");
            button.Attributes.Add("chk", (fieldRecord.DisplayValue == "1") ? "1" : "0");
            button.Attributes.Add("onclick", (fieldRecord.RightIsUpdatable) ? "changeBtnBit(this);" : "return false;");
            #endregion

            //if (_bIsFileRenderer)
            //{
            // Infobulle
            if (bDoTT)
                button.ToolTip = fieldRecord.FldInfo.ToolTipText;

            // Style
            if (fieldRecord.FldInfo.StyleForeColor.Length > 0) { button.Style.Add(HtmlTextWriterStyle.Color, fieldRecord.FldInfo.StyleForeColor); }
            if (fieldRecord.FldInfo.ButtonColor.Length > 0) { button.Style.Add(HtmlTextWriterStyle.BackgroundColor, fieldRecord.FldInfo.ButtonColor); }
            if (fieldRecord.FldInfo.StyleBold) { button.Style.Add(HtmlTextWriterStyle.FontWeight, "bold"); }
            if (fieldRecord.FldInfo.StyleItalic) { button.Style.Add(HtmlTextWriterStyle.FontStyle, "italic"); }
            if (fieldRecord.FldInfo.StyleUnderline) { button.Style.Add(HtmlTextWriterStyle.TextDecoration, "underline"); }
            //}


            //wrapper.Controls.Add(button);

            return button;
        }

        /// <summary>
        /// Ajoute les actions affichant la vcard au control passé en parametre
        /// </summary>
        /// <param name="webControl"></param>
        public virtual void VCMouseActionAttributes(WebControl webControl)
        {
            webControl.Attributes.Add("onmouseover", String.Concat("shvc(this, 1)"));
            webControl.Attributes.Add("onmouseout", String.Concat("shvc(this, 0)"));
        }

        /// <summary>
        /// Ajoute les actions affichant la minifiche au control passé en parametre
        /// </summary>
        /// <param name="webControl"></param>
        public virtual void VCMiniFileMouseActionAttributes(WebControl webControl, int tab)
        {
            webControl.Attributes.Add("vcMiniFileTab", tab.ToString());
            webControl.Attributes.Add("onmouseover", String.Concat("shvc(this, 1)"));
            webControl.Attributes.Add("onmouseout", String.Concat("shvc(this, 0)"));
        }

        /// <summary>
        /// Dictionnaire pour stocker les mappings des minifiches activés
        /// </summary>
        protected Dictionary<int, bool> _dicoMiniFileMappingEnabled = new Dictionary<int, bool>();

        /// <summary>
        /// Vérifie que le mapping de miniFiche est configuré pour une table, et stocke la valeur dans un dictionnaire
        /// </summary>
        /// <param name="tab">Descid de la table à tester</param>
        protected bool VCMiniFileCheckMappingEnabled(int tab)
        {
            if (!_dicoMiniFileMappingEnabled.ContainsKey(tab))
            {
                string error;
                _dicoMiniFileMappingEnabled.Add(tab, eFilemapPartner.IsMiniFileMappingEnabled(Pref, tab, out error));
            }

            bool mappingEnabled = false;
            _dicoMiniFileMappingEnabled.TryGetValue(tab, out mappingEnabled);

            return mappingEnabled;
        }

        /// <summary>
        /// Action spécifique au Site web
        /// </summary>
        /// <param name="fieldRow">Information du champ courant</param>
        /// <param name="ednWebControl">Controle qui va être ajoute</param>
        /// <returns>ClassA utiliser</returns>
        public virtual String GetFieldValueCell_TYP_WEB(EdnWebControl ednWebControl, eFieldRecord fieldRow)
        {
            String sClassAction = string.Empty;
            if (!String.IsNullOrEmpty(fieldRow.DisplayValue))
                sClassAction = "LNKOPENWEB";
            else if (fieldRow.RightIsUpdatable)
                sClassAction = "LNKWEBSIT";

            GetValueContentControl(ednWebControl, fieldRow.DisplayValue);
            return sClassAction;
        }

        /// <summary>
        /// Action spécifique au Site web
        /// </summary>
        /// <param name="fieldRow">Information du champ courant</param>
        /// <param name="ednWebControl">Controle qui va être ajoute</param>
        /// <returns>ClassA utiliser</returns>
        public virtual String GetFieldValueCell_TYP_SOCIALNETWORK(EdnWebControl ednWebControl, eFieldRecord fieldRow)
        {
            String sClassAction = string.Empty;
            if (!String.IsNullOrEmpty(fieldRow.DisplayValue))
                sClassAction = "LNKOPENSOCNET";
            else if (fieldRow.RightIsUpdatable)
                sClassAction = "LNKSOCNET";

            GetValueContentControl(ednWebControl, fieldRow.DisplayValue);
            return sClassAction;
        }

        /// <summary>
        /// Ajoute la cellule contenant le bouton
        /// dans le cas du mode consultation, la cellule est vide
        /// et idem dans le cas ou le champ n'est pas modifiable
        /// </summary>
        /// <param name="tcButton">The tc button.</param>
        /// <param name="cellValue">The cell value.</param>
        /// <param name="bDisplayBtn">if set to <c>true</c> [display BTN].</param>
        /// <param name="sIcon">The icon.</param>
        /// <param name="sIconColor">Color of the icon.</param>
        protected void GetBaseButtonCell(WebControl tcButton, Control cellValue, Boolean bDisplayBtn, String sIcon = "", String sIconColor = "")
        {
            tcButton.CssClass = "btn";

            if (!bDisplayBtn)
                return;

            string sAction = String.Empty;
            string sActionBtn = String.Empty;
            string sIconButton = String.Empty;
            int iRes = 0;
            bool bApplyColor = false;

            if (cellValue != null)
            {
                if (cellValue.GetType() == typeof(TextBox))
                {
                    TextBox txBoxValue = (TextBox)cellValue;
                    sAction = txBoxValue.Attributes["eaction"];
                    tcButton.Attributes.Add("eActTg", txBoxValue.ID);


                    switch (sAction)
                    {
                        // blocage de l'input pour les champs ne permettant pas la saisie libre

                        case "LNKFREECAT":
                            sAction = "LNKFREECAT";
                            sIconButton = "icon-catalog";
                            //txBoxValue.Attributes.Remove("eaction");
                            break;
                        case "LNKDATE":
                            txBoxValue.Attributes["eaction"] = "LNKFREETEXT";
                            sIconButton = "icon-agenda";
                            break;
                        case "LNKCAT":
                        case "LNKADVCAT":
                        case "LNKCATUSER":
                        case "LNKCATDESC":
                        case "LNKCATENUM":
                            txBoxValue.ReadOnly = true;
                            txBoxValue.ToolTip = txBoxValue.Text;
                            sIconButton = "icon-catalog";
                            break;
                        case "LNKWEBSIT":
                            sAction = "LNKOPENWEB";
                            sIconButton = "icon-site_web";
                            iRes = 2742;
                            break;
                        case "LNKOPENWEBSPECIF":
                            sAction = "LNKOPENWEBSPECIF";
                            bool isReadOnly = txBoxValue.Attributes != null && txBoxValue.Attributes["ero"] != null && txBoxValue.Attributes["ero"] == "1";

                            //Si champ modifiable, le click edit le lien
                            if (!isReadOnly)
                                txBoxValue.Attributes["eaction"] = "LNKWEBSIT";

                            sIconButton = "icon-site_web";
                            break;
                        case "LNKMNGFILE":
                        case "LNKOPENFILE":
                            txBoxValue.ReadOnly = true;
                            sAction = "LNKMNGFILE";
                            sIconButton = "icon-catalog";
                            break;
                        case "LNKCATFILE":
                        case "LNKGOFILE":
                            // HLA - LNKCATFILE deplacé pour obtenir une Icone cranté pour toutes les liaisons même si pas de rattachement
                            txBoxValue.ReadOnly = true;
                            sAction = "LNKCATFILE";
                            sIconButton = "icon-hyperlink";
                            break;
                        case "LNKMAIL":
                        case "LNKOPENMAIL":
                        case "LNKMAILRO":
                            sAction = "LNKSENDMAIL";
                            sIconButton = "icon-email";
                            break;
                        // TODO SMS
                        case "LNKPHONE":
                        case "LNKPHONERO":
                        case "LNKOPENPHONE":
                            if (_ePref.SmsEnabled)
                            {
                                sAction = "LNKSENDPHONE";
                                sIconButton = "icon-sms";
                            }
                            break;
                        case "LNKPICKCOLOR":
                            sIconButton = "icon-paint-brush";
                            break;
                        case "LNKSOCNET":
                            sAction = "LNKOPENSOCNET";
                            bApplyColor = true;
                            if (!String.IsNullOrEmpty(sIcon))
                                sIconButton = eFontIcons.GetFontClassName(sIcon);
                            else
                                sIconButton = "icon-site_web";
                            sIconButton = String.Concat(sIconButton, " icnFileBtnSocNet");
                            if (sIcon == "facebook-square")
                                iRes = 2743;
                            else if (sIcon == "skype2")
                                iRes = 2744;

                            if (String.IsNullOrEmpty(txBoxValue.Text))
                                tcButton.Style.Add("opacity", "0");

                            break;
                        case "LNKGEO":
                            sIconButton = "icon-map-marker";
                            break;
                        default:
                            break;
                    }
                }
                // Cas du champ Mémo : ajout d'un bouton spécifique à droite pour afficher le champ dans une nouvelle fenêtre
                // On ajoute le contrôle sous forme d'image et un attribut contenant l'ID du champ contrôlé par ce bouton
                // pour le bon fonctionnement de fldLClick dans eMain.js
                else if (cellValue.GetType() == typeof(HtmlInputHidden) ||
                    (cellValue.GetType() == typeof(Panel) && cellValue.HasControls() && cellValue.Controls[0].GetType() == typeof(HtmlInputHidden)))
                {
                    if (cellValue.GetType() == typeof(Panel) && cellValue.HasControls() && cellValue.Controls[0].GetType() == typeof(HtmlInputHidden))
                        cellValue = cellValue.Controls[0];
                    sAction = "LNKOPENMEMOPOPUP";
                    tcButton.Attributes.Add("ctrl", cellValue.Parent.ID);
                    HtmlImage img = new HtmlImage();
                    img.Src = eConst.GHOST_IMG;
                    img.Attributes.Add("class", "LNKOPENMEMOPOPUPbtn");
                    tcButton.Controls.Add(img);
                    sIconButton = "icon-edn-pen icnMemo";
                }
                // Cas du champs MULTI_OWNER de planning
                else if (cellValue.GetType() == typeof(HtmlGenericControl))
                {
                    HtmlGenericControl divValue = (HtmlGenericControl)cellValue;
                    sAction = divValue.Attributes["eaction"];
                    tcButton.Attributes.Add("eActTg", divValue.ID);
                    sIconButton = "icon-catalog";
                }
                else if (cellValue.GetType() == typeof(Panel))
                {
                    Panel panel = (Panel)cellValue;
                    if (String.IsNullOrEmpty(panel.Attributes["data-eEmptyPictureArea"]) || panel.Attributes["data-eEmptyPictureArea"] != "1")
                    {
                        sAction = panel.Attributes["eaction"];
                        if (sAction == "LNKSTEPCAT")
                            sAction = "LNKADVCAT";
                        tcButton.Attributes.Add("eActTg", panel.ID);
                        sIconButton = "icon-catalog";
                    }
                }
            }

            if (sAction == "LNKFREECAT")
                sActionBtn = "LNKCAT";
            else
                sActionBtn = sAction;

            tcButton.CssClass = String.Concat(sIconButton, " icnFileBtn"); // String.Concat(sActionBtn, "btn btn");

            if (bApplyColor && !String.IsNullOrEmpty(sIconColor))
                tcButton.Style.Add("color", sIconColor);

            tcButton.Attributes.Add("eaction", sAction);
            tcButton.Attributes.Add("efld", "1");

            if (iRes > 0)
                tcButton.Attributes.Add("eRes", eResApp.GetRes(_ePref.LangId, iRes));

        }
        /// <summary>
        /// Ajoute la cellule contenant le bouton
        /// dans le cas du mode consultation, la cellule est vide
        /// et idem dans le cas ou le champ n'est pas modifiable
        /// </summary>
        /// <param name="cellValue">The cell value.</param>
        /// <param name="bDisplayBtn">if set to <c>true</c> [display BTN].</param>
        /// <param name="sIcon">The icon.</param>
        /// <param name="sIconColor">Color of the icon.</param>
        /// <param name="field">Objet field pour information complémentaire</param>
        /// <returns></returns>
        public virtual TableCell GetButtonCell(TableCell cellValue, Boolean bDisplayBtn, String sIcon = "", String sIconColor = "", Field field = null)
        {
            TableCell tcButton = new TableCell();
            GetBaseButtonCell(tcButton, cellValue.HasControls() ? (Control)cellValue.Controls[0] : null, bDisplayBtn, sIcon, sIconColor);
            ((TableCell)tcButton).RowSpan = cellValue.RowSpan;
            return tcButton;
        }

        /// <summary>
        /// remplit le web control avec le contenu souhaité
        /// </summary>
        /// <param name="ednWebCtrl"></param>
        /// <param name="sValue"></param>
        protected virtual void GetValueContentControl(EdnWebControl ednWebCtrl, String sValue)
        {
            // Ajout du nbsp pour l'affichage de cellule vide sur IE7 (problème de border)
            String emptyValue = "&nbsp;";

            if (ednWebCtrl.TypCtrl == EdnWebControl.WebControlType.TEXTBOX)
            {
                //pas d'encodage dans le cas de l'affichage de la valeur dans un textBox
                //webCtrl.Attributes.Add("value", String.IsNullOrEmpty(sValue) ? string.Empty : HttpUtility.HtmlAttributeEncode(sValue));

                TextBox tb = (TextBox)ednWebCtrl.WebCtrl;
                tb.Text = String.IsNullOrEmpty(sValue) ? String.Empty : sValue;
            }
            else
            {
                Control contentControl = new LiteralControl();
                // On utilise HtmlEncode pour la sécurisation des valeurs

                ((LiteralControl)contentControl).Text = String.IsNullOrEmpty(sValue) ? emptyValue : HttpUtility.HtmlEncode(sValue);
                if (ednWebCtrl.WebCtrl.Controls.Count > 0)
                    ednWebCtrl.WebCtrl.Controls.Clear();   //On clear dans le cas ou il y a déjà du contenu afin de ne pas en avoir en double. (GCH : exemple si plusieurs champs contenu dans le même champ)
                ednWebCtrl.WebCtrl.Controls.Add(contentControl);
            }
        }
        /// <summary>
        /// remplit le web control avec le contenu souhaité
        /// </summary>
        /// <param name="ednWebCtrl"></param>
        protected virtual String GetCurrentValueContentControl(EdnWebControl ednWebCtrl)
        {
            if (ednWebCtrl.TypCtrl == EdnWebControl.WebControlType.TEXTBOX)
            {
                //pas d'encodage dans le cas de l'affichage de la valeur dans un textBox
                //webCtrl.Attributes.Add("valtue", String.IsNullOrEmpty(sValue) ? string.Empty : HttpUtility.HtmlAttributeEncode(sValue));

                TextBox tb = ednWebCtrl.WebCtrl as TextBox;
                if (tb == null)
                    return String.Empty;

                return tb.Text;
            }
            else
            {
                LiteralControl lc = ednWebCtrl.WebCtrl.Controls[0] as LiteralControl;
                if (lc == null)
                    return String.Empty;

                return lc.Text;
            }
        }

        /// <summary>
        /// retourne le mainContainer sous forme d'un stringbuilder contenant son code html
        /// Utilisé pour pouvoir ajouter certains control (radiobutton, checkbox, textbox) sans
        /// qui nécessite un control form runat server avec viewstate forcé
        /// 
        /// </summary>
        /// <returns></returns>
        public StringBuilder RenderMainPanel()
        {
            return new StringBuilder(RenderWebControl(PgContainer));

        }





        /// <summary>
        /// retourne le mainContainer sous forme d'un stringbuilder contenant son code html
        /// Utilisé pour pouvoir ajouter certains control (radiobutton, checkbox, textbox) sans
        /// qui nécessite un control form runat server avec viewstate forcé
        /// 
        /// </summary>
        /// <returns></returns>
        public static String RenderWebControl(WebControl wcMyControl)
        {
            StringBuilder sb = new StringBuilder();

            //le Decorator de Design Pattern
            wcMyControl.RenderControl(new HtmlTextWriter(new StringWriter(sb)));
            return sb.ToString();
        }

        /// <summary>
        /// retourne le mainContainer sous forme d'un stringbuilder contenant son code html
        /// Utilisé pour pouvoir ajouter certains control (radiobutton, checkbox, textbox) sans
        /// qui nécessite un control form runat server avec viewstate forcé
        /// 
        /// </summary>
        /// <returns></returns>
        public static String RenderWebControl(HtmlControl wcMyControl)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter tw = new HtmlTextWriter(sw);

            wcMyControl.RenderControl(tw);
            return sb.ToString();
        }

        /// <summary>
        /// retourne le mainContainer sous forme d'un stringbuilder contenant son code html
        /// Utilisé pour pouvoir ajouter certains control (radiobutton, checkbox, textbox) sans
        /// qui nécessite un control form runat server avec viewstate forcé
        /// Avec un control generic
        /// </summary>
        /// <returns></returns>

        public static String RenderControl(Control wcMyControl)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter tw = new HtmlTextWriter(sw);

            wcMyControl.RenderControl(tw);
            return sb.ToString();
        }


        /// <summary>
        /// remplit le web control avec le contenu souhaité
        /// </summary>
        /// <param name="ednWebCtrl"></param>
        /// <param name="sValue"></param>
        protected virtual void GetHTMLMemoControl(EdnWebControl ednWebCtrl, String sValue)
        {
            WebControl webCtrl = ednWebCtrl.WebCtrl;

            HtmlInputHidden memoValue = new HtmlInputHidden();
            memoValue.ID = String.Concat(webCtrl.ID, "hid");
            memoValue.Value = sValue;
            webCtrl.Controls.Add(memoValue);

            HtmlGenericControl iFrame = new HtmlGenericControl("iFrame");

            iFrame.Attributes.Add("class", "eME");
            iFrame.Attributes.Add("src", "blank.htm");
            iFrame.ID = String.Concat(webCtrl.ID, "ifr");
            iFrame.Style.Add("width", "99%"); // largeur légèrement inférieure à 100% pour éviter que l'iframe dépasse de la cellule et afficher complètement la bordure verte après édition
            iFrame.Style.Add("height", "100%");
            iFrame.Attributes.Add("src", "about:blank");
            webCtrl.Controls.Add(iFrame);
        }

        /// <summary>
        /// remplit le web control avec le contenu souhaité
        /// </summary>
        /// <param name="ednWebCtrl"></param>
        /// <param name="sValue"></param>
        protected virtual void GetRawMemoControl(EdnWebControl ednWebCtrl, String sValue)
        {
            // En mode Planning en Popup, les champs Mémo mode Texte doivent être affichés directement
            if (this.RendererType == RENDERERTYPE.PlanningFile)
            {
                GetHTMLMemoControl(ednWebCtrl, HttpUtility.HtmlDecode(sValue));
            }
            // Sur les autres modes sur lesquels GetRawMemoControl n'est pas surchargé (Liste notamment), on affiche directement le contenu Texte du champ
            // Il faudra alors cliquer sur le contenu du champ pour ouvrir une fenêtre popup permettant de l'éditer
            else
            {
                GetValueContentControl(ednWebCtrl, HttpUtility.HtmlDecode(sValue));
            }
        }

        /* Méthode abstraite à implémenter */

        /// <summary>
        /// Appel l'objet métier
        ///  eList/eFiche (l'appel a EudoQuery est fait dans cet appel ainsi que l'appel et le parcours du dataset)
        /// </summary>
        /// <returns></returns>
        protected virtual Boolean Init() { return true; }

        /// <summary>
        /// Construit le html de l'objet demandé
        /// </summary>
        /// <returns></returns>
        protected virtual Boolean Build() { return true; }

        /// <summary>
        /// Construit des objets html annexes/place des appel JS d'apres chargement
        /// </summary>
        /// <returns></returns>
        protected virtual Boolean End() { return true; }

        /// <summary>
        /// Icone Pochoir conditionnel
        /// </summary>
        /// <param name="row">ligne d'enregistrement contenant la règle</param>
        /// <param name="idxLine">index de ligne dans la liste</param>
        /// <param name="sLstRulesCss">liste des css de règles conditionnel déjà réalisé (sera complété avec celui ajouté par la méthode)</param>
        /// <param name="sRulesId">Id de la règle</param>
        protected void AddPochoirInputCss(eRecord row, int idxLine, ref string sLstRulesCss, string sRulesId)
        {
            string sRulesCss = string.Empty;

            sRulesCss = string.Concat(sRulesCss, "background:");

            if (!string.IsNullOrEmpty(row.RuleColor.BgColor))
                sRulesCss = string.Concat(sRulesCss, row.RuleColor.BgColor);

            if (!string.IsNullOrEmpty(row.RuleColor.Icon))
            {
                // Pas de prise en compte du theme ici, on utilise donc ThemePaths
                string imagePathToSkin = string.Concat("/images/iconPochoirTMP/", ((idxLine % 2) == 0 ? "contact2.png" : "contact1.png"));
                string sIconColor = string.Concat("themes/", Pref.ThemePaths.GetImageWebPath(imagePathToSkin));

                sRulesCss = string.Concat(sRulesCss, " url(", sIconColor, ") no-repeat");
            }

            sRulesCss = string.Concat(sRulesCss, "  !important ;", "background-position: top left !important;");

            //Création du css histo
            sLstRulesCss = string.Concat(sLstRulesCss, ";", sRulesId, ";");
            HtmlInputHidden inptRulesCss = new HtmlInputHidden();
            inptRulesCss.ID = sRulesId;
            inptRulesCss.Attributes.Add("etype", "css");
            inptRulesCss.Attributes.Add("ecssname", sRulesId);
            inptRulesCss.Attributes.Add("ecssclass", sRulesCss);
            _divHidden.Controls.Add(inptRulesCss);
        }

        /// <summary>
        /// Ajoute l'attribut "srcPJ" au webControl pour l'URL sécurisée de l'annexe
        /// </summary>
        /// <param name="rowPj">The row pj.</param>
        /// <param name="webControl">The web control.</param>
        /// <returns></returns>
        protected void SetSecuredPJLinkAttribute(eRecordPJ rowPj, WebControl webControl)
        {
            webControl.Attributes.Add("srcPJ", rowPj.SecureLink);


        }

        /// <summary>
        /// retrouve un sous-control par son id 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        protected Control FindControlRecursive(Control root, string id)
        {
            if (root.ID == id)
                return root;

            foreach (Control c in root.Controls)
            {
                Control t = FindControlRecursive(c, id);
                if (t != null) return t;
            }
            return null;
        }

        /// <summary>
        /// Récupère les valeurs du catalogue Interaction.Type de média
        /// </summary>
        /// <param name="tabId">descid de la table</param>
        /// <param name="fieldId">descid de la rubrique</param>
        /// <returns></returns>
        protected List<eCatalog.CatalogValue> GetCatalogValues(int tabId, int fieldId)
        {
            List<eCatalog.CatalogValue> _catalogValues = new List<eCatalog.CatalogValue>();

            eudoDAL dal = eLibTools.GetEudoDAL(Pref);
            try
            {
                dal.OpenDatabase();

                FieldLite field = null;

                IEnumerable<FieldLite> listFields = RetrieveFields.GetEmpty(null)
                                                .AddOnlyThisTabs(new int[] { tabId })
                                                .AddOnlyThisDescIds(new int[] { fieldId })
                                                .SetExternalDal(dal)
                                                .ResultFieldsInfo(FieldLite.Factory());

                if (listFields.Any())
                    field = listFields.First();

                if (field == null)
                    throw new Exception(string.Format("GetCatalogValues error : impossible de récupérer les infos de la rubrique {0}", fieldId));

                eCatalog catalog = new eCatalog(dal, Pref, field.Popup, Pref.User, field.PopupDescId);
                _catalogValues = catalog.Values;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dal.CloseDatabase();
            }

            return _catalogValues;
        }

        /// <summary>
        /// Get catalogue value for campaign of type Email
        /// </summary>
        /// <param name="tabId"></param>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        protected List<eCatalog.CatalogValue> GetEmailCatalgueValues(int tabId, int fieldId)
        {
            List<eCatalog.CatalogValue> _catalogValues = new List<eCatalog.CatalogValue>();
            List<eCatalog.CatalogValue> lsCatalogMediaType = GetCatalogValues(tabId, fieldId);

            _catalogValues = lsCatalogMediaType.Where(c => c.Data.ToLower() == "email").ToList();

            return _catalogValues;
        }

        /// <summary>
        /// Récupère les valeurs du catalogue Interaction.Type de campagne
        /// </summary>
        /// <returns></returns>
        protected List<eCatalog.CatalogValue> GetCampaignTypeCatalogValues(int parentId)
        {
            List<eCatalog.CatalogValue> catalogValues = new List<eCatalog.CatalogValue>();

            eudoDAL dal = eLibTools.GetEudoDAL(Pref);
            try
            {
                dal.OpenDatabase();

                FieldLite typeCampaignField = null;

                IEnumerable<FieldLite> listFields = RetrieveFields.GetEmpty(null)
                                                .AddOnlyThisTabs(new int[] { (int)TableType.INTERACTION })
                                                .AddOnlyThisDescIds(new int[] { (int)InteractionField.TypeCampaign })
                                                .SetExternalDal(dal)
                                                .ResultFieldsInfo(FieldLite.Factory());

                if (listFields.Any())
                    typeCampaignField = listFields.First();

                if (typeCampaignField == null)
                    throw new Exception("GetCampaignTypeCatalogValues error : impossible de récupérer les infos de la rubrique Type de Campagne");

                eCatalog catalog = new eCatalog(dal, Pref, typeCampaignField.Popup, Pref.User, typeCampaignField.PopupDescId);
                catalogValues = catalog.Values.Where(cv => cv.ParentId == parentId).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dal.CloseDatabase();
            }

            return catalogValues;
        }

        /// <summary>
        /// Retourne les champs de fusion et les champs de tracking
        /// TODO: utiliser cette méthode pour les méthodes dans les renderers actuels
        /// pour éviter la duplication du code
        /// </summary>
        /// <param name="pref">Preference utilisateur</param>
        /// <param name="nTabFrom"> table d'ou on vient</param>
        /// <param name="bGetOnlyTxtMergedField"> charger uniquement les champs de fusion pour l'objet du mail</param>
        /// <returns> variables javascript champs de fusion et track fields</returns>
        protected String GetMergeAndTrackFields(ePref pref, int nTabFrom, bool bGetOnlyTxtMergedField = false)
        {
            StringBuilder strScriptBuilder = new StringBuilder();

            #region Champs de fusion
            string strJavaScript = String.Empty;
            string strWebsiteFieldsJavaScript = String.Empty;
            string error = String.Empty;

            eTableLiteMailing table;
            IEnumerable<eFieldLiteWithLib> fields = null;

            eudoDAL dal = eLibTools.GetEudoDAL(pref);

            try
            {
                dal.OpenDatabase();

                if (nTabFrom == 0)
                    throw new Exception("Invalide nTabFrom = " + nTabFrom);

                table = new eTableLiteMailing(nTabFrom, pref.Lang);
                table.ExternalLoadInfo(dal, out error);
                if (error.Length > 0)
                    throw new Exception(error);

                //Tous les champs de fusion
                List<int> AllMergeFields = eLibTools.GetMergeFieldsMailingList(dal, pref, table, table.ProspectEnabled, bGetOnlyTxtMergedField: bGetOnlyTxtMergedField);

                //Tous les champs de fusion de type site web
                List<int> AllWebsiteMergeFields = eLibTools.GetSpecificMergeFieldsMailingList(dal, pref, table, table.ProspectEnabled, false, false, FieldFormat.TYP_WEB);

                //On filtre la  liste par rapport aux droits de visu
                List<int> AllowedMergeFields = new List<int>(eLibTools.GetAllowedFieldsFromDescIds(pref, pref.User, String.Join(";", AllMergeFields.ToArray()), false).Keys);

                //On filtre la liste des champs de type site web
                List<int> WebsiteMergeFields = new List<int>(eLibTools.GetAllowedFieldsFromDescIds(pref, pref.User, String.Join(";", AllWebsiteMergeFields.ToArray()), false).Keys);

                //on construit la liste des champs
                eLibTools.GetMergeFieldsData(dal, pref, pref.User, AllowedMergeFields, null, null, null, null, null, null, out strJavaScript);

                //on construit la liste des champs de type site web
                eLibTools.GetMergeFieldsData(dal, pref, pref.User, WebsiteMergeFields, null, null, null, null, null, null, out strWebsiteFieldsJavaScript);

                fields = Internal.RetrieveFields.GetEmpty(pref)
                    .AddOnlyThisTabs(new int[] { nTabFrom })
                    .AddOnlyThisFormats(new FieldFormat[] { FieldFormat.TYP_BIT, FieldFormat.TYP_NUMERIC })
                    .AddExcludeSystemFields(true)
                    .SetExternalDal(dal)
                    .ResultFieldsInfo(eFieldLiteWithLib.Factory(pref));
            }
            catch (Exception ex)
            {
                throw new Exception("eEditMailingRenderer::GetMergeAndTrackFields:", ex);
            }
            finally
            {
                dal.CloseDatabase();
            }

            strScriptBuilder.Append(" var mailMergeFields = ").Append(String.IsNullOrEmpty(strJavaScript) ? "{}" : strJavaScript).Append(";").AppendLine();

            #endregion

            #region Champs tracking
            //pour l'objet de mail, pas besoin de charger la liste des champs tracking
            if (!bGetOnlyTxtMergedField)
            {
                strScriptBuilder.AppendLine()
                .Append(" var oTrackFields = { link :{href:'', ednc:'lnk', ednt:'on', ednd:'0', ednn:'', ednl:'0', title:'', target:'_blank'}, fields : [ ")
                .Append("['<").Append(eResApp.GetRes(pref, 141)).Append(">', '0', ''], ");     // option vide

                if (fields != null && fields.Count() > 0)
                {
                    string prefName = HttpUtility.JavaScriptStringEncode(table.Libelle);

                    //insère des options au format suivant : [libellé, id, format]
                    strScriptBuilder
                        .Append(eLibTools.Join(",", fields, delegate (eFieldLiteWithLib fld)
                        {
                            return new StringBuilder()
                                .Append("['").Append(prefName).Append(".").Append(HttpUtility.JavaScriptStringEncode(fld.Libelle))
                                .Append("', '").Append(fld.Descid).Append("', '").Append(fld.Format.GetHashCode()).Append("']").ToString();
                        }));
                }

                strScriptBuilder.Append(" ]}; ");
            }
            #endregion

            #region Champs de fusion site web
            if (!bGetOnlyTxtMergedField)
            {
                strScriptBuilder.AppendLine()
                .Append(" var oMergeHyperLinkFields = { link :{href:'', ednc:'lnk', ednt:'on', ednd:'0', ednn:'', ednl:'0', title:'', target:'_blank'}, fields : [ ")
                .Append("['<").Append(eResApp.GetRes(pref, 141)).Append(">', '0', '']");     // option vide

                if (String.IsNullOrEmpty(strWebsiteFieldsJavaScript))
                    strScriptBuilder.Append(", []");
                else
                {
                    string[] websiteFields = strWebsiteFieldsJavaScript.Split(',');
                    foreach (String wbF in websiteFields)
                    {
                        string[] strlist1 = wbF.Split(':');
                        string fldLibelle = Regex.Replace(strlist1[0], "(\\r|\\n|\\|\u0022|{)*", String.Empty);
                        string[] strlist2 = strlist1[1].Split(';');
                        string fldDescId = String.Concat("'", strlist2[0].Replace("\"", String.Empty), "'").Replace(" ", String.Empty);
                        string fldFormat = String.Concat("'", strlist2[3], "'").Replace(" ", String.Empty);
                        string wsField = String.Concat(", [", HttpUtility.HtmlDecode(fldLibelle), ",", fldDescId, ",", fldFormat, "]");
                        strScriptBuilder.Append(wsField);
                    }
                }

                strScriptBuilder.Append(" ]}; ");
            }

            #endregion

            return strScriptBuilder.ToString();
        }

        /// <summary>
        /// Renvoie les informations étendues d'une table (eAdminTableInfos) pour l'admin.
        /// Si l'information n'a pas encore été récupérée, elle est stockée en cache pour une réuilisation ultérieure
        /// Si elle a déjà été demandée pour la table en question, on utilise l'information issue du cache pour éviter de rappeler la base de données
        /// </summary>
        /// <param name="pref">Objet Pref pour l'accès en base de données</param>
        /// <param name="nTabId">TabID (DescID) de la table pour laquelle on souhaite récupérer les informations</param>
        /// <returns>Les eAdminTableInfos si récupérées, ou null sinon</returns>
        protected eAdminTableInfos GetAdminTableInfos(ePref pref, int nTabId)
        {
            if (!_adminTableInfos.ContainsKey(nTabId))
                _adminTableInfos.Add(nTabId, new eAdminTableInfos(pref, nTabId));

            if (_adminTableInfos.ContainsKey(nTabId))
                return _adminTableInfos[nTabId];
            else
                return null;
        }

    }




    /// <summary>
    /// "Extenusion" d'un webcontrol avec ajout d'information spécifique Eudo
    /// </summary>
    public class EdnWebControl
    {
        /// <summary>
        /// Enum du type de controle (au sens eudo)
        /// </summary>
        public enum WebControlType
        {

            /// <summary>Rendu de type texbox</summary>
            TEXTBOX,

            /// <summary>Rendu de type div</summary>
            PANEL,

            /// <summary>Rendu pour les libellé</summary>
            LABEL,

            /// <summary>Rendu dans une cellule de tableau</summary>
            TABLE_CELL,

            /// <summary>Rendu du type password</summary>
            PASSWORD_CELL
        }

        /// <summary>
        /// Control HTML
        /// </summary>
        public WebControl WebCtrl { get; set; }


        /// <summary>
        /// type de controle (au sens eudo)
        /// </summary>
        public WebControlType TypCtrl
        {
            get; set;
        }

        /// <summary>
        /// Control HTML complémentaire du WebCtrl
        /// </summary>
        public HtmlGenericControl AdditionalWebCtrl { get; set; }
    }


    /// <summary>
    /// Liste contenant la taille max des libellé par colonne
    /// </summary>
    public class ListColMaxValues
    {
        private String _colMaxValue = String.Empty;
        private String _headValue = String.Empty;
        private String _additionalCss = String.Empty;

        /// <summary>
        /// Valeur de la colonne la plus longue
        /// </summary>
        public String ColMaxValue
        {
            get { return _colMaxValue; }
            set { _colMaxValue = value; }
        }

        /// <summary>
        /// Valeur de l'en-tête de la colonne
        /// </summary>
        public String HeadValue
        {
            get { return _headValue; }
        }


        /// <summary>
        /// Css additionnelles sur les cellules de gestion de tailles
        /// </summary>
        public String AdditionalCss
        {
            get { return _additionalCss; }
            set { _additionalCss = value; }
        }

        /// <summary>
        /// Valeur la plus grande du libelle pour la colonne
        /// </summary>
        /// <param name="headValue"></param>
        public ListColMaxValues(String headValue)
        {
            _headValue = headValue;
            _colMaxValue = _headValue;
        }

        /// <summary>
        /// Retourne la taille maximal de la colonne (max libelle/contenu
        /// dans la font spécifié (defaut : verdana 12)
        /// </summary>
        /// <returns></returns>
        public Int32 GetMaxSize(System.Drawing.Font f = null, System.Drawing.Font fLabel = null)
        {

            if (fLabel == null)
                fLabel = new System.Drawing.Font("Verdana", 8, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);



            return Math.Max(eTools.MesureString(ColMaxValue, f), eTools.MesureString(HeadValue, fLabel)); ;
        }


    }





    /// <summary>
    /// Type de renderer
    /// </summary>
    public enum RENDERERTYPE
    {
        /// <summary>Pas début</summary>
        UNDEFINED = 0,

        // Renderer "de base"

        /// <summary>
        /// Render master
        /// </summary>
        Renderer,

        /// <summary>
        /// Renderer pour un champ
        /// </summary>
        Field,

        // Heritage direct de eRender
        /// <summary>
        /// Renderer pour les mode fiche 'standard'
        /// </summary>
        MainFile,

        /// <summary>
        /// Mode liste principale
        /// </summary>
        ListRendererMain,

        /// <summary>
        /// Wizard des rapports
        /// </summary>
        ReportWizard,

        /// <summary>Wizard pour l'emailing</summary>
        MailingWizard,

        /// <summary>Wizard des invitations</summary>
        SelectInvitWizard,

        /// <summary>Wizard des rapports graphiques</summary>
        ChartWizard,

        /// <summary>Wizard des formulaires</summary>
        FormularWizard,

        /// <summary>squelette du mode fiche </summary>
        FileBackBone,

        /// <summary>informations des parents en en-tete de fiche </summary>
        FileParentInHead,

        /// <summary>
        /// Rendu des VCards
        /// </summary>
        VCardFile,

        /// <summary>Note en signet</summary>
        NotesBookmark,

        // Hérite de  eMainFileRenderer
        /// <summary>Visualisation d'un mail envoyé (lecture seule)</summary>
        MailFile,
        /// <summary>Fiche planning</summary>
        PlanningFile,
        /// <summary>mode impression </summary>
        PrintFile,
        /// <summary>Mode Edition</summary>
        EditFile,
        /// <summary>Mode Edition mais l'objet de données n'a pas de signets chargés </summary>
        EditFileLite,
        /// <summary>Edition d'un mail (Créa/Modif)</summary>
        EditMail,
        /// <summary>Edition du mail à envoyer dans le cadre de l'assistant d'Emailing</summary>
        EditMailing,



        // eListRendererMain

        /// <summary>Finder</summary>
        Finder,
        /// <summary>Selection du Finder (multiple)</summary>
        FinderSelection,

        /// <summary>Signets</summary>
        Bookmark,

        /// <summary>Liste des filtres et des rapports</summary>
        FilterReportList,

        /// <summary>Liste des annexes</summary>
        PjList,

        /// <summary>informations des parents en bas de fiche (mode popup) </summary>
        FileParentInFoot,


        /// <summary>Flux XML pour les charts</summary>
        ChartXML,

        /// <summary>Liste des rapports utilisateur</summary>
        UserReportList,


        /// <summary>Liste des modeles d'emailing</summary>
        UserMailTemplateList,

        /// <summary>
        /// Liste de selection d'invité pour les ++
        /// </summary>
        ListInvit,

        /// <summary>Signet de typep page Web </summary>
        WebBookmark,
        /// <summary>Signet de typep page Web </summary>
        WebTab,

        /// <summary>Formulaire fiche</summary>
        FormularFile,
        /// <summary>Liste des pjs depuis un template</summary>
        ListPjFromTpl,

        /// <summary>
        /// Renderer racine pour les pages d'administration/options utilisateur
        /// </summary>
        Admin,

        /// <summary>Fenêtre des droits de traitements</summary>
        TreatmentRights,

        /// <summary>Liste des selections user</summary>
        ListSelection,


        /// <summary>
        /// Admin des onglets
        /// </summary>
        AdminFile,

        /// <summary>
        /// Administration des users
        /// </summary>
        AdminUserList,

        /// <summary>
        /// Main liste complète
        /// </summary>
        FullMainList,

        /// <summary>
        /// Liste des automatismes (bien que je deteste cette façon de faire :p )
        /// </summary>
        AutomationList,
        /// <summary>
        /// Rendu Html du fusion chart
        /// </summary>
        FusionChartHtml,
        /// <summary>
        /// Rendu Html des modèles d'import
        /// </summary>
        ImportTemplate,

        /// <summary>Visualisation d'un SMS envoyé (lecture seule)</summary>
        SMSFile,
        /// <summary>Edition d'un SMS (Créa/Modif)</summary>
        EditSMS,
        /// <summary>Edition du SMS à envoyer dans le cadre de l'assistant de SMSing</summary>
        EditSMSMailing
    }
}

